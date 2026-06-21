---
type: phase-task
schema_version: 1
task_id: 001_021
phase: 001
status: done
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 001_021: Add Swagger UI

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Keep the existing built-in OpenAPI JSON endpoint and Scalar UI.
- [x] Add Swagger UI at /swagger using the existing /swagger/v1/swagger.json document.
- [x] Keep API documentation registration and mapping outside Program.cs.
- [x] Update README and API knowledge, then verify build and integration tests.

## Progress

- Status: `done`
- Completed items: `4/4`
