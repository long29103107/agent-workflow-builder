---
type: phase-task
schema_version: 1
task_id: 001_022
phase: 001
status: done
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 001_022: Replace Markdown Task Memories With CodeGraph

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Use CodeGraph as the repo-local searchable code/task context index.
- [x] Remove `.codex/memories` Markdown task memory workflow from active routing.
- [x] Update AGENTS, config, prompts, agents, phase guidance, README, and knowledge docs.
- [x] Ignore `.codegraph/` local SQLite indexes and document install/init commands.

## Progress

- Status: `done`
- Completed items: `4/4`
