---
type: integration
title: CodeGraph Repo Memory
domain: integrations
owner: project
status: draft
last_updated: 2026-06-20
tags:
  - integration
  - codegraph
  - memory
---

# CodeGraph Repo Memory

## Purpose

Use CodeGraph as the repository-local source-code memory and code-index surface instead of Markdown task memory files under `.codex/memories/`.

## Current Implementation

- Task IDs and status remain in `docs/knowledge/phases/`; scan phase summaries before loading individual task files.
- Durable project knowledge remains in `docs/knowledge/`.
- CodeGraph owns source-derived searchable code context through a local `.codegraph/` SQLite index.
- Markdown phase and knowledge files are still read directly with `rg` or file reads.
- `.codegraph/` is ignored because it is local, rebuildable state.
- `.codex/memories/` is retired from the active workflow.

## Commands

```powershell
irm https://raw.githubusercontent.com/colbymchenry/codegraph/main/install.ps1 | iex
codegraph install
codegraph init
codegraph status .
codegraph query AgentWorkflow --limit 10
codegraph explore "How does the scheduler process tasks?"
codegraph node src/AgentWorkflow.Api/Endpoints/AgentWorkflowApiEndpoints.cs
```

`codegraph init` creates `.codegraph/` and builds the project index. The index storage is local SQLite at `.codegraph/codegraph.db`.

## Fallback

If `codegraph` is not installed or `.codegraph/` has not been initialized, use targeted `rg` and direct file reads, then report that CodeGraph memory was unavailable for the turn.

## Related Files

- `.codex/skills/codegraph-memory/SKILL.md`
- `docs/knowledge/phases/README.md`
- `AGENTS.md`

## Related Knowledge

- [Knowledge Maintenance Standards](../standards/knowledge-maintenance.md)
- [Memory And Repository Integrations](memory-and-repository.md)
