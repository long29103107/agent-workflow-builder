---
type: service
title: AgentWorkflow.Mcp
domain: mcp
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - service
  - mcp
  - stdio
---

# AgentWorkflow.Mcp

## Purpose

Expose the shared workflow through a lightweight line-delimited JSON stdio adapter.

## Responsibilities

- Register Core services.
- Read one JSON request per stdin line.
- Dispatch `workflow.investigate`.
- Write one JSON response per stdout line.
- Keep readiness diagnostics on stderr.

## Main APIs / Entry Points

```powershell
dotnet run --project src/AgentWorkflow.Mcp
```

Example request:

```json
{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","requestedAgents":[]}
```

## Dependencies

- [AgentWorkflow.Core](agentworkflow-core.md)
- `Microsoft.Extensions.DependencyInjection`

## Data Models

- Request: `McpInvestigationRequest`
- Response: `{ "result": WorkflowRun }` or `{ "error": "..." }`

## Business Rules

- Unsupported methods return an error response.
- `workflow.investigate` maps to `IWorkflowEngine.StartInvestigationAsync`.
- Stdout must remain JSON-only for protocol consumers.

## Configuration

Uses Core environment variables such as `OPENAI_API_KEY`, `OPENAI_MODEL`, and `AGENT_WORKFLOW_REPOSITORY_PATH`.

## Related Files

- `src/AgentWorkflow.Mcp/Program.cs`
- `src/AgentWorkflow.Mcp/Protocol/McpStdioServer.cs`
- `src/AgentWorkflow.Mcp/Contracts/McpInvestigationRequest.cs`

## Related Knowledge

- [Troubleshooting](../runbooks/troubleshooting.md)
- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)
