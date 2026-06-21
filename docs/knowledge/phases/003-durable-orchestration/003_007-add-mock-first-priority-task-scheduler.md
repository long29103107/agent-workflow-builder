---
type: phase-task
schema_version: 1
task_id: 003_007
phase: 003
status: done
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 003_007: Add Mock-First Priority Task Scheduler

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Add a Core-owned in-memory priority queue over the current task source.
- [x] Claim queued tasks in priority and FIFO order, then process them through the shared workflow engine.
- [x] Add thin API endpoints to enqueue, inspect, and process the next task.
- [x] Add React queue controls and monitoring without duplicating scheduling rules.
- [x] Add automated Core tests for priority order, FIFO tie-breaking, validation, and processing.
- [x] Defer CLI/MCP contract changes and durable background processing to their planned phases.

## Progress

- Status: `done`
- Completed items: `6/6`
