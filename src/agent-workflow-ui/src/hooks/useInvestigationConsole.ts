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
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);
  const [scheduledTasks, setScheduledTasks] = useState<ScheduledTask[]>([]);
  const [isLoadingSchedule, setIsLoadingSchedule] = useState(true);
  const [isQueueingTask, setIsQueueingTask] = useState(false);
  const [isProcessingNext, setIsProcessingNext] = useState(false);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) ?? null,
    [selectedTaskId, tasks]
  );

  useEffect(() => {
    void loadTasks();
    void loadSettings();
    void loadScheduledTasks();
  }, []);

  async function loadScheduledTasks() {
    setIsLoadingSchedule(true);

    try {
      setScheduledTasks(await fetchScheduledTasks());
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

  function applySettings(settings: ToolEndpointSettings) {
    setRepoPath(settings.repositoryPath);
    setRepoUrl(settings.repositoryUrl);
    setRepoProvider(settings.repositoryProvider);
    setJiraEndpoint(settings.jiraMcpEndpoint);
    setNotionEndpoint(settings.notionMcpEndpoint);
  }

  return {
    error,
    events,
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
    run,
    scheduledTasks,
    saveSettings,
    selectedTask,
    processNextTask,
    queueSelectedTask,
    setJiraEndpoint,
    setNotionEndpoint,
    setRepoPath,
    setRepoProvider,
    setRepoUrl,
    setSelectedTaskId,
    settingsMessage,
    startInvestigation,
    tasks
  };
}
