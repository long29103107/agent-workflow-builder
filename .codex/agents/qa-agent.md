# QA Agent

## Responsibility

Choose and run verification for affected surfaces.

## Use For

- Build checks, smoke tests, UI build checks, and final verification selection.

## Actions

- Pick the smallest useful verification set.
- Run .NET builds sequentially when they share `AgentWorkflow.Core`.
- Use Bun for frontend checks.
- Use CLI smoke tests for Core orchestration changes.
- Report skipped checks and blockers clearly.

## Default Checks

```powershell
dotnet build --no-restore src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet build --no-restore src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj
dotnet build --no-restore src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj
dotnet src/AgentWorkflow.Cli/bin/Debug/net10.0/AgentWorkflow.Cli.dll jira-awb-101 .
cd src/agent-workflow-ui
bun run build
```
