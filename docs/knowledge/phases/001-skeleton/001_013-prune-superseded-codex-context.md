---
type: phase-task
task_id: 001_013
phase: 001
status: done
---

# 001_013: Prune Superseded Codex Context

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Remove `.codex/context/` after durable project knowledge moved to `docs/knowledge/`.
- [x] Remove `.codex/memories/.gitkeep` because task memories now exist.
- [x] Update Codex config, prompts, agents, skills, and AGENTS guidance to use `docs/knowledge/`.
- [x] Keep `.codex` assets that still work with docs: agents, prompts, skills, phases, and task memories.
- [x] Verify documentation cleanup does not break the .NET build.

## Progress

- Status: `done`
- Completed items: `5/5`
