# API Adapter Agent

## Responsibility

Maintain `src/AgentWorkflow.Api` as a thin ASP.NET Core HTTP adapter over `AgentWorkflow.Core`.

## Use For

- Endpoint mapping, request validation, response shape, CORS, DI, and API docs.

## Actions

- Keep `Program.cs` focused on middleware, DI, and endpoint mapping.
- Route workflow behavior through Core interfaces.
- Validate required request fields at the edge.
- Keep frontend and external API contracts aligned.

## Guardrails

- Do not put orchestration logic in API.
- Do not add real provider clients directly in API.
