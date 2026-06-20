# CodeGraph Memory Skill

Use this repo-local skill when initializing, querying, or maintaining CodeGraph as the repository's source-derived memory index.

## Purpose

CodeGraph replaces Markdown task memory files under `.codex/memories/` for source-derived code context. Phase files still own task IDs and status, while durable project knowledge remains in `docs/knowledge/`.

## Commands

Run from the repository root:

```powershell
irm https://raw.githubusercontent.com/colbymchenry/codegraph/main/install.ps1 | iex
codegraph install
codegraph init
codegraph status .
codegraph query AgentWorkflow --limit 10
codegraph explore "How does the scheduler process tasks?"
codegraph node src/AgentWorkflow.Api/Endpoints/AgentWorkflowApiEndpoints.cs
```

## Workflow

1. Confirm `codegraph` is installed with `Get-Command codegraph`.
2. If it is not installed, install it with the official PowerShell installer or `npm i -g @colbymchenry/codegraph`.
3. Run `codegraph install` to wire the MCP server into supported agents when needed.
4. If `.codegraph/` is missing, run `codegraph init`.
5. Query CodeGraph before broad source scans when the task depends on source code context.
6. Keep `.codegraph/` out of git; it is a rebuildable local SQLite index.
7. Read Markdown phase and knowledge files directly with `rg`/file reads.
8. Update `.codex/phases/` for task status and `docs/knowledge/` for durable behavior changes.

## Fallback

If CodeGraph is unavailable or not initialized, state that clearly and fall back to targeted `rg` plus file reads.
