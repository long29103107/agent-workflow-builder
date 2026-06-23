---
type: phase-task
schema_version: 2
task_id: 004_006
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore
outcome: added_workspace_finalization_artifacts_destroy_and_quarantine
---

# 004_006: Capture Artifacts And Destroy Workspace

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Capture repository metadata, diffs, logs, and generated artifacts.
- [x] Apply artifact-retention rules.
- [x] Destroy or quarantine the workspace after completion or failure.
- [x] Prove cleanup cannot delete outside the configured workspace root.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore`

## Outcome

- Added `IRepositoryWorkspaceService.FinalizeAsync` to create repository metadata, diff, status, and log artifacts before workspace teardown.
- Added workspace finalization and artifact-retention domain records, including generated-artifact selection by retention limit.
- Added sandbox quarantine support for failed workspaces while preserving normal destroy behavior for successful runs.
- Hardened Docker artifact host-copy destinations so artifact names cannot escape the configured artifact root.
- Added tests for finalization artifacts, retention behavior, destroy/quarantine behavior, and Docker artifact-root safety.
