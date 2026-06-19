---
type: runbook
title: Test
domain: operations
owner: project
status: draft
last_updated: 2026-06-19
tags:
  - runbook
  - test
---

# Test

## Purpose

Describe the current verification path.

## Automated Tests

Run Core unit tests and API integration tests:

```powershell
dotnet test AgentWorkflowBuilder.slnx --no-restore --no-build
```

The scheduler tests cover priority ordering, FIFO tie-breaking, validation, concurrent claiming, API enqueue/process behavior, and empty-queue behavior.

## Smoke Tests

Run the CLI smoke test:

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

Expected high-level result: JSON output with `Status` or `status` equal to `Completed`.

Run the MCP stdio smoke test:

```powershell
'{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","repositoryUrl":"https://github.com/example/repository","requestedAgents":[]}' | dotnet run --project src/AgentWorkflow.Mcp
```

Expected high-level result: JSON output with `result.status` equal to `Completed`.

## Related Knowledge

- [Investigation Workflow Rules](../business/investigation-workflow.md)
- [AgentWorkflow.Cli](../services/agentworkflow-cli.md)
- [AgentWorkflow.Mcp](../services/agentworkflow-mcp.md)

## Open Questions

- Browser automation is not yet configured.
