---
type: phase-task
schema_version: 2
task_id: 005_001
phase: 005
status: done
updated_at: 2026-06-23
depends_on: none
---

# 005_001: Evidence-Backed Plan Contract

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Extend the Core execution plan contract with source references and an evidence summary.
- [x] Aggregate repository, memory, and graph context into the Lead Agent plan artifact.
- [x] Keep the React workflow contract aligned with the Core payload.
- [x] Cover the Lead Agent aggregation behavior with a focused Core test.
- [x] Run focused verification and phase knowledge validation.

## Progress

- Status: `done`
- Completed items: `5/5`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore --filter "OpenAiLeadAgentTests|WorkflowStateMachineTests"`
- `bun run build` from `src/agent-workflow-ui`

## Outcome

`ExecutionPlan` now carries deduplicated repository source references and a concise evidence summary. `OpenAiLeadAgent` populates those fields from repository, memory, and graph context, and the React workflow result view renders the evidence metadata.
