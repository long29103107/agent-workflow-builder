---
type: phase-task
schema_version: 2
task_id: 003_002
phase: 003
status: done
updated_at: 2026-06-22
depends_on: none
---

# 003_002: Add Background Workflow Worker

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Move long-running workflow execution out of HTTP requests.
- [x] Persist and enqueue stage work before returning from the API.
- [x] Add lease, cancellation, heartbeat, and graceful-shutdown behavior.
- [x] Keep adapters thin.

## Progress

- Status: `done`
- Completed items: `4/4`

## Outcome

- `POST /api/workflows/investigate` persists a `Created` workflow run, enqueues its work, and returns `202 Accepted` without executing the Lead Agent in the request.
- The API hosted worker drains priority/FIFO work through the Core scheduler and shared workflow engine.
- Processing items expose heartbeat and lease timestamps; cancellation requeues work and host shutdown cancels processing gracefully.
- Scheduled queue entries remain mock-first and in memory; the workflow run itself uses the configured durable run store.

## Verification

- `dotnet test AgentWorkflowBuilder.slnx --no-restore`
- `powershell -ExecutionPolicy Bypass -File scripts/validate-phase-knowledge.ps1`
