---
type: phase-task
schema_version: 2
task_id: 003_006
phase: 003
status: done
updated_at: 2026-06-22
depends_on: none
---

# 003_006: Add Idempotency And Stage Recovery

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Add idempotency keys to stage commands and external writes.
- [x] Add safe retry rules for transient read operations.
- [x] Prevent blind retry of commit, push, PR, or merge actions.
- [x] Add recovery tests for interrupted workflows.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test AgentWorkflowBuilder.slnx --no-restore -p:NuGetAudit=false` — 63 tests passed.
- `dotnet build AgentWorkflowBuilder.slnx --no-restore -p:NuGetAudit=false` — succeeded.
- `scripts/validate-phase-knowledge.ps1` — passed.

## Outcome

Workflow stage mutations are deduplicated by persisted command keys, interrupted non-terminal runs resume from their durable stage with a new attempt, and automatic retry is limited to explicitly transient reads. Commit, push, pull-request creation, and merge remain single-attempt operations unless a future provider resumes them through a persisted idempotency key.
