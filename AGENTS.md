# Agent Workflow Builder Guidance

This repository is a runnable skeleton for an Agent Workflow Orchestration Platform.

## Architecture Rules

- Keep `src/AgentWorkflow.Core` as the source of truth for orchestration, domain models, contracts, mock integrations, and OpenAI SDK reasoning.
- Keep one central Lead Agent / workflow engine responsible for orchestration, event emission, and aggregation.
- Keep subagents as replaceable workers with clear interfaces and mock implementations first.
- Keep external systems behind interfaces:
  - Jira and Notion MCP tools
  - repository reader
  - vector memory
  - graph memory
  - workflow run persistence
- Do not introduce peer-to-peer agent swarms in this MVP.
- Prefer a small runnable vertical slice over broad incomplete integrations.

## Current Project Shape

- `src/AgentWorkflow.Core` contains domain models, application interfaces, mocks, Lead Agent, subagents, workflow engine, and OpenAI SDK reasoning.
- `src/AgentWorkflow.Api` is a thin HTTP adapter over `AgentWorkflow.Core`.
- `src/AgentWorkflow.Cli` is a thin CLI adapter over `AgentWorkflow.Core`.
- `src/AgentWorkflow.Mcp` is a thin MCP/stdio adapter over `AgentWorkflow.Core`.
- `src/agent-workflow-ui` contains the React investigation console and uses Bun for frontend dependency and script execution.
- `docker-compose.yml` starts API, UI, Neo4j, Qdrant, and Postgres for local development.
- `docs/knowledge` contains durable project knowledge in Open Knowledge Format style.
- `.codex/agents` contains repo-local Codex agent roles split by responsibility and action.
- `docs/knowledge/phases` contains one folder per phase, concise `PHASE_SUMMARY.md` indexes, and task files with IDs in `PPP_TTT` format.
- `.codex/prompts` contains reusable Codex prompts for feature work, reviews, and MVP runs.
- `.codex/skills` contains repo-local implementation guidance for agent-platform work.
- `.codegraph/` is the local CodeGraph SQLite index for source-derived code context. It is ignored and rebuilt with `codegraph init` or `codegraph index`.

## Task Routing

- Use `.codex/skills/implement-task/SKILL.md` as the canonical implementation workflow.
- Scan the phase index and relevant `PHASE_SUMMARY.md`; load one task file only when needed.
- Read only knowledge related to the affected surface, then query CodeGraph for source context.
- Preserve `PPP_TTT` task IDs and run `scripts/validate-phase-knowledge.ps1` after phase/task edits.

## Repo-Local Skills

- Use `.codex/skills/agent-workflow-platform/SKILL.md` for this repository's source-of-truth architecture.
- Use `.codex/skills/implement-task/SKILL.md` for ordinary task implementation from request to verified change.
- Use `.codex/skills/codegraph-memory/SKILL.md` when initializing, querying, or maintaining CodeGraph as the repo-local memory/index surface.
- Generic skills (`aspnet-core`, `cli-creator`, `security-best-practices`, and `security-threat-model`) are external dependencies declared in `.codex/skills-manifest.json`; run `scripts/setup-codex.ps1` to install missing skills.

## Repo-Local Agents

- Start with `.codex/agents/lead-task-agent.md` for broad task intake, decomposition, and specialist selection.
- Use `.codex/agents/repository-investigator-agent.md` for repository context and file discovery before implementation.
- Use `.codex/agents/backend-agent.md` for backend work spanning Core, API, CLI, MCP, dependency injection, contracts, provider boundaries, OpenAI reasoning, memory, and external tool abstractions.
- Use `.codex/agents/core-platform-agent.md` for source-of-truth Core changes.
- Use `.codex/agents/frontend-agent.md` for Bun + React UI work.
- Use `.codex/agents/docs-agent.md` for README, AGENTS, diagrams, and `.codex` assets.
- Use `.codex/agents/qa-agent.md` for verification strategy and smoke tests.

## Context Ownership

- `AGENTS.md` routes work; it does not duplicate the implementation workflow.
- `docs/knowledge` owns durable knowledge and phase planning.
- CodeGraph owns searchable source-derived context.
- `.codex` owns prompts, agents, skills, and config only.

## Skill Maintenance

- Keep project skills under `.codex/skills/` with standard `SKILL.md` structure.
- Keep reusable generic skills out of git and declare them in `.codex/skills-manifest.json`.
- Run `scripts/setup-codex.ps1` after cloning to install missing generic skills.
- Update the skill list above when adding or removing skills.
- Restart Codex after adding a new skill folder.
- Use `docs/knowledge/runbooks/` for build and test commands.
