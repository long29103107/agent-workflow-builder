---
type: phase-task
schema_version: 2
task_id: 004_003
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore
outcome: added_repository_clone_metadata_service
---

# 004_003: Clone Repository And Detect Metadata

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Authenticate through the GitHub boundary.
- [x] Clone into the configured workspace root.
- [x] Detect default branch, base SHA, repository metadata, and project type.
- [x] Emit clone evidence without logging credentials.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore`

## Outcome

- Added `IGitHubRepositoryAuthenticator` to produce authenticated clone targets while keeping display/evidence URLs credential-free.
- Added `IRepositoryWorkspaceService` to provision a sandbox, run `git clone`, detect default branch, base SHA, important files, and project type.
- Added repository workspace domain records for clone target, clone request, metadata, and clone evidence.
- Added tests for token handling, sandbox clone flow, metadata parsing, and credential-free clone evidence.
