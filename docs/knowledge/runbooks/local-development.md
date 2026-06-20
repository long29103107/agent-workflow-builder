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
- Optional: CodeGraph CLI for repo-local source code memory.

## Repo Memory

```powershell
irm https://raw.githubusercontent.com/colbymchenry/codegraph/main/install.ps1 | iex
codegraph install
codegraph init
codegraph status .
```

CodeGraph stores its local SQLite index under `.codegraph/`, which is ignored by git.

## Backend

```powershell
dotnet run --project src/AgentWorkflow.Api
```

Default local API URL: `http://localhost:5275`.

API documentation:

- Scalar UI: `http://localhost:5275/scalar/v1`
- Swagger UI: `http://localhost:5275/swagger`
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
{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","repositoryUrl":"https://github.com/example/repository","requestedAgents":[]}
```

## Optional Environment

```powershell
$env:OPENAI_API_KEY='your-api-key'
$env:OPENAI_MODEL='gpt-5.1'
$env:AGENT_WORKFLOW_REPOSITORY_PATH=(Get-Location).Path
$env:AGENT_WORKFLOW_REPOSITORY_URL='https://github.com/example/repository'
```

## Related Knowledge

- [AgentWorkflow.Api](../services/agentworkflow-api.md)
- [Agent Workflow UI](../services/agent-workflow-ui.md)
