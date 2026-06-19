import type {
  ScheduledTask,
  TaskItem,
  ToolEndpointSettings,
  WorkflowEvent,
  WorkflowRun
} from "../types/workflow";

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5275/api";

async function readJson<T>(response: Response, fallbackError: string): Promise<T> {
  if (!response.ok) {
    throw new Error(`${fallbackError} returned ${response.status}`);
  }

  return response.json() as Promise<T>;
}

export async function fetchTasks(): Promise<TaskItem[]> {
  const response = await fetch(`${apiBaseUrl}/tasks`);
  return readJson<TaskItem[]>(response, "Task API");
}

export async function fetchScheduledTasks(): Promise<ScheduledTask[]> {
  const response = await fetch(apiBaseUrl + "/scheduler/tasks");
  return readJson<ScheduledTask[]>(response, "Scheduler API");
}

export async function enqueueScheduledTask(
  taskId: string,
  repositoryPath: string,
  repositoryUrl: string
): Promise<ScheduledTask> {
  const response = await fetch(apiBaseUrl + "/scheduler/tasks", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      taskId,
      priority: null,
      repositoryPath,
      repositoryUrl
    })
  });

  return readJson<ScheduledTask>(response, "Scheduler enqueue");
}

export async function processNextScheduledTask(): Promise<ScheduledTask> {
  const response = await fetch(apiBaseUrl + "/scheduler/process-next", {
    method: "POST"
  });

  return readJson<ScheduledTask>(response, "Scheduler process");
}

export async function fetchWorkflowRun(runId: string): Promise<WorkflowRun> {
  const response = await fetch(apiBaseUrl + "/workflows/" + runId);
  return readJson<WorkflowRun>(response, "Workflow API");
}

export async function fetchSettings(): Promise<ToolEndpointSettings | null> {
  const response = await fetch(`${apiBaseUrl}/settings`);
  if (!response.ok) return null;

  return response.json() as Promise<ToolEndpointSettings>;
}

export async function updateSettings(settings: ToolEndpointSettings): Promise<ToolEndpointSettings> {
  const response = await fetch(`${apiBaseUrl}/settings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(settings)
  });

  return readJson<ToolEndpointSettings>(response, "Settings API");
}

export async function startWorkflowInvestigation(
  taskId: string,
  repositoryPath: string,
  repositoryUrl: string
): Promise<WorkflowRun> {
  const response = await fetch(`${apiBaseUrl}/workflows/investigate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      taskId,
      repositoryPath,
      repositoryUrl,
      requestedAgents: []
    })
  });

  return readJson<WorkflowRun>(response, "Investigation API");
}

export async function fetchWorkflowEvents(runId: string): Promise<WorkflowEvent[]> {
  const response = await fetch(`${apiBaseUrl}/workflows/${runId}/events`);
  if (!response.ok) return [];

  return response.json() as Promise<WorkflowEvent[]>;
}
