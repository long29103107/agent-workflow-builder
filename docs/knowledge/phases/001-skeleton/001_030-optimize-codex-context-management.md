---
type: phase-task
schema_version: 2
task_id: 001_030
phase: 001
status: done
updated_at: 2026-06-21
depends_on: 001_029
---

# 001_030: Optimize Codex Context Management

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Remove broken agent routes from Codex config.
- [x] Add active phase and next task metadata.
- [x] Add deterministic phase/task validation.
- [x] Make `implement-task` the canonical operating workflow.
- [x] Archive the stale workspace plan and remove the obsolete .NET API skill.
- [x] Add compact verification and outcome fields to the task template.

## Verification

- `scripts/validate-phase-knowledge.ps1` passes for 9 phases and 83 tasks.
- All 7 configured agent prompt paths exist.
- Canonical skill frontmatter and UI metadata pass equivalent structural checks.
- Official `quick_validate.py` could not run because `PyYAML` is not installed.

## Outcome

Context routing now has one canonical implementation workflow, deterministic phase validation, explicit next-work metadata, and a smaller active documentation surface.
