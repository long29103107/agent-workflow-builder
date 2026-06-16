---
type: runbook
title: Local Development
domain: operations
owner: project
status: draft
last_updated: 2026-06-16
tags:
  - runbook
  - local-development
---

# Local Development

## Purpose

Run the Agent Workflow Builder MVP locally.

## Prerequisites

- .NET 10 SDK
- Bun for the React UI
- Docker when using Docker Compose

## Backend

```powershell
dotnet run --project src/AgentWorkflow.Api
```

Default local API URL: `http://localhost:5275`.

API documentation:

- Scalar UI: `http://localhost:5275/scalar/v1`
- Swagger/OpenAPI JSON: `http://localhost:5275/swagger/v1/swagger.json`

## Frontend

```powershell
cd src/agent-workflow-ui
bun install
bun run dev
```

Default UI URL: `http://localhost:5173`.

## CLI

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

## MCP Stdio

```powershell
dotnet run --project src/AgentWorkflow.Mcp
```

Send one JSON request per line:

```json
{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","requestedAgents":[]}
```

## Optional Environment

```powershell
$env:OPENAI_API_KEY='your-api-key'
$env:OPENAI_MODEL='gpt-5.1'
$env:AGENT_WORKFLOW_REPOSITORY_PATH=(Get-Location).Path
```

## Related Knowledge

- [AgentWorkflow.Api](../services/agentworkflow-api.md)
- [Agent Workflow UI](../services/agent-workflow-ui.md)
