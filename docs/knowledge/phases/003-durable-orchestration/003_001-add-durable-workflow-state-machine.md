---
type: phase-task
schema_version: 2
task_id: 003_001
phase: 003
status: done
updated_at: 2026-06-21
depends_on: none
---

# 003_001: Add Durable Workflow State Machine

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Model workflow stages and legal transitions in Core.
- [x] Keep the Lead Agent as the single transition authority.
- [x] Persist current stage, attempt, result, and failure details.
- [x] Reject invalid or out-of-order transitions.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore` passed: 25/25.
- `dotnet test tests/AgentWorkflow.Api.Tests/AgentWorkflow.Api.Tests.csproj --no-restore` passed: 10/10.
- `dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj --no-restore` passed with 0 warnings and 0 errors.
- CLI smoke returned `Status=Completed`, `Stage=Completed`, `Attempt=1`, and no failure details.
- `bun run build` passed for the React UI.
- EF Core generated migration `20260621132104_AddDurableWorkflowStateMachine` and its idempotent SQL script.

## Outcome

Workflow runs now follow a validated durable stage sequence, expose and persist their current stage and attempt, retain terminal result or failure details, and reject skipped or out-of-order transitions.
