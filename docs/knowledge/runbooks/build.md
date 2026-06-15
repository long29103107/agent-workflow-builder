---
type: runbook
title: Build
domain: operations
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - runbook
  - build
---

# Build

## Purpose

Build the backend, CLI, MCP adapter, and frontend.

## .NET Projects

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet build src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj
dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj
```

## Frontend

```powershell
cd src/agent-workflow-ui
bun run build
```

## Docker Compose

```powershell
docker compose up --build
```

## Notes

If a build fails because an executable is locked, stop the running API, CLI, or MCP process and retry.

## Related Knowledge

- [Troubleshooting](troubleshooting.md)
