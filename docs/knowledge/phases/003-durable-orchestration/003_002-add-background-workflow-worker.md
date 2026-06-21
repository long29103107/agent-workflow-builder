---
type: phase-task
task_id: 003_002
phase: 003
status: planned
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
