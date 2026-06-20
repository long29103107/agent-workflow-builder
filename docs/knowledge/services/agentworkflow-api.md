---
type: service
title: AgentWorkflow.Api
domain: api
owner: project
status: draft
last_updated: 2026-06-19
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
- Configure CORS for the Vite development UI.
- Publish Swagger/OpenAPI JSON, Scalar API reference UI, and Swagger UI.
- Map `/api` endpoints for tasks, scheduler queue processing, workflow runs, memory, repository context, repository connection, health, and settings.

## Main APIs / Entry Points

- `GET /scalar/v1`
- `GET /swagger`
- `GET /swagger/v1/swagger.json`
- `GET /api/health`
- `GET /api/tasks`
- `GET /api/scheduler/tasks`
- `GET /api/scheduler/tasks/{scheduledTaskId}`
- `POST /api/scheduler/tasks`
- `POST /api/scheduler/process-next`
- `POST /api/workflows/investigate`
- `GET /api/workflows/{runId}`
- `GET /api/workflows/{runId}/events`
- `GET /api/memory/search?query=...`
- `POST /api/memory`
- `GET /api/repos/context?path=...`
- `GET /api/repos/connection`
- `POST /api/repos/connection`
- `GET /api/settings`
- `POST /api/settings`

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
- Memory creation returns a `Created` response with a search URL.
- Repository connection endpoints use a mock-first Core provider and do not call GitHub over the network yet.
- Scheduler enqueue rejects unknown tasks and active duplicates.
- Scheduler processing returns `404` when no queued task is available.

## Configuration

- Default local API port from launch settings is `http://localhost:5275`.
- Local Scalar API reference URL is `http://localhost:5275/scalar/v1`.
- Local Swagger UI URL is `http://localhost:5275/swagger`.
- Local Swagger/OpenAPI JSON URL is `http://localhost:5275/swagger/v1/swagger.json`.
- Docker Compose maps backend `8080` to host `5086`.
- CORS allows `http://localhost:5173` and `http://127.0.0.1:5173`.

## Related Files

- `src/AgentWorkflow.Api/Program.cs`
- `src/AgentWorkflow.Api/Endpoints/AgentWorkflowApiEndpoints.cs`
- `src/AgentWorkflow.Api/Extensions/ServiceCollectionExtensions.cs`
- `src/AgentWorkflow.Api/Extensions/WebApplicationExtensions.cs`
- `src/AgentWorkflow.Api/Dockerfile`

## Related Knowledge

- [Local Development](../runbooks/local-development.md)
- [Build](../runbooks/build.md)
