import { useEffect, useMemo, useState } from "react";
import {
  enqueueScheduledTask,
  fetchSettings,
  fetchScheduledTasks,
  fetchTasks,
  fetchWorkflowEvents,
  fetchWorkflowRun,
  processNextScheduledTask,
  startWorkflowInvestigation,
  updateSettings
} from "../api/client";
import type {
  ScheduledTask,
  TaskItem,
  ToolEndpointSettings,
  WorkflowEvent,
  WorkflowRun
} from "../types/workflow";

const fallbackMessage = "Using local mock settings.";

type PlannerStep = {
  title: string;
  detail: string;
  owner: string;
};

export type RequestEntry = {
  id: string;
  content: string;
  createdAt: string;
};

export type PlannerLog = {
  id: string;
  requestId: string;
  request: string;
  createdAt: string;
  status: "PendingApproval" | "Approved";
  steps: PlannerStep[];
};

export type { PlannerStep };

export function useInvestigationConsole() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [run, setRun] = useState<WorkflowRun | null>(null);
  const [events, setEvents] = useState<WorkflowEvent[]>([]);
  const [isLoadingTasks, setIsLoadingTasks] = useState(true);
  const [isInvestigating, setIsInvestigating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [repoPath, setRepoPath] = useState("");
  const [repoUrl, setRepoUrl] = useState("");
  const [repoProvider, setRepoProvider] = useState("github");
  const [apiKey, setApiKey] = useState("");
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);
  const [apiScheduledTasks, setApiScheduledTasks] = useState<ScheduledTask[]>([]);
  const [localScheduledTasks, setLocalScheduledTasks] = useState<ScheduledTask[]>([]);
  const [generatedTasks, setGeneratedTasks] = useState<TaskItem[]>([]);
  const [isLoadingSchedule, setIsLoadingSchedule] = useState(true);
  const [isQueueingTask, setIsQueueingTask] = useState(false);
  const [isProcessingNext, setIsProcessingNext] = useState(false);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) ?? null,
    [selectedTaskId, tasks]
  );
  const [requestText, setRequestText] = useState("");
  const [requestHistory, setRequestHistory] = useState<RequestEntry[]>([]);
  const [plannerLogs, setPlannerLogs] = useState<PlannerLog[]>([]);
  const plannerSteps = useMemo(() => createPlannerSteps(requestText, selectedTask), [requestText, selectedTask]);
  const backlogTasks = useMemo(() => [...generatedTasks, ...tasks], [generatedTasks, tasks]);
  const scheduledTasks = useMemo(
    () => [...localScheduledTasks, ...apiScheduledTasks],
    [apiScheduledTasks, localScheduledTasks]
  );

  useEffect(() => {
    void loadTasks();
    void loadSettings();
    void loadScheduledTasks();
  }, []);

  async function loadScheduledTasks() {
    setIsLoadingSchedule(true);

    try {
      setApiScheduledTasks(await fetchScheduledTasks());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load scheduler queue.");
    } finally {
      setIsLoadingSchedule(false);
    }
  }

  async function loadTasks() {
    setIsLoadingTasks(true);
    setError(null);

    try {
      setTasks(await fetchTasks());
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load tasks.");
    } finally {
      setIsLoadingTasks(false);
    }
  }

  async function loadSettings() {
    try {
      const settings = await fetchSettings();
      if (!settings) return;

      applySettings(settings);
    } catch {
      setSettingsMessage(fallbackMessage);
    }
  }

  async function saveSettings() {
    setIsSavingSettings(true);
    setSettingsMessage(null);

    try {
      const settings = await updateSettings({
        jiraMcpEndpoint: jiraEndpoint,
        notionMcpEndpoint: notionEndpoint,
        repositoryPath: repoPath,
        repositoryUrl: repoUrl,
        repositoryProvider: repoProvider
      });
      applySettings(settings);
      setSettingsMessage("Settings saved for this API session.");
    } catch (err) {
      setSettingsMessage(err instanceof Error ? err.message : "Could not save settings.");
    } finally {
      setIsSavingSettings(false);
    }
  }

  async function startInvestigation() {
    if (!selectedTask) return;

    setIsInvestigating(true);
    setError(null);
    setRun(null);
    setEvents([]);

    try {
      const nextRun = await startWorkflowInvestigation(selectedTask.id, repoPath, repoUrl);
      setRun(nextRun);
      setEvents(await fetchWorkflowEvents(nextRun.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Investigation failed.");
    } finally {
      setIsInvestigating(false);
    }
  }

  async function queueSelectedTask() {
    if (!selectedTask) return;

    setIsQueueingTask(true);
    setError(null);

    try {
      if (selectedTask.source === "agent-planner") {
        setLocalScheduledTasks((current) => [
          {
            id: crypto.randomUUID(),
            taskId: selectedTask.id,
            taskTitle: selectedTask.title,
            priority: "Medium",
            status: "Queued",
            queuedAt: new Date().toISOString(),
            startedAt: null,
            completedAt: null,
            workflowRunId: null,
            error: null
          },
          ...current
        ]);
        setGeneratedTasks((current) => current.filter((task) => task.id !== selectedTask.id));
        setSelectedTaskId(null);
        return;
      }

      await enqueueScheduledTask(selectedTask.id, repoPath, repoUrl);
      await loadScheduledTasks();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not queue task.");
    } finally {
      setIsQueueingTask(false);
    }
  }

  async function processNextTask() {
    setIsProcessingNext(true);
    setError(null);

    try {
      const nextLocalTask = localScheduledTasks.find((task) => task.status === "Queued");
      if (nextLocalTask) {
        moveLocalTaskToProcessing(nextLocalTask.id);
        return;
      }

      const processed = await processNextScheduledTask();
      await loadScheduledTasks();

      if (processed.workflowRunId) {
        const nextRun = await fetchWorkflowRun(processed.workflowRunId);
        setRun(nextRun);
        setEvents(await fetchWorkflowEvents(nextRun.id));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not process scheduled task.");
    } finally {
      setIsProcessingNext(false);
    }
  }

  async function startQueuedTask(scheduledTaskId: string) {
    setIsProcessingNext(true);
    setError(null);

    try {
      const localTask = localScheduledTasks.find((task) => task.id === scheduledTaskId && task.status === "Queued");
      if (localTask) {
        moveLocalTaskToProcessing(localTask.id);
        return;
      }

      const processed = await processNextScheduledTask();
      await loadScheduledTasks();

      if (processed.workflowRunId) {
        const nextRun = await fetchWorkflowRun(processed.workflowRunId);
        setRun(nextRun);
        setEvents(await fetchWorkflowEvents(nextRun.id));
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not start scheduled task.");
    } finally {
      setIsProcessingNext(false);
    }
  }

  function moveLocalTaskToProcessing(scheduledTaskId: string) {
    setLocalScheduledTasks((current) =>
      current.map((task) =>
        task.id === scheduledTaskId
          ? { ...task, status: "Processing", startedAt: new Date().toISOString() }
          : task
      )
    );
  }

  function applySettings(settings: ToolEndpointSettings) {
    setRepoPath(settings.repositoryPath);
    setRepoUrl(settings.repositoryUrl);
    setRepoProvider(settings.repositoryProvider);
    setJiraEndpoint(settings.jiraMcpEndpoint);
    setNotionEndpoint(settings.notionMcpEndpoint);
  }

  function submitRequest() {
    const content = requestText.trim();
    if (!content) return false;

    const requestId = crypto.randomUUID();
    const createdAt = new Date().toISOString();
    const steps = createPlannerSteps(content, null);
    setRequestHistory((current) => [
      {
        id: requestId,
        content,
        createdAt
      },
      ...current
    ]);
    setPlannerLogs((current) => [
      {
        id: crypto.randomUUID(),
        requestId,
        request: content,
        createdAt,
        status: "PendingApproval",
        steps
      },
      ...current
    ]);
    setRequestText("");
    return true;
  }

  function approvePlannerLog(plannerLogId: string) {
    const plannerLog = plannerLogs.find((log) => log.id === plannerLogId);
    if (!plannerLog || plannerLog.status === "Approved") return;

    setPlannerLogs((current) =>
      current.map((log) => (log.id === plannerLogId ? { ...log, status: "Approved" } : log))
    );
    setGeneratedTasks((current) => [...createTasksFromPlannerLog(plannerLog), ...current]);
  }

  return {
    error,
    events,
    apiKey,
    isInvestigating,
    isLoadingSchedule,
    isLoadingTasks,
    isProcessingNext,
    isQueueingTask,
    isSavingSettings,
    jiraEndpoint,
    loadTasks,
    loadScheduledTasks,
    notionEndpoint,
    repoProvider,
    repoPath,
    repoUrl,
    requestText,
    requestHistory,
    run,
    scheduledTasks,
    saveSettings,
    selectedTask,
    approvePlannerLog,
    backlogTasks,
    processNextTask,
    queueSelectedTask,
    plannerSteps,
    plannerLogs,
    setApiKey,
    setJiraEndpoint,
    setNotionEndpoint,
    setRepoPath,
    setRepoProvider,
    setRepoUrl,
    setRequestText,
    setSelectedTaskId,
    settingsMessage,
    startInvestigation,
    startQueuedTask,
    submitRequest,
    tasks
  };
}

function createTasksFromPlannerLog(plannerLog: PlannerLog): TaskItem[] {
  return plannerLog.steps.map((step, index) => ({
    id: `planner-${plannerLog.id}-${index + 1}`,
    source: "agent-planner",
    key: `PLAN-${index + 1}`,
    title: step.title,
    description: step.detail,
    status: "Backlog",
    priority: index === 0 ? "High" : "Medium",
    tags: [step.owner, "planner"]
  }));
}

function createPlannerSteps(requestText: string, selectedTask: TaskItem | null): PlannerStep[] {
  const trimmedRequest = requestText.trim();
  const requestFocus = trimmedRequest.length > 0
    ? trimmedRequest
    : "Clarify the user request and define the implementation target.";
  const taskFocus = selectedTask
    ? `${selectedTask.key}: ${selectedTask.title}`
    : "Select or create a work item for execution.";

  return [
    {
      title: "Capture request",
      detail: requestFocus,
      owner: "Request intake"
    },
    {
      title: "Ground in work item",
      detail: taskFocus,
      owner: "Agent planner"
    },
    {
      title: "Plan execution",
      detail: "Break the request into repository investigation, implementation, verification, and review slices.",
      owner: "Lead agent"
    },
    {
      title: "Prepare processing",
      detail: "Queue the selected task, process the next priority item, then inspect run output and agent activity.",
      owner: "Scheduler"
    }
  ];
}
