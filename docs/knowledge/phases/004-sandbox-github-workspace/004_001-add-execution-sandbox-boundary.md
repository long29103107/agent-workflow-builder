---
type: phase-task
schema_version: 2
task_id: 004_001
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore
outcome: added_core_execution_sandbox_boundary
---

# 004_001: Add Execution Sandbox Boundary

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Define sandbox provision, execute, artifact, and destroy contracts in Core.
- [x] Model workspace leases and lifecycle events.
- [x] Require every code, command, and Git action to reference a workspace.
- [x] Keep a mock provider for deterministic tests.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore`

## Outcome

- Added `IExecutionSandboxProvider` plus workspace-scoped provision, code, command, Git, artifact, and destroy contracts in Core.
- Added sandbox lease, lifecycle event, action context, result, and artifact domain records.
- Registered a deterministic mock sandbox provider and covered lifecycle, workspace-scope enforcement, command, Git, code, and artifact behavior with Core tests.
