# 001_012: Prune Open Knowledge Format Noise

## Phase

001: Skeleton And Operating System

## Task

001_012: Prune Open Knowledge Format Noise

## Goal

Remove knowledge artifacts that are unnecessary after adopting `docs/knowledge/` as the Open Knowledge Format surface for Codex and humans.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Removed the one-off `MIGRATAE_TO_openknowledgeformat.md` migration prompt after its content was converted into `docs/knowledge/`.
- Removed placeholder-only deployment and rollback runbooks because the repository does not contain real production deployment or rollback procedures.
- Updated `docs/knowledge/index.md` so it links only to retained knowledge files and keeps deployment/rollback as open questions.
- Updated `docs/knowledge/standards/knowledge-maintenance.md` to keep one-off prompts and placeholder-only files out of the long-lived knowledge index.
- Preserved `.codex` workflow files because they still provide task IDs, memories, repo-local skills, and agent routing for Codex.

## Verification

- `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed with 0 warnings and 0 errors.
- Scanned `docs`, `AGENTS.md`, phase files, and task memories for removed migration, deployment, and rollback paths. No live knowledge links remain; only this historical task memory mentions the removed migration prompt.

## Goal Achieved

Yes. The Open Knowledge Format surface is smaller and no longer links to one-off or placeholder-only files.

## Next Idea

Add a lightweight docs link checker once the knowledge folder has more cross-links.
