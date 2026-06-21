import type {
  PlannerApprovalResult,
  PlannerLog,
  RequestSubmissionResult,
  ScheduledTask,
  TaskItem,
  ToolEndpointSettings,
  WorkspaceProject,
  WorkspaceUserRequest,
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

export async function fetchWorkspaces(): Promise<WorkspaceProject[]> {
  const response = await fetch(apiBaseUrl + "/workspaces");
  return readJson<WorkspaceProject[]>(response, "Workspace API");
}

export async function createWorkspace(name: string, code: string): Promise<WorkspaceProject> {
  const response = await fetch(apiBaseUrl + "/workspaces", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      name,
      code,
      repositoryPath: "",
      repositoryUrl: "",
      repositoryProvider: "github"
    })
  });
  return readJson<WorkspaceProject>(response, "Workspace create");
}

export async function fetchWorkspaceRequests(workspaceId: string): Promise<WorkspaceUserRequest[]> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/requests`);
  return readJson<WorkspaceUserRequest[]>(response, "Workspace requests");
}

export async function submitWorkspaceRequest(
  workspaceId: string,
  content: string
): Promise<RequestSubmissionResult> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/requests`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ content })
  });
  return readJson<RequestSubmissionResult>(response, "Workspace request");
}

export async function fetchPlannerLogs(workspaceId: string): Promise<PlannerLog[]> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/planner/logs`);
  return readJson<PlannerLog[]>(response, "Planner logs");
}

export async function updatePlannerLog(
  workspaceId: string,
  plannerLogId: string,
  steps: PlannerLog["steps"]
): Promise<PlannerLog> {
  const response = await fetch(
    `${apiBaseUrl}/workspaces/${workspaceId}/planner/logs/${plannerLogId}`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ steps })
    }
  );
  return readJson<PlannerLog>(response, "Planner update");
}

export async function fetchWorkspaceAgents(workspaceId: string): Promise<string[]> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/agents`);
  return readJson<string[]>(response, "Workspace agents");
}

export async function approvePlannerLog(
  workspaceId: string,
  plannerLogId: string
): Promise<PlannerApprovalResult> {
  const response = await fetch(
    `${apiBaseUrl}/workspaces/${workspaceId}/planner/logs/${plannerLogId}/approve`,
    { method: "POST" }
  );
  return readJson<PlannerApprovalResult>(response, "Planner approval");
}

export async function fetchWorkspaceTasks(workspaceId: string): Promise<TaskItem[]> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/tasks`);
  return readJson<TaskItem[]>(response, "Workspace tasks");
}

export async function assignWorkspaceTaskAgent(
  workspaceId: string,
  taskId: string,
  agentName: string
): Promise<TaskItem> {
  const response = await fetch(
    `${apiBaseUrl}/workspaces/${workspaceId}/tasks/${taskId}/agent`,
    {
      method: "PUT",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ agentName })
    }
  );
  return readJson<TaskItem>(response, "Task agent assignment");
}

export async function fetchWorkspaceScheduledTasks(workspaceId: string): Promise<ScheduledTask[]> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/scheduler/tasks`);
  return readJson<ScheduledTask[]>(response, "Workspace scheduler");
}

export async function enqueueWorkspaceTask(
  workspaceId: string,
  taskId: string,
  repositoryPath: string,
  repositoryUrl: string
): Promise<ScheduledTask> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/scheduler/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      taskId,
      priority: null,
      repositoryPath,
      repositoryUrl,
      workspaceId
    })
  });
  return readJson<ScheduledTask>(response, "Workspace scheduler enqueue");
}

export async function processNextWorkspaceTask(workspaceId: string): Promise<ScheduledTask> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/scheduler/process-next`, {
    method: "POST"
  });
  return readJson<ScheduledTask>(response, "Workspace scheduler process");
}

export async function processWorkspaceTask(
  workspaceId: string,
  scheduledTaskId: string
): Promise<ScheduledTask> {
  const response = await fetch(
    `${apiBaseUrl}/workspaces/${workspaceId}/scheduler/tasks/${scheduledTaskId}/process`,
    { method: "POST" }
  );
  return readJson<ScheduledTask>(response, "Workspace scheduled task process");
}

export async function fetchWorkspaceSettings(
  workspaceId: string
): Promise<ToolEndpointSettings> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/settings`);
  return readJson<ToolEndpointSettings>(response, "Workspace settings");
}

export async function updateWorkspaceSettings(
  workspaceId: string,
  settings: ToolEndpointSettings
): Promise<ToolEndpointSettings> {
  const response = await fetch(`${apiBaseUrl}/workspaces/${workspaceId}/settings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(settings)
  });
  return readJson<ToolEndpointSettings>(response, "Workspace settings update");
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
  repositoryUrl: string,
  workspaceId?: string
): Promise<WorkflowRun> {
  const response = await fetch(`${apiBaseUrl}/workflows/investigate`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      taskId,
      repositoryPath,
      repositoryUrl,
      requestedAgents: [],
      workspaceId: workspaceId ?? null
    })
  });

  return readJson<WorkflowRun>(response, "Investigation API");
}

export async function fetchWorkflowEvents(runId: string): Promise<WorkflowEvent[]> {
  const response = await fetch(`${apiBaseUrl}/workflows/${runId}/events`);
  if (!response.ok) return [];

  return response.json() as Promise<WorkflowEvent[]>;
}
