# 001_011: Migrate Project Knowledge To Open Knowledge Format

## Phase

001: Skeleton And Operating System

## Task

001_011: Migrate Project Knowledge To Open Knowledge Format

## Goal

Create a `docs/knowledge/` folder that converts repository documentation and source-derived project knowledge into Markdown files with YAML frontmatter, internal links, and concise sections for agents and humans.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Created Open Knowledge Format-style architecture, service, business, data, integration, runbook, standard, ADR, and index files under `docs/knowledge/`.
- Linked source documentation from the knowledge index.
- Marked missing or inferred information explicitly where needed.
- Added the knowledge-first workflow to `AGENTS.md`.

## Verification

- `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed with 0 warnings and 0 errors.
- Checked `docs/knowledge/` file list and frontmatter markers.

## Goal Achieved

Yes. The repository now has an Open Knowledge Format-style `docs/knowledge/` entry point and topic files.

## Next Idea

Add automated documentation linting for frontmatter and broken internal links once the knowledge folder stabilizes.
