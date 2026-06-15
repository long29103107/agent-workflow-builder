# Repo-Local Agents

These files define project-local Codex operating roles. They are not runtime agents in `src/`; they describe which responsibility Codex should assume for a specific action.

## Selection Rule

Start with `lead-task-agent.md` for broad implementation work, then use the smallest specialist set needed for the affected surfaces.

## Runtime Agent Responsibilities

- `lead-task-agent.md`: task intake, decomposition, specialist selection, delivery flow.
- `repository-investigator-agent.md`: repository context, file discovery, technology signals.
- `jira-notion-context-agent.md`: Jira/Notion task context and future MCP tool boundaries.
- `memory-research-agent.md`: vector memory, graph memory, Qdrant/Neo4j boundaries.
- `planning-agent.md`: execution plan, risks, open questions, delivery sequencing.

## Implementation Surface Responsibilities

- `backend-agent.md`: backend work spanning Core, API, DI, contracts, and provider boundaries.
- `core-platform-agent.md`: source-of-truth Core changes.
- `api-adapter-agent.md`: ASP.NET Core HTTP API changes.
- `cli-adapter-agent.md`: CLI command and JSON output changes.
- `mcp-adapter-agent.md`: MCP/stdio adapter changes.
- `frontend-agent.md`: Bun + React UI changes.
- `openai-reasoning-agent.md`: OpenAI SDK reasoning changes.
- `docs-agent.md`: README, AGENTS, diagrams, `.codex` assets.
- `qa-agent.md`: verification and smoke-test selection.
- `security-agent.md`: threat modeling, secrets, auth, trust boundaries.
