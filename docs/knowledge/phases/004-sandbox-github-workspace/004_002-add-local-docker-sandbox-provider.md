---
type: phase-task
schema_version: 2
task_id: 004_002
phase: 004
status: done
updated_at: 2026-06-23
depends_on: none
verification: dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore
outcome: added_local_docker_sandbox_provider
---

# 004_002: Add Local Docker Sandbox Provider

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Provision an isolated container per workflow run.
- [x] Apply CPU, memory, timeout, network, environment, and filesystem limits.
- [x] Keep production paths and credentials outside writable mounts.
- [x] Capture redacted stdout, stderr, exit codes, and runtime.

## Progress

- Status: `done`
- Completed items: `4/4`

## Verification

- `dotnet test tests/AgentWorkflow.Core.Tests/AgentWorkflow.Core.Tests.csproj --no-restore`

## Outcome

- Added a Docker CLI-backed sandbox provider with per-lease containers, default CPU and memory limits, disabled network by default, command timeout execution, and artifact copy support.
- Added provider validation that blocks credential-like environment variables and writable protected host mounts.
- Kept deterministic mock behavior as the default provider; Docker is selected with `AGENT_WORKFLOW_SANDBOX_PROVIDER=docker`.
- Added tests for Docker command construction, output redaction, credential environment rejection, and protected mount rejection.
