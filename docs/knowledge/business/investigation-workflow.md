---
type: business-rule
title: Investigation Workflow Rules
domain: workflow
owner: project
status: draft
last_updated: 2026-06-19
tags:
  - business-rule
  - workflow
---

# Investigation Workflow Rules

## Purpose

Describe how an investigation run is created, executed, summarized, and exposed to adapters.

## Rules

- A workflow investigation requires a task ID.
- The Lead Agent is the single orchestrator for the MVP.
- Subagents are selected by name matching when requested; if no requested agent matches, all subagents run.
- Subagent suggested steps are ordered by `ExecutionStep.Order`.
- Risks and open questions are merged from subagent output and reasoning output, then de-duplicated.
- A workflow run starts as `Running` and becomes `Completed` or `Failed`.
- Scheduled tasks run in Critical, High, Medium, then Low order.
- Tasks with equal priority run in FIFO enqueue order.
- A task cannot have more than one active queued or processing item.
- Processing claims an item before starting the investigation workflow so concurrent calls do not process the same item.

## Validation

- API rejects blank `taskId` with a bad request response.
- Missing task IDs fail through the workflow engine with a failed run.
- Unknown scheduler task IDs are rejected before enqueue.

## Edge Cases

- If `OPENAI_API_KEY` is missing, deterministic fallback reasoning is used.
- If repository path does not exist, repository context falls back to a mock inventory message.

## Related Services

- [AgentWorkflow.Core](../services/agentworkflow-core.md)
- [AgentWorkflow.Api](../services/agentworkflow-api.md)
- [Agent Workflow UI](../services/agent-workflow-ui.md)

## Related Tests

Not detected from repository analysis.

## Related Source Files

- `src/AgentWorkflow.Core/Infrastructure/Agents/OpenAiLeadAgent.cs`
- `src/AgentWorkflow.Core/Infrastructure/Orchestration/WorkflowEngine.cs`
- `src/AgentWorkflow.Api/Endpoints/AgentWorkflowApiEndpoints.cs`

## Open Questions

- Production approval gates are planned in the backlog but not implemented.
