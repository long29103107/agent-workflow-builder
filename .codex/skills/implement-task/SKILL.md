---
name: implement-task
description: Implement, fix, refactor, or deliver tasks in Agent Workflow Builder. Use for repository changes that require phase tracking, focused context discovery, architecture-safe implementation, documentation updates, and verification.
---

# Implement Task

Use this as the single canonical operating workflow for ordinary repository implementation.

## 1. Route And Scope

1. Read `AGENTS.md` and `docs/knowledge/phases/README.md`.
2. Read the active or relevant `PHASE_SUMMARY.md`; load one task file only when its checklist or context is needed.
3. Select an existing `PPP_TTT` task or create and link the next task before editing source.
4. Read `docs/knowledge/index.md` and only the knowledge files directly related to the affected surface.
5. Query CodeGraph for related source context when initialized; otherwise use targeted `rg` and file reads.
6. Preserve unrelated user changes.

## 2. Implement

- Keep `AgentWorkflow.Core` as the source of truth for domain behavior, contracts, orchestration, and provider boundaries.
- Keep API, CLI, MCP, and UI as thin adapters.
- Keep `Program.cs` limited to registration, middleware, and endpoint mapping.
- Prefer mock-first runnable behavior unless a real provider is explicitly requested.
- Preserve cancellation tokens on async backend paths.
- Use Bun for frontend commands.
- Keep stdout JSON-only for MCP stdio.

## 3. Track Without Context Bloat

- Use task status values `planned`, `in_progress`, `blocked`, or `done`.
- Update checklist items as work completes.
- Record concise verification and outcome in the task file.
- Keep `PHASE_SUMMARY.md` link-oriented; update only status and counts.
- Update durable knowledge only when behavior or architecture changes.
- Run `scripts/validate-phase-knowledge.ps1` after phase/task edits.

## 4. Verify And Report

Run the narrowest checks that prove the changed surfaces. Report changed behavior, verification, knowledge files changed, and remaining risks. Never claim checks that were not run.

## Context Budget

- Do not load every task in a phase.
- Do not scan all knowledge files when one service or rule file is sufficient.
- Do not read README unless commands, ports, setup, or public API shape may change.
- Prefer CodeGraph queries and targeted `rg` over broad source dumps.