---
type: runbook
title: Test
domain: operations
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - runbook
  - test
---

# Test

## Purpose

Describe the current verification path.

## Automated Tests

Not detected from repository analysis.

## Smoke Tests

Run the CLI smoke test:

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

Expected high-level result: JSON output with `Status` or `status` equal to `Completed`.

Run the MCP stdio smoke test:

```powershell
'{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","requestedAgents":[]}' | dotnet run --project src/AgentWorkflow.Mcp
```

Expected high-level result: JSON output with `result.status` equal to `Completed`.

## Related Knowledge

- [Investigation Workflow Rules](../business/investigation-workflow.md)
- [AgentWorkflow.Cli](../services/agentworkflow-cli.md)
- [AgentWorkflow.Mcp](../services/agentworkflow-mcp.md)

## Open Questions

- Unit, integration, and browser test strategy is not detected from repository analysis.
