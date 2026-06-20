import { useEffect, useMemo, useState } from "react";
import {
  approvePlannerLog as approvePlannerLogRequest,
  createWorkspace as createWorkspaceRequest,
  enqueueWorkspaceTask,
  fetchPlannerLogs,
  fetchWorkspaceRequests,
  fetchWorkspaceScheduledTasks,
  fetchWorkspaceSettings,
  fetchWorkspaceTasks,
  fetchWorkspaces,
  fetchWorkflowEvents,
  fetchWorkflowRun,
  processNextWorkspaceTask,
  startWorkflowInvestigation,
  submitWorkspaceRequest,
  updateWorkspaceSettings
} from "../api/client";
import type {
  PlannerLog,
  ScheduledTask,
  TaskItem,
  ToolEndpointSettings,
  WorkspaceProject,
  WorkspaceUserRequest,
  WorkflowEvent,
  WorkflowRun
} from "../types/workflow";

const fallbackMessage = "Could not load workspace settings.";

export function useInvestigationConsole() {
  const [workspaces, setWorkspaces] = useState<WorkspaceProject[]>([]);
  const [activeWorkspaceId, setActiveWorkspaceId] = useState("");
  const [tasks, setTasks] = useState<TaskItem[]>([]);
  const [scheduledTasks, setScheduledTasks] = useState<ScheduledTask[]>([]);
  const [requestHistory, setRequestHistory] = useState<WorkspaceUserRequest[]>([]);
  const [plannerLogs, setPlannerLogs] = useState<PlannerLog[]>([]);
  const [selectedTaskId, setSelectedTaskId] = useState<string | null>(null);
  const [requestDrafts, setRequestDrafts] = useState<Record<string, string>>({});
  const [apiKeys, setApiKeys] = useState<Record<string, string>>({});
  const [run, setRun] = useState<WorkflowRun | null>(null);
  const [events, setEvents] = useState<WorkflowEvent[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [repoPath, setRepoPath] = useState("");
  const [repoUrl, setRepoUrl] = useState("");
  const [repoProvider, setRepoProvider] = useState("github");
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isLoadingTasks, setIsLoadingTasks] = useState(true);
  const [isLoadingSchedule, setIsLoadingSchedule] = useState(true);
  const [isInvestigating, setIsInvestigating] = useState(false);
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [isQueueingTask, setIsQueueingTask] = useState(false);
  const [isProcessingNext, setIsProcessingNext] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);

  const activeWorkspace = useMemo(
    () => workspaces.find((workspace) => workspace.id === activeWorkspaceId) ?? null,
    [activeWorkspaceId, workspaces]
  );
  const scheduledTaskIds = useMemo(
    () => new Set(scheduledTasks.map((task) => task.taskId)),
    [scheduledTasks]
  );
  const backlogTasks = useMemo(
    () => tasks.filter((task) => !scheduledTaskIds.has(task.id)),
    [scheduledTaskIds, tasks]
  );
  const selectedTask = useMemo(
    () => backlogTasks.find((task) => task.id === selectedTaskId) ?? null,
    [backlogTasks, selectedTaskId]
  );
  const plannerSteps = plannerLogs[0]?.steps ?? [];
  const requestText = requestDrafts[activeWorkspaceId] ?? "";
  const apiKey = apiKeys[activeWorkspaceId] ?? "";

  useEffect(() => {
    void initializeWorkspaces();
  }, []);

  useEffect(() => {
    if (!activeWorkspaceId) return;

    setSelectedTaskId(null);
    setRun(null);
    setEvents([]);
    void loadWorkspaceData(activeWorkspaceId);
  }, [activeWorkspaceId]);

  async function initializeWorkspaces() {
    setError(null);
    try {
      const result = await fetchWorkspaces();
      setWorkspaces(result);
      setActiveWorkspaceId((current) => current || result[0]?.id || "");
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load workspaces.");
      setIsLoadingTasks(false);
      setIsLoadingSchedule(false);
    }
  }

  async function loadWorkspaceData(workspaceId: string) {
    setIsLoadingTasks(true);
    setIsLoadingSchedule(true);
    setError(null);

    try {
      const [workspaceTasks, workspaceSchedule, requests, logs, settings] = await Promise.all([
        fetchWorkspaceTasks(workspaceId),
        fetchWorkspaceScheduledTasks(workspaceId),
        fetchWorkspaceRequests(workspaceId),
        fetchPlannerLogs(workspaceId),
        fetchWorkspaceSettings(workspaceId)
      ]);
      setTasks(workspaceTasks);
      setScheduledTasks(workspaceSchedule);
      setRequestHistory(requests);
      setPlannerLogs(logs);
      applySettings(settings);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load workspace data.");
    } finally {
      setIsLoadingTasks(false);
      setIsLoadingSchedule(false);
    }
  }

  async function loadTasks() {
    if (!activeWorkspaceId) return;
    setIsLoadingTasks(true);
    try {
      setTasks(await fetchWorkspaceTasks(activeWorkspaceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load workspace tasks.");
    } finally {
      setIsLoadingTasks(false);
    }
  }

  async function loadScheduledTasks() {
    if (!activeWorkspaceId) return;
    setIsLoadingSchedule(true);
    try {
      setScheduledTasks(await fetchWorkspaceScheduledTasks(activeWorkspaceId));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not load workspace scheduler.");
    } finally {
      setIsLoadingSchedule(false);
    }
  }

  async function createWorkspace() {
    setError(null);
    try {
      const workspace = await createWorkspaceRequest(`Project ${workspaces.length + 1}`);
      setWorkspaces((current) => [...current, workspace]);
      setActiveWorkspaceId(workspace.id);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not create workspace.");
    }
  }

  async function submitRequest() {
    const content = requestText.trim();
    if (!activeWorkspaceId || !content) return false;

    setError(null);
    try {
      const result = await submitWorkspaceRequest(activeWorkspaceId, content);
      setRequestHistory((current) => [result.request, ...current]);
      setPlannerLogs((current) => [result.plannerLog, ...current]);
      setRequestText("");
      return true;
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not submit request.");
      return false;
    }
  }

  async function approvePlannerLog(plannerLogId: string) {
    if (!activeWorkspaceId) return;

    setError(null);
    try {
      const result = await approvePlannerLogRequest(activeWorkspaceId, plannerLogId);
      setPlannerLogs((current) =>
        current.map((log) => (log.id === result.plannerLog.id ? result.plannerLog : log))
      );
      await loadTasks();
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not approve planner log.");
    }
  }

  async function queueSelectedTask() {
    if (!activeWorkspaceId || !selectedTask) return;

    setIsQueueingTask(true);
    setError(null);
    try {
      await enqueueWorkspaceTask(
        activeWorkspaceId,
        selectedTask.id,
        repoPath,
        repoUrl
      );
      setSelectedTaskId(null);
      await Promise.all([loadTasks(), loadScheduledTasks()]);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not queue task.");
    } finally {
      setIsQueueingTask(false);
    }
  }

  async function processNextTask() {
    if (!activeWorkspaceId) return;

    setIsProcessingNext(true);
    setError(null);
    try {
      const processed = await processNextWorkspaceTask(activeWorkspaceId);
      await loadScheduledTasks();
      await loadRun(processed.workflowRunId);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Could not process scheduled task.");
    } finally {
      setIsProcessingNext(false);
    }
  }

  async function startQueuedTask(_scheduledTaskId: string) {
    await processNextTask();
  }

  async function loadRun(workflowRunId: string | null) {
    if (!workflowRunId) return;

    const nextRun = await fetchWorkflowRun(workflowRunId);
    setRun(nextRun);
    setEvents(await fetchWorkflowEvents(nextRun.id));
  }

  async function saveSettings() {
    if (!activeWorkspaceId) return;

    setIsSavingSettings(true);
    setSettingsMessage(null);
    try {
      const settings = await updateWorkspaceSettings(activeWorkspaceId, {
        jiraMcpEndpoint: jiraEndpoint,
        notionMcpEndpoint: notionEndpoint,
        repositoryPath: repoPath,
        repositoryUrl: repoUrl,
        repositoryProvider: repoProvider
      });
      applySettings(settings);
      setWorkspaces((current) =>
        current.map((workspace) =>
          workspace.id === activeWorkspaceId
            ? {
                ...workspace,
                repositoryPath: settings.repositoryPath,
                repositoryUrl: settings.repositoryUrl,
                repositoryProvider: settings.repositoryProvider
              }
            : workspace
        )
      );
      setSettingsMessage("Workspace settings saved.");
    } catch (err) {
      setSettingsMessage(err instanceof Error ? err.message : fallbackMessage);
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
      const nextRun = await startWorkflowInvestigation(
        selectedTask.id,
        repoPath,
        repoUrl,
        activeWorkspaceId
      );
      setRun(nextRun);
      setEvents(await fetchWorkflowEvents(nextRun.id));
    } catch (err) {
      setError(err instanceof Error ? err.message : "Investigation failed.");
    } finally {
      setIsInvestigating(false);
    }
  }

  function applySettings(settings: ToolEndpointSettings) {
    setRepoPath(settings.repositoryPath);
    setRepoUrl(settings.repositoryUrl);
    setRepoProvider(settings.repositoryProvider);
    setJiraEndpoint(settings.jiraMcpEndpoint);
    setNotionEndpoint(settings.notionMcpEndpoint);
  }

  function setRequestText(value: string) {
    if (!activeWorkspaceId) return;
    setRequestDrafts((current) => ({ ...current, [activeWorkspaceId]: value }));
  }

  function setApiKey(value: string) {
    if (!activeWorkspaceId) return;
    setApiKeys((current) => ({ ...current, [activeWorkspaceId]: value }));
  }

  return {
    activeWorkspace,
    activeWorkspaceId,
    apiKey,
    approvePlannerLog,
    backlogTasks,
    createWorkspace,
    error,
    events,
    isInvestigating,
    isLoadingSchedule,
    isLoadingTasks,
    isProcessingNext,
    isQueueingTask,
    isSavingSettings,
    jiraEndpoint,
    loadScheduledTasks,
    loadTasks,
    notionEndpoint,
    plannerLogs,
    plannerSteps,
    processNextTask,
    queueSelectedTask,
    repoPath,
    repoProvider,
    repoUrl,
    requestHistory,
    requestText,
    run,
    saveSettings,
    scheduledTasks,
    selectedTask,
    setActiveWorkspaceId,
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
    tasks,
    workspaces
  };
}
