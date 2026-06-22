---
type: phase-task
schema_version: 2
task_id: 003_005
phase: 003
status: done
updated_at: 2026-06-22
depends_on: none
---

# 003_005: Add Task History And SSE Activity

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Add an append-only TaskActivity model and durable store with monotonic sequence IDs.
- [x] Project workflow, agent, approval, evidence, and artifact events into task history.
- [x] Expose paged history using an exclusive sequence cursor.
- [x] Add an SSE stream resumable through `Last-Event-ID` or an explicit cursor.
- [x] Add heartbeat, cancellation, and bounded replay behavior for slow or disconnected clients.
- [x] Keep correlation IDs stable and redact activity summaries before persistence.
- [x] Preserve the current workflow event endpoint and synchronize API/UI contracts.

## Progress

- Status: `done`
- Completed items: `7/7`

## Review Gaps Addressed

- Defines history ownership, persistence, cursor semantics, replay bounds, and SSE reconnect behavior.
- Uses monotonic sequence IDs instead of timestamp/GUID ordering.
- Requires task/run/correlation identity and redaction across all projected event sources.
- Adds explicit compatibility and reconnect verification expectations.

## Verification

- `dotnet test AgentWorkflowBuilder.slnx --no-restore` passed: Core `43/43`, API `12/12`.
- API tests verified ordered history, the legacy workflow-event endpoint, query-cursor replay, and `Last-Event-ID` precedence during SSE reconnect.
- `bun run build` passed for synchronized history/SSE contracts.
- `dotnet ef migrations has-pending-model-changes --project src/AgentWorkflow.Core --startup-project src/AgentWorkflow.Api --no-build` reported no pending changes.
- `powershell -ExecutionPolicy Bypass -File scripts/validate-phase-knowledge.ps1` passed.

## Outcome

Tasks now have an append-only activity timeline with durable monotonic sequence IDs, stable task/run/correlation identity, and redacted summaries. Workflow, agent, approval, evidence, and artifact changes project into that timeline. The API supports exclusive-cursor history pages and cancellable SSE with heartbeats, bounded replay, and reconnect through `Last-Event-ID`, while the original workflow event endpoint remains available.
