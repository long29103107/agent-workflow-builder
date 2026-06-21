---
type: phase-task
schema_version: 1
task_id: 003_002
phase: 003
status: planned
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 003_002: Add Background Workflow Worker

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [ ] Move long-running workflow execution out of HTTP requests.
- [ ] Persist and enqueue stage work before returning from the API.
- [ ] Add lease, cancellation, heartbeat, and graceful-shutdown behavior.
- [ ] Keep adapters thin.

## Progress

- Status: `planned`
- Completed items: `0/4`
