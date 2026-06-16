---
type: service
title: AgentWorkflow.Cli
domain: cli
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - service
  - cli
---

# AgentWorkflow.Cli

## Purpose

Run the shared investigation workflow from a local command-line adapter.

## Responsibilities

- Parse positional arguments.
- Register Core services.
- Start a workflow investigation.
- Write the resulting `WorkflowRun` as indented JSON.

## Main APIs / Entry Points

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

Arguments:

- First argument: task ID, defaults to `jira-awb-101`.
- Second argument: repository path, defaults to `null`.

## Dependencies

- [AgentWorkflow.Core](agentworkflow-core.md)
- `Microsoft.Extensions.DependencyInjection`

## Data Models

Outputs `WorkflowRun` JSON from [Workflow Domain Models](../data/workflow-domain-models.md).

## Business Rules

- If no task ID is provided, the CLI investigates `jira-awb-101`.
- Output is intended to be machine-readable JSON.

## Configuration

Uses Core environment variables such as `OPENAI_API_KEY`, `OPENAI_MODEL`, `AGENT_WORKFLOW_REPOSITORY_PATH`, and `AGENT_WORKFLOW_REPOSITORY_URL`.

The positional CLI shape remains:

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

The optional mock GitHub target flag is:

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 . --repo-url https://github.com/example/repository
```

## Related Files

- `src/AgentWorkflow.Cli/Program.cs`
- `src/AgentWorkflow.Cli/Application/CliOptions.cs`
- `src/AgentWorkflow.Cli/Application/CliRunner.cs`
- `src/AgentWorkflow.Cli/Extensions/ServiceCollectionExtensions.cs`

## Related Knowledge

- [Build](../runbooks/build.md)
- [Test](../runbooks/test.md)
