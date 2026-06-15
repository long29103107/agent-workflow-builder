import { useEffect, useMemo, useState } from "react";
import {
  fetchSettings,
  fetchTasks,
  fetchWorkflowEvents,
  startWorkflowInvestigation,
  updateSettings
} from "../api/client";
import type { TaskItem, ToolEndpointSettings, WorkflowEvent, WorkflowRun } from "../types/workflow";

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
  const [jiraEndpoint, setJiraEndpoint] = useState("mock://jira");
  const [notionEndpoint, setNotionEndpoint] = useState("mock://notion");
  const [isSavingSettings, setIsSavingSettings] = useState(false);
  const [settingsMessage, setSettingsMessage] = useState<string | null>(null);

  const selectedTask = useMemo(
    () => tasks.find((task) => task.id === selectedTaskId) ?? null,
    [selectedTaskId, tasks]
  );

  useEffect(() => {
    void loadTasks();
    void loadSettings();
  }, []);

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
        repositoryPath: repoPath
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
      const nextRun = await startWorkflowInvestigation(selectedTask.id, repoPath);
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
    setJiraEndpoint(settings.jiraMcpEndpoint);
    setNotionEndpoint(settings.notionMcpEndpoint);
  }

  return {
    error,
    events,
    isInvestigating,
    isLoadingTasks,
    isSavingSettings,
    jiraEndpoint,
    loadTasks,
    notionEndpoint,
    repoPath,
    run,
    saveSettings,
    selectedTask,
    setJiraEndpoint,
    setNotionEndpoint,
    setRepoPath,
    setSelectedTaskId,
    settingsMessage,
    startInvestigation,
    tasks
  };
}
