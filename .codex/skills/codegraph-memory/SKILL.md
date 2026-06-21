---
name: codegraph-memory
description: Initialize, query, and maintain CodeGraph as Agent Workflow Builder's source-derived repository index while keeping phase planning and durable knowledge in their dedicated documentation surfaces.
---

# CodeGraph Memory

Use CodeGraph for source-derived context, not for phase status or durable product knowledge.

## Workflow

1. Confirm `codegraph` is installed and `.codegraph/` is initialized.
2. Run `codegraph status .` before relying on the index.
3. Prefer `codegraph query`, `codegraph explore`, and `codegraph node` before broad source scans.
4. Read `docs/knowledge/phases/PHASE_SUMMARY.md` files and task Markdown directly.
5. Keep `.codegraph/` ignored because it is a rebuildable local index.

## Fallback

If CodeGraph is unavailable or stale, say so and use targeted `rg` plus focused file reads.
