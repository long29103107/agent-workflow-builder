---
type: phase-task
schema_version: 2
task_id: 003_004
phase: 003
status: done
updated_at: 2026-06-22
depends_on: none
---

# 003_004: Add Approval Policy Engine

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Model investigation-plan, implementation, pull-request, and merge gates.
- [x] Bind approvals to artifact hashes, target branches, or commit SHAs.
- [x] Invalidate stale approvals when approved inputs change.
- [x] Enforce policy in Core and write adapters, not only in the UI.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test AgentWorkflowBuilder.slnx --no-restore` passed: Core `38/38`, API `12/12`.
- `bun run build` passed for synchronized approval contracts.
- `dotnet ef migrations has-pending-model-changes --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --no-build` reported no pending changes.
- `powershell -ExecutionPolicy Bypass -File scripts/validate-phase-knowledge.ps1` passed.

## Outcome

Core now owns four durable approval gates bound to exact SHA-256 artifact hashes, protected target branches, or commit SHAs. Re-approval with changed inputs invalidates the previous record, guarded EngineeringTask transitions reject missing or stale approvals, and workspace planner approval signs and authorizes the exact planner-step snapshot before generating tasks.
