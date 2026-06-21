---
type: standard
title: Knowledge Maintenance Standards
domain: documentation
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - standard
  - knowledge
  - documentation
---

# Knowledge Maintenance Standards

## Purpose

Keep `docs/knowledge/` useful as a source for AI agents and humans.

## Standards

- Every knowledge file starts with YAML frontmatter.
- Use internal Markdown links for related knowledge.
- Mark inferred information with `Inferred from source code`.
- Use `Not detected from repository analysis` when information is missing.
- Add an `Open Questions` section for unclear design, deployment, data, or testing gaps.
- Update related knowledge files when behavior changes.
- Mention changed knowledge files in final implementation summaries.
- Keep one-off migration prompts, scratch requests, and placeholder-only files out of the long-lived knowledge index after their work is complete.
- Keep `.codex` focused on Codex operating assets such as agents, prompts, skills, and config; durable project knowledge and concise phase/task planning belong in `docs/knowledge/`.
- Keep each `PHASE_SUMMARY.md` short and link-oriented. Load individual phase task files only when their checklist or implementation context is needed.
- Run `scripts/validate-phase-knowledge.ps1` after changing phase metadata, task status, checklist progress, or summary links.
- Treat `active_phase` as the deliberate product decision and let the validator derive the next eligible task. Use `next_task_override` only for a temporary explicit ordering decision.
- Keep legacy task evidence honest: schema version 1 uses `not_recorded`; new or reopened work uses schema version 2 with concrete verification and outcome sections.
- Use CodeGraph for repo-local searchable source code context instead of Markdown task memory files. Read Markdown phase and knowledge files directly.

## Related Files

- `docs/knowledge/index.md`
- `AGENTS.md`
- `.codex/skills/codegraph-memory/SKILL.md`

## Related Knowledge

- [Coding Standards](coding-standards.md)
- [CodeGraph Repo Memory](../integrations/codegraph-memory.md)
