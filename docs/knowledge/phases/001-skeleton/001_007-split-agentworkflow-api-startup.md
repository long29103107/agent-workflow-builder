---
type: phase-task
schema_version: 1
task_id: 001_007
phase: 001
status: done
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 001_007: Split AgentWorkflow.Api Startup

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Move API service registration out of `Program.cs`.
- [x] Move Minimal API endpoint mapping out of `Program.cs`.
- [x] Preserve the existing HTTP routes and response behavior.
- [x] Verify the API project build.

## Progress

- Status: `done`
- Completed items: `4/4`
