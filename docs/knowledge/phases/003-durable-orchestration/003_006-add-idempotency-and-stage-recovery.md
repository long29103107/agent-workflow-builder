---
type: phase-task
schema_version: 1
task_id: 003_006
phase: 003
status: planned
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 003_006: Add Idempotency And Stage Recovery

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [ ] Add idempotency keys to stage commands and external writes.
- [ ] Add safe retry rules for transient read operations.
- [ ] Prevent blind retry of commit, push, PR, or merge actions.
- [ ] Add recovery tests for interrupted workflows.

## Progress

- Status: `planned`
- Completed items: `0/4`
