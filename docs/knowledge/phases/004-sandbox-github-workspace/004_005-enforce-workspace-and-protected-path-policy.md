---
type: phase-task
schema_version: 2
task_id: 004_005
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore
outcome: enforced_sandbox_workspace_and_protected_path_policy
---

# 004_005: Enforce Workspace And Protected-Path Policy

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Restrict filesystem access to the verified workspace root.
- [x] Enforce Project protected paths before and after changes.
- [x] Require explicit policy for network and external write access.
- [x] Block deployment commands by default.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore`

## Outcome

- Added `SandboxWorkspacePolicy` to action contexts so sandbox code, command, Git, and artifact actions can enforce per-project safety policy.
- Added shared sandbox policy enforcement for workspace-relative paths, protected paths, external write operations, and deployment commands.
- Applied the policy enforcer in both mock and Docker sandbox providers before executing workspace actions.
- Added tests for path escape rejection, protected path rejection, default-denied Git push/deployment commands, explicit policy opt-in, and Docker working-directory validation.
