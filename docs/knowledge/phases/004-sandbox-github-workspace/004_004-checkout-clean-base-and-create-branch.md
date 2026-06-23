---
type: phase-task
schema_version: 2
task_id: 004_004
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj
outcome: added_clean_base_checkout_and_policy_branch_preparation
---

# 004_004: Checkout Clean Base And Create Branch

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Recreate or reset the run workspace safely.
- [x] Checkout the selected base SHA.
- [x] Apply the Project branch naming convention.
- [x] Never mutate the default branch directly.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj`

## Outcome

- Added `PrepareBranchAsync` to `IRepositoryWorkspaceService` for deterministic clean-base branch preparation after clone.
- Added repository branch preparation request/result domain records carrying the workspace, branch policy, selected base SHA, target branch, and evidence.
- Implemented branch preparation by fetching the default branch, detaching to the selected base SHA, cleaning and hard-resetting the workspace, force-updating the policy branch, and checking it out.
- Added tests for policy branch naming, clean reset commands, Git action order, and rejection of default/base branch mutation.
