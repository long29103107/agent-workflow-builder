---
type: service
title: AgentWorkflow.Api
domain: api
owner: project
status: draft
last_updated: 2026-06-22
tags:
  - service
  - api
  - aspnet-core
---

# AgentWorkflow.Api

## Purpose

Expose AgentWorkflow.Core behavior through a thin ASP.NET Core Minimal API adapter.

## Responsibilities

- Register API services and Core services.
- Host the background workflow worker that drains queued investigations outside HTTP requests.
- Configure CORS for the Vite development UI.
- Publish Swagger/OpenAPI JSON, Scalar API reference UI, and Swagger UI.
- Organize Minimal API mappings into feature route groups and expose matching Swagger/Scalar tags.
- Map `/api` endpoints for Projects, project-scoped EngineeringTasks and WorkItems, workspaces, request intake, planner approval, compatibility tasks, scheduler queue processing, workflow runs, memory, repository context, repository connection, health, and settings.

## Main APIs / Entry Points

- `GET /scalar/v1`
- `GET /swagger`
- `GET /swagger/v1/swagger.json`
- `GET /api/health`
- `GET /api/tasks`
- `GET /api/tasks/{taskId}/history`
- `GET /api/tasks/{taskId}/activity`
- `GET /api/scheduler/tasks`
- `GET /api/scheduler/tasks/{scheduledTaskId}`
- `POST /api/scheduler/tasks`
- `POST /api/scheduler/process-next`
- `POST /api/workflows/investigate`
- `GET /api/workflows/{runId}`
- `GET /api/workflows/{runId}/events`
- `GET /api/workflows/{runId}/evidence`
- `GET /api/memory/search?query=...`
- `POST /api/memory`
- `GET /api/repos/context?path=...`
- `GET /api/repos/connection`
- `POST /api/repos/connection`
- `GET /api/settings`
- `POST /api/settings`
- `GET /api/projects`
- `POST /api/projects`
- `GET /api/projects/{projectId}`
- `PUT /api/projects/{projectId}`
- `DELETE /api/projects/{projectId}`
- `GET /api/projects/{projectId}/tasks`
- `POST /api/projects/{projectId}/tasks`
- `GET /api/projects/{projectId}/tasks/{taskId}`
- `PATCH /api/projects/{projectId}/tasks/{taskId}/status`
- `GET /api/projects/{projectId}/tasks/{taskId}/approvals`
- `POST /api/projects/{projectId}/tasks/{taskId}/approvals`
- `GET /api/projects/{projectId}/tasks/{taskId}/work-items`
- `POST /api/projects/{projectId}/tasks/{taskId}/work-items`
- `GET /api/workspaces`
- `POST /api/workspaces`
- `GET /api/workspaces/{workspaceId}`
- `PUT /api/workspaces/{workspaceId}`
- `GET /api/workspaces/{workspaceId}/requests`
- `POST /api/workspaces/{workspaceId}/requests`
- `GET /api/workspaces/{workspaceId}/agents`
- `GET /api/workspaces/{workspaceId}/planner/logs`
- `PUT /api/workspaces/{workspaceId}/planner/logs/{plannerLogId}`
- `POST /api/workspaces/{workspaceId}/planner/logs/{plannerLogId}/approve`
- `GET /api/workspaces/{workspaceId}/tasks`
- `PUT /api/workspaces/{workspaceId}/tasks/{taskId}/agent`
- `GET /api/workspaces/{workspaceId}/scheduler/tasks`
- `POST /api/workspaces/{workspaceId}/scheduler/tasks`
- `POST /api/workspaces/{workspaceId}/scheduler/tasks/{scheduledTaskId}/process`
- `POST /api/workspaces/{workspaceId}/scheduler/process-next`
- `GET /api/workspaces/{workspaceId}/settings`
- `POST /api/workspaces/{workspaceId}/settings`

## Dependencies

- [AgentWorkflow.Core](agentworkflow-core.md)
- ASP.NET Core Minimal APIs
- Microsoft.AspNetCore.OpenApi
- Scalar.AspNetCore
- Swashbuckle.AspNetCore.SwaggerUI

## Data Models

HTTP payloads use Core records from [Workflow Domain Models](../data/workflow-domain-models.md).

## Business Rules

- `taskId` is required for investigation requests.
- Unknown workflow run IDs return `404`.
- Workflow evidence returns one ordered bundle of agent executions, evidence items, and artifacts, or `404` for an unknown run.
- Task history uses an exclusive sequence cursor; task activity SSE resumes from `Last-Event-ID`, emits heartbeats, and bounds each replay batch.
- Memory creation returns a `Created` response with a search URL.
- Repository connection endpoints use a mock-first Core provider and do not call GitHub over the network yet.
- Scheduler enqueue rejects unknown tasks and active duplicates.
- Investigation requests return `202 Accepted` after persisting a `Created` workflow run and enqueueing background work.
- Processing uses a renewable lease and heartbeat; host cancellation requeues interrupted in-memory work.
- Scheduler processing returns `404` when no queued task is available.
- A default workspace is seeded from `WorkspaceDefaults`.
- Request submission creates a Project-owned EngineeringTask, a compatible workspace request, and a pending planner log.
- Planner edits are accepted only while a log is pending and require at least one complete step whose owner is an enabled project agent.
- Planner approval is idempotent and creates workspace-scoped planner tasks.
- Task assignment validates the selected name against the workspace project's enabled agents and is retained by the in-memory workspace assignment store.
- Scheduler duplicate checks and process-next selection are scoped by workspace for workspace routes.
- Exact scheduled-task processing is workspace-scoped and accepts only queued items, allowing Kanban drag-and-drop to start the selected card.
- Workspace state is in memory and resets when the API process restarts.
- Project APIs validate Core policies and protect the seeded default project from deletion.
- Project task APIs enforce project scope, return linked WorkItems, and update typed lifecycle state.
- Guarded task lifecycle updates return `409` unless their approval binding matches an active Core approval.

## Configuration

- Default local API port from launch settings is `http://localhost:5275`.
- Local Scalar API reference URL is `http://localhost:5275/scalar/v1`.
- Local Swagger UI URL is `http://localhost:5275/swagger`.
- Local Swagger/OpenAPI JSON URL is `http://localhost:5275/swagger/v1/swagger.json`.
- Docker Compose maps backend `8080` to host `5086`.
- CORS allows `http://localhost:5173` and `http://127.0.0.1:5173`.
- `ConnectionStrings:AgentWorkflowDb` provides the local PostgreSQL connection for debugging and future EF Core migrations.
- `Persistence:Provider=PostgreSql` selects PostgreSQL stores; tests override it with `InMemory`.
- `WorkflowWorker:Enabled` enables the hosted queue worker and defaults to `true`.
- `Cors:AllowedOrigins`, `WorkspaceDefaults`, and `ToolEndpoints` are read from API appsettings.

## Related Files

- `src/AgentWorkflow.Api/Program.cs`
- `src/AgentWorkflow.Api/Endpoints/AgentWorkflowApiEndpoints.cs`
- `src/AgentWorkflow.Api/Endpoints/*ApiEndpoints.cs`
- `src/AgentWorkflow.Api/Endpoints/ProjectApiEndpoints.cs`
- `src/AgentWorkflow.Api/Endpoints/ProjectTaskApiEndpoints.cs`
- `src/AgentWorkflow.Api/Extensions/ServiceCollectionExtensions.cs`
- `src/AgentWorkflow.Api/Extensions/WebApplicationExtensions.cs`
- `src/AgentWorkflow.Api/Services/WorkflowBackgroundWorker.cs`
- `src/AgentWorkflow.Api/Dockerfile`

## Related Knowledge

- [Local Development](../runbooks/local-development.md)
- [Build](../runbooks/build.md)
