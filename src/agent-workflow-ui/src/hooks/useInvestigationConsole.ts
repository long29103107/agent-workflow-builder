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
const defaultWorkspaceId = "workspace-default";

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

export type WorkspaceProject = {
  id: string;
  name: string;
  apiKey: string;
  generatedTasks: TaskItem[];
  localScheduledTasks: ScheduledTask[];
  requestHistory: RequestEntry[];
  requestText: string;
  plannerLogs: PlannerLog[];
  repoPath: string;
  repoProvider: string;
  repoUrl: string;
  selectedTaskId: string | null;
};

export type { PlannerStep };

export function useInvestigationConsole() {
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [run, setRun] = useState<WorkflowRun | null>(null);
  const [events, setEvents] = useState<WorkflowEvent[]>([]);
  const [isLoadingTasks, setIsLoadingTasks] = useState(true);
  const [isInvestigating, setIsInvestigating] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);
  const [apiScheduledTasks, setApiScheduledTasks] = useState<ScheduledTask[]>([]);
  const [isLoadingSchedule, setIsLoadingSchedule] = useState(true);
  const [isQueueingTask, setIsQueueingTask] = useState(false);
  const [isProcessingNext, setIsProcessingNext] = useState(false);
  const [workspaces, setWorkspaces] = useState<WorkspaceProject[]>(() => [createDefaultWorkspace()]);
  const [activeWorkspaceId, setActiveWorkspaceId] = useState(defaultWorkspaceId);

  const activeWorkspace = useMemo(
    () => workspaces.find((workspace) => workspace.id === activeWorkspaceId) ?? workspaces[0],
    [activeWorkspaceId, workspaces]
  );
  const backlogTasks = useMemo(
    () => [...activeWorkspace.generatedTasks, ...tasks],
    [activeWorkspace.generatedTasks, tasks]
  );
  const selectedTask = useMemo(
    () => backlogTasks.find((task) => task.id === activeWorkspace.selectedTaskId) ?? null,
    [activeWorkspace.selectedTaskId, backlogTasks]
  );
  const plannerSteps = useMemo(
    () => createPlannerSteps(activeWorkspace.requestText, selectedTask),
    [activeWorkspace.requestText, selectedTask]
  );
  const scheduledTasks = useMemo(
    () => [...activeWorkspace.localScheduledTasks, ...apiScheduledTasks],
    [activeWorkspace.localScheduledTasks, apiScheduledTasks]
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
        repositoryPath: activeWorkspace.repoPath,
        repositoryUrl: activeWorkspace.repoUrl,
        repositoryProvider: activeWorkspace.repoProvider
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
      const nextRun = await startWorkflowInvestigation(selectedTask.id, activeWorkspace.repoPath, activeWorkspace.repoUrl);
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
        updateActiveWorkspace((workspace) => ({
          localScheduledTasks: [
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
            ...workspace.localScheduledTasks
          ],
          generatedTasks: workspace.generatedTasks.filter((task) => task.id !== selectedTask.id),
          selectedTaskId: null
        }));
        return;
      }

      await enqueueScheduledTask(selectedTask.id, activeWorkspace.repoPath, activeWorkspace.repoUrl);
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
      const nextLocalTask = activeWorkspace.localScheduledTasks.find((task) => task.status === "Queued");
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
      const localTask = activeWorkspace.localScheduledTasks.find((task) => task.id === scheduledTaskId && task.status === "Queued");
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
    updateActiveWorkspace((workspace) => ({
      localScheduledTasks: workspace.localScheduledTasks.map((task) =>
        task.id === scheduledTaskId
          ? { ...task, status: "Processing", startedAt: new Date().toISOString() }
          : task
      )
    }));
  }

  function applySettings(settings: ToolEndpointSettings) {
    updateActiveWorkspace(() => ({
      repoPath: settings.repositoryPath,
      repoUrl: settings.repositoryUrl,
      repoProvider: settings.repositoryProvider
    }));
    setJiraEndpoint(settings.jiraMcpEndpoint);
    setNotionEndpoint(settings.notionMcpEndpoint);
  }

  function submitRequest() {
    const content = activeWorkspace.requestText.trim();
    if (!content) return false;

    const requestId = crypto.randomUUID();
    const createdAt = new Date().toISOString();
    const steps = createPlannerSteps(content, null);
    updateActiveWorkspace((workspace) => ({
      requestHistory: [
        {
          id: requestId,
          content,
          createdAt
        },
        ...workspace.requestHistory
      ],
      plannerLogs: [
        {
          id: crypto.randomUUID(),
          requestId,
          request: content,
          createdAt,
          status: "PendingApproval",
          steps
        },
        ...workspace.plannerLogs
      ],
      requestText: ""
    }));
    return true;
  }

  function approvePlannerLog(plannerLogId: string) {
    const plannerLog = activeWorkspace.plannerLogs.find((log) => log.id === plannerLogId);
    if (!plannerLog || plannerLog.status === "Approved") return;

    updateActiveWorkspace((workspace) => ({
      plannerLogs: workspace.plannerLogs.map((log) =>
        log.id === plannerLogId ? { ...log, status: "Approved" } : log
      ),
      generatedTasks: [...createTasksFromPlannerLog(plannerLog), ...workspace.generatedTasks]
    }));
  }

  function createWorkspace() {
    const workspaceNumber = workspaces.length + 1;
    const workspace = createWorkspaceProject(`Project ${workspaceNumber}`);
    setWorkspaces((current) => [...current, workspace]);
    setActiveWorkspaceId(workspace.id);
  }

  function updateActiveWorkspace(updater: (workspace: WorkspaceProject) => Partial<WorkspaceProject>) {
    setWorkspaces((current) =>
      current.map((workspace) =>
        workspace.id === activeWorkspaceId ? { ...workspace, ...updater(workspace) } : workspace
      )
    );
  }

  function setWorkspaceValue<K extends keyof WorkspaceProject>(key: K, value: WorkspaceProject[K]) {
    updateActiveWorkspace(() => ({ [key]: value } as Pick<WorkspaceProject, K>));
  }

  return {
    activeWorkspace,
    activeWorkspaceId,
    error,
    events,
    apiKey: activeWorkspace.apiKey,
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
    repoProvider: activeWorkspace.repoProvider,
    repoPath: activeWorkspace.repoPath,
    repoUrl: activeWorkspace.repoUrl,
    requestText: activeWorkspace.requestText,
    requestHistory: activeWorkspace.requestHistory,
    run,
    scheduledTasks,
    saveSettings,
    selectedTask,
    approvePlannerLog,
    backlogTasks,
    createWorkspace,
    processNextTask,
    queueSelectedTask,
    plannerSteps,
    plannerLogs: activeWorkspace.plannerLogs,
    setActiveWorkspaceId,
    setApiKey: (value: string) => setWorkspaceValue("apiKey", value),
    setJiraEndpoint,
    setNotionEndpoint,
    setRepoPath: (value: string) => setWorkspaceValue("repoPath", value),
    setRepoProvider: (value: string) => setWorkspaceValue("repoProvider", value),
    setRepoUrl: (value: string) => setWorkspaceValue("repoUrl", value),
    setRequestText: (value: string) => setWorkspaceValue("requestText", value),
    setSelectedTaskId: (value: string | null) => setWorkspaceValue("selectedTaskId", value),
    settingsMessage,
    startInvestigation,
    startQueuedTask,
    submitRequest,
    tasks,
    workspaces
  };
}

function createWorkspaceProject(name: string): WorkspaceProject {
  return {
    id: crypto.randomUUID(),
    name,
    apiKey: "",
    generatedTasks: [],
    localScheduledTasks: [],
    requestHistory: [],
    requestText: "",
    plannerLogs: [],
    repoPath: "",
    repoProvider: "github",
    repoUrl: "",
    selectedTaskId: null
  };
}

function createDefaultWorkspace(): WorkspaceProject {
  return {
    ...createWorkspaceProject("Project Alpha"),
    id: defaultWorkspaceId
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
