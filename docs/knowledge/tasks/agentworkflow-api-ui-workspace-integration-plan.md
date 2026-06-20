---
type: implementation-plan
title: AgentWorkflow.Api UI Workspace Integration Plan
domain: agent-workflow-builder
owner: backend
status: draft
last_updated: 2026-06-20
tags:
  - task
  - api
  - ui-integration
  - workspace
---

# AgentWorkflow.Api UI Workspace Integration Plan

## Goal

Make `AgentWorkflow.Api` support the current `agent-workflow-ui` dashboard flow as real API state instead of local UI-only state.

The API should support:

- Multiple workspaces, where each workspace represents one project.
- Request intake per workspace.
- Planner logs that wait for approval.
- Approved planner logs that create Kanban backlog tasks.
- Kanban movement from Backlog to Todo to In Progress.
- A pipeline status view for the active queued or processing task.
- Repository/API session settings scoped to the active workspace.

Keep the implementation mock-first and in-memory for this slice. Do not add database persistence yet.

## Current Gap

The UI currently keeps these concepts in local React state:

- Workspace projects.
- Request history.
- Planner logs and approval status.
- Planner-generated backlog tasks.
- Local queued and processing tasks.
- Per-workspace repository target and API key display state.

The API currently exposes shared endpoints for:

- `GET /api/tasks`
- `GET /api/scheduler/tasks`
- `POST /api/scheduler/tasks`
- `POST /api/scheduler/process-next`
- `GET /api/settings`
- `POST /api/settings`

These endpoints do not accept a `workspaceId`, so API-loaded backlog tasks and scheduler queues are shared across the session.

## Backend Design Rules

- Put source-of-truth records in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Put source-of-truth contracts in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`.
- Put mock/in-memory implementation in `src/AgentWorkflow.Core/Infrastructure/`.
- Keep `src/AgentWorkflow.Api` thin: endpoint mapping only.
- Keep cancellation tokens on async APIs.
- Do not store real API keys in logs, docs, memory, CodeGraph, or source.
- Treat `apiKey` as UI-only unless a later secret-store contract is added.

## Proposed Core Models

Add these records to `WorkflowModels.cs`:

```csharp
public sealed record WorkspaceProject(
    string Id,
    string Name,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider);

public sealed record UpdateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider);

public sealed record WorkspaceUserRequest(
    string Id,
    string WorkspaceId,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record CreateWorkspaceUserRequest(
    string Content);

public enum PlannerLogStatus
{
    PendingApproval,
    Approved
}

public sealed record PlannerLog(
    string Id,
    string WorkspaceId,
    string RequestId,
    string Request,
    PlannerLogStatus Status,
    IReadOnlyList<PlannerStep> Steps,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record PlannerStep(
    string Title,
    string Detail,
    string Owner);
```

Extend existing scheduler and task models only where needed:

- Add `WorkspaceId` to `TaskItem` only if planner-generated tasks need to be returned from `/api/workspaces/{workspaceId}/tasks`.
- Add `WorkspaceId` to `ScheduleTaskRequest` and `ScheduledTask` so queue state can be filtered by workspace.
- Keep `TaskItem.Source = "agent-planner"` for tasks created by planner approval.

## Proposed Core Contracts

Add these contracts to `WorkflowContracts.cs`:

```csharp
public interface IWorkspaceStore
{
    Task<IReadOnlyList<WorkspaceProject>> GetWorkspacesAsync(CancellationToken cancellationToken);
    Task<WorkspaceProject?> GetWorkspaceAsync(string workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceProject> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken);
    Task<WorkspaceProject?> UpdateWorkspaceAsync(string workspaceId, UpdateWorkspaceRequest request, CancellationToken cancellationToken);
}

public interface IRequestIntakeStore
{
    Task<IReadOnlyList<WorkspaceUserRequest>> GetRequestsAsync(string workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceUserRequest> CreateRequestAsync(string workspaceId, CreateWorkspaceUserRequest request, CancellationToken cancellationToken);
}

public interface IPlannerLogStore
{
    Task<IReadOnlyList<PlannerLog>> GetPlannerLogsAsync(string workspaceId, CancellationToken cancellationToken);
    Task<PlannerLog?> GetPlannerLogAsync(string workspaceId, string plannerLogId, CancellationToken cancellationToken);
    Task<PlannerLog> CreatePendingPlannerLogAsync(string workspaceId, WorkspaceUserRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> ApprovePlannerLogAsync(string workspaceId, string plannerLogId, CancellationToken cancellationToken);
}
```

Mock implementation behavior:

- `CreatePendingPlannerLogAsync` should generate the same four planner steps the UI currently generates:
  - Capture request
  - Ground in work item
  - Plan execution
  - Prepare processing
- `ApprovePlannerLogAsync` should mark the log `Approved` and create planner backlog tasks.
- Planner-generated tasks should remain queryable for the workspace after approval.

## Proposed API Endpoints

Add a workspace route group under `/api/workspaces`.

### Workspaces

- `GET /api/workspaces`
  - Returns all workspace projects.
- `POST /api/workspaces`
  - Creates a workspace.
- `GET /api/workspaces/{workspaceId}`
  - Returns one workspace.
- `PUT /api/workspaces/{workspaceId}`
  - Updates workspace name and repository target.

### Requests

- `GET /api/workspaces/{workspaceId}/requests`
  - Returns previous user requests for the workspace.
- `POST /api/workspaces/{workspaceId}/requests`
  - Creates a user request.
  - Also creates a pending planner log.
  - Response should include both the request and planner log.

Suggested response:

```json
{
  "request": { },
  "plannerLog": { }
}
```

### Planner

- `GET /api/workspaces/{workspaceId}/planner/logs`
  - Returns planner logs for the workspace.
- `POST /api/workspaces/{workspaceId}/planner/logs/{plannerLogId}/approve`
  - Approves the planner log.
  - Creates planner-generated backlog tasks.
  - Returns the approved log and created tasks.

Suggested response:

```json
{
  "plannerLog": { },
  "tasks": []
}
```

### Workspace Tasks

- `GET /api/workspaces/{workspaceId}/tasks`
  - Returns planner-generated tasks plus existing source tasks.
  - Planner-generated tasks should appear first, matching the UI.
- `POST /api/workspaces/{workspaceId}/scheduler/tasks`
  - Enqueues a task for that workspace.
- `GET /api/workspaces/{workspaceId}/scheduler/tasks`
  - Returns queued, processing, completed, and failed tasks for that workspace.
- `POST /api/workspaces/{workspaceId}/scheduler/process-next`
  - Processes the next queued task for that workspace.

Keep existing non-workspace scheduler endpoints during the migration, but make the UI move to workspace endpoints.

### Workspace Settings

- `GET /api/workspaces/{workspaceId}/settings`
  - Returns repository provider/path/url and MCP endpoints for the workspace.
- `POST /api/workspaces/{workspaceId}/settings`
  - Updates repository provider/path/url for the workspace.
  - May keep Jira and Notion endpoints shared until a real tenant settings contract exists.

Do not add `apiKey` to the backend settings contract in this slice.

## UI Contract Changes After API Work

Update `src/agent-workflow-ui/src/api/client.ts` to call workspace endpoints:

- Load workspaces from `GET /api/workspaces`.
- Create workspace through `POST /api/workspaces`.
- Submit request through `POST /api/workspaces/{workspaceId}/requests`.
- Load planner logs through `GET /api/workspaces/{workspaceId}/planner/logs`.
- Approve planner through `POST /api/workspaces/{workspaceId}/planner/logs/{plannerLogId}/approve`.
- Load backlog through `GET /api/workspaces/{workspaceId}/tasks`.
- Queue and process through workspace scheduler endpoints.
- Save repository settings through workspace settings endpoints.

After this migration, remove local-only request history, planner log, generated tasks, and local scheduler state from `useInvestigationConsole`.

## Implementation Order

1. Add Core domain models and contracts.
2. Add in-memory Core services:
   - `InMemoryWorkspaceStore`
   - `InMemoryRequestIntakeStore`
   - `InMemoryPlannerLogStore`
3. Extend scheduler state with `WorkspaceId`.
4. Add workspace endpoint mappings in `AgentWorkflow.Api`.
5. Register new Core services in API service registration.
6. Update OpenAPI/knowledge docs for new endpoints.
7. Update UI client to use workspace endpoints.
8. Remove UI-only workspace workflow state once API integration is verified.

## Acceptance Criteria

- `GET /api/workspaces` returns a default workspace when no workspace has been created.
- Creating a workspace makes it selectable by the UI.
- Submitting a request creates:
  - one workspace request entry,
  - one pending planner log.
- Approving a planner log creates workspace-scoped backlog tasks.
- Queueing a planner-generated task moves it to Todo for the same workspace.
- Processing the next task moves it to In Progress for the same workspace.
- Switching workspaces shows each workspace's own requests, planner logs, generated tasks, scheduler queue, and repository target.
- Existing non-workspace endpoints continue to work until UI migration is complete.

## Verification

Run:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
cd src/agent-workflow-ui
bun run build
```

Optional manual smoke:

1. Start API.
2. Start UI.
3. Create a second workspace.
4. Submit a request in workspace A.
5. Approve the planner log.
6. Queue and process one generated task.
7. Switch to workspace B and confirm workspace A state is not shown.

## Out Of Scope

- Database persistence.
- Real secret storage for API keys.
- Real GitHub clone/checkout.
- Real Code Review and Testing backend statuses.
- Authentication and authorization.
