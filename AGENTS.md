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
- `.codex/agents` contains repo-local Codex agent roles split by responsibility and action.
- `.codex/context` contains durable project context and task workflow rules.
- `.codex/phases` contains phase files with task IDs in `PPP_TTT` format.
- `.codex/prompts` contains reusable Codex prompts for feature work, reviews, and MVP runs.
- `.codex/skills` contains repo-local implementation guidance for agent-platform work.
- `.codex/memories` contains repo-local task memory logs keyed by phase/task ID.

## Implementation Notes

- Before implementing a new task, read `.codex/context/project-context.md`, `.codex/context/task-workflow.md`, `.codex/phases/README.md`, the relevant phase file, and related `.codex/memories/tasks/*.md`.
- Every implementation task must use a task ID in `PPP_TTT` format, where `PPP` is phase number and `TTT` is task number. Example: `001_002`.
- If no task ID exists for the requested work, add the next task to the relevant `.codex/phases/*.md` file before editing source.
- After implementation, create or update `.codex/memories/tasks/PPP_TTT-short-slug.md` with phase, task, implementation log, verification, goal achieved, and next idea.
- Keep `Program.cs` thin: dependency registration and endpoint mapping only.
- Add new contracts in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs` and models in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Add mock infrastructure in `src/AgentWorkflow.Core/Infrastructure/` before adding real Jira, Notion, Qdrant, Neo4j, GitHub/GitLab, or LLM integrations.
- Use the official OpenAI .NET SDK behind `IAgentReasoningService`; keep fallback behavior runnable when `OPENAI_API_KEY` is not configured.
- Preserve cancellation-token plumbing on async backend APIs.
- Update the README when runtime ports, commands, or API shapes change.

## Repo-Local Skills

- Use `.codex/skills/agent-workflow-platform.md` for this repository's source-of-truth architecture.
- Use `.codex/skills/implement-task.md` for ordinary task implementation from request to verified change.
- Use `.codex/skills/aspnet-core` for ASP.NET Core Minimal API, dependency injection, configuration, middleware, authentication, authorization, testing, performance, and upgrade work.
- Use `.codex/skills/cli-creator` when shaping `src/AgentWorkflow.Cli` into a durable external CLI with stable JSON, `doctor`, auth/config, install path, and companion usage docs.
- Use `.codex/skills/security-threat-model` when explicitly asked for threat modeling, trust boundaries, assets, abuse paths, and mitigations.
- Use `.codex/skills/security-best-practices` for supported security best-practice reviews, especially the TypeScript frontend; prefer `aspnet-core` references for ASP.NET Core security details.

## Repo-Local Agents

- Start with `.codex/agents/lead-task-agent.md` for broad task intake, decomposition, and specialist selection.
- Use `.codex/agents/backend-agent.md` for backend work spanning Core, API, dependency injection, contracts, and provider boundaries.
- Use `.codex/agents/core-platform-agent.md` for source-of-truth Core changes.
- Use `.codex/agents/repository-investigator-agent.md` for repository context and file discovery.
- Use `.codex/agents/jira-notion-context-agent.md` for Jira/Notion task context and MCP tool boundaries.
- Use `.codex/agents/memory-research-agent.md` for vector memory, graph memory, Qdrant, and Neo4j boundaries.
- Use `.codex/agents/planning-agent.md` for execution plan shape, risks, open questions, and delivery sequencing.
- Use `.codex/agents/api-adapter-agent.md` for ASP.NET Core HTTP API work.
- Use `.codex/agents/cli-adapter-agent.md` for CLI commands and stable JSON output.
- Use `.codex/agents/mcp-adapter-agent.md` for MCP/stdio protocol work.
- Use `.codex/agents/frontend-agent.md` for Bun + React UI work.
- Use `.codex/agents/openai-reasoning-agent.md` for OpenAI SDK reasoning behind `IAgentReasoningService`.
- Use `.codex/agents/docs-agent.md` for README, AGENTS, diagrams, and `.codex` assets.
- Use `.codex/agents/qa-agent.md` for verification strategy and smoke tests.
- Use `.codex/agents/security-agent.md` for explicit security review, threat modeling, secrets, auth, and trust boundaries.

## Phase And Memory Workflow

- Phase files live in `.codex/phases/`.
- Task memory logs live in `.codex/memories/tasks/`.
- Task IDs must match between phase files and memory files.
- Use existing phase/task/memory context to develop the next idea instead of starting from scratch.
- `AGENTS.md` is the routing surface; detailed phase/task/memory details belong in `.codex/context`, `.codex/phases`, and `.codex/memories`.

## Project Skill Setup Rule

- When adding or updating Codex skills for this project, install them repo-locally under `.codex/skills/`.
- Preserve full skill folders, including `SKILL.md`, `references/`, `agents/`, `assets/`, scripts, and licenses when present.
- Do not install project skills into global `$CODEX_HOME/skills` unless explicitly requested.
- After adding or removing repo-local skills, update the `Repo-Local Skills` section above so future Codex runs know when to use each skill.
- Keep `AGENTS.md` compact; detailed reusable workflows and references belong in `.codex/skills/`.
- Tell the user to restart Codex after new skill folders are added so the session can pick them up.

## Verification

Use these checks for normal changes:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet build src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj
dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
cd src/agent-workflow-ui
bun run build
```
