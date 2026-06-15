# 001_003: Setup Repo-Local Codex Skills And Agents

## Phase

001: Skeleton And Operating System

## Task

001_003: Setup Repo-Local Codex Skills And Agents

## Goal

Create repo-local Codex skills and agents so implementation work can route through specific responsibilities.

## Implementation Log

- Pulled repo-local skills into `.codex/skills/`.
- Added `.codex/agents/` with roles for Lead Task, runtime subagent responsibilities, Core, API, CLI, MCP, frontend, OpenAI reasoning, docs, QA, and security.
- Updated `.codex/config.toml` and `AGENTS.md` so Codex can discover and use repo-local agents.

## Verification

- `.codex/agents` contains README plus responsibility-specific agent files.
- `AGENTS.md` maps each agent to a clear action.

## Goal Achieved

Yes. The project now has repo-local agents split by responsibility.

## Next Idea

Use `.codex/agents/lead-task-agent.md` to select specialist agents for future work.
