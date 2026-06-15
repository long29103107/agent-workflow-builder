# 001_013: Prune Superseded Codex Context

## Phase

001: Skeleton And Operating System

## Task

001_013: Prune Superseded Codex Context

## Goal

Remove `.codex` files that became redundant after `docs/knowledge/` became the durable Open Knowledge Format surface, while keeping Codex operating assets that still work with the docs.

## Implementation Log

- Removed `.codex/context/project-context.md`, `.codex/context/task-workflow.md`, and `.codex/context/README.md` because their durable knowledge role is now covered by `docs/knowledge/`.
- Removed `.codex/memories/.gitkeep` because `.codex/memories/tasks/` contains real task memory files.
- Updated `AGENTS.md` to route project knowledge reads through `docs/knowledge/` and keep `.codex` for agents, prompts, skills, phases, and task memories.
- Updated `.codex/config.toml`, prompts, agents, and the local implementation skill so they no longer point to `.codex/context`.
- Updated the historical phase/memory note for `001_004` to mark `.codex/context` as superseded.
- Updated `docs/knowledge/standards/knowledge-maintenance.md` to document the split between `docs/knowledge` and `.codex`.

## Verification

- `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed with 0 warnings and 0 errors.
- Scanned `.codex`, `AGENTS.md`, and `docs/knowledge` for `.codex/context`, `project-context`, `task-workflow`, and `REQUEST.md`. No live routing references remain; only historical phase and task memory notes mention the removed context folder.

## Goal Achieved

Yes. `.codex` now keeps Codex operating assets, while durable project knowledge lives in `docs/knowledge/`.

## Next Idea

Add a small docs checklist for future `.codex` additions so durable knowledge does not drift back into Codex operating folders.
