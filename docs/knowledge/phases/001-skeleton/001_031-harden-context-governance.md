---
type: phase-task
schema_version: 2
task_id: 001_031
phase: 001
status: done
updated_at: 2026-06-21
depends_on: 001_030
---

# 001_031: Harden Context Governance

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Backfill legacy task metadata without inventing verification evidence.
- [x] Derive the next eligible task from phase status and dependencies.
- [x] Add CI validation for phase knowledge and repo-local skills.
- [x] Pin the validator dependency and run the official skill validator.
- [x] Replace vendored generic skills with a manifest and setup script.
- [x] Verify the complete context-management workflow.

## Progress

- Status: `done`
- Completed items: `6/6`

## Verification

- `scripts/validate-phase-knowledge.ps1` passed for 9 phases and 84 tasks and derived `003_001` as the next eligible task.
- `scripts/validate-skills.py` passed for all 3 repo-local skills with pinned PyYAML 6.0.2.
- Official `skill-creator/scripts/quick_validate.py` passed for all 3 repo-local skills.
- `scripts/setup-codex.ps1` installed the two missing generic skills and passed a second idempotency run.
- All 9 configured Codex prompt files exist.
- `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed with 0 warnings and 0 errors.
- `git diff --check` passed; only line-ending conversion warnings were reported.

## Outcome

Phase/task governance is now automated and CI-enforced, historical gaps are explicit rather than fabricated, and generic Codex skills are reproducible without being vendored into the repository.
