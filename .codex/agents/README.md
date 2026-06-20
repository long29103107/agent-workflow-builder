# Repo-Local Agents

These files define project-local Codex operating roles. They are not runtime agents in `src/`; they describe which responsibility Codex should assume for a specific action.

## Selection Rule

Start with `lead-task-agent.md` for broad implementation work, then use the smallest specialist set needed for the affected surfaces.

## Primary Responsibilities

- `lead-task-agent.md`: task intake, decomposition, specialist selection, delivery flow.
- `repository-investigator-agent.md`: repository context, file discovery, technology signals.
- `backend-agent.md`: backend work spanning Core, API, CLI, MCP, DI, contracts, provider boundaries, OpenAI reasoning, memory, and external tool abstractions.
- `core-platform-agent.md`: source-of-truth Core changes.
- `frontend-agent.md`: Bun + React UI changes.
- `docs-agent.md`: README, AGENTS, diagrams, `.codex` assets, and CodeGraph memory guidance.
- `qa-agent.md`: verification and smoke-test selection.
