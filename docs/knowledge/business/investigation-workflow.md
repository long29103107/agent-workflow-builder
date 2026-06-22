---
type: business-rule
title: Investigation Workflow Rules
domain: workflow
owner: project
status: draft
last_updated: 2026-06-22
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
- A workflow run starts at `Created`, then advances through `LoadingTaskContext`, `ResolvingRepository`, `LoadingMemory`, `Investigating`, and `Aggregating` before `Completed`.
- Any non-terminal stage may transition to `Failed`; completed and failed runs are terminal.
- The Lead Agent is the authority for work-stage progression. The workflow engine persists those transitions and owns terminal completion or failure.
- Invalid, skipped, repeated, or out-of-order stage transitions are rejected before persistence.
- Every run persists its current stage, one-based attempt number, result, and failure details.
- Scheduled tasks run in Critical, High, Medium, then Low order.
- Tasks with equal priority run in FIFO enqueue order.
- A task cannot have more than one active queued or processing item.
- A task's assigned agent is copied into its scheduled item and becomes the investigation's requested-agent filter.
- Planner-generated task keys use the owning Project code and a monotonically increasing project-local number.
- Processing claims an item before starting the investigation workflow so concurrent calls do not process the same item.
- Explicit processing by scheduled-task ID claims only that queued item within its workspace; process-next retains priority and FIFO selection.
- API investigation requests persist the workflow run at `Created`, enqueue work, and return before Lead Agent execution begins.
- Processing claims carry a 30-second lease renewed by a five-second heartbeat.
- Cancellation clears the lease and requeues the scheduled item so graceful host shutdown does not mark it failed.
- Each workflow attempt records a Lead Agent execution and append-only action evidence for stage progress.
- Completed investigations record only a user-facing rationale summary, repository source references, aggregate tool-result counts, and the generated execution plan artifact.
- Evidence fields and artifact content are redacted before persistence; prompts and hidden chain-of-thought are never written to the evidence store.

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
