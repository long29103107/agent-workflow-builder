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

## Implementation Notes

- Before implementing a new task, read `docs/knowledge/index.md`, the related knowledge files, `docs/knowledge/phases/README.md`, and the relevant `PHASE_SUMMARY.md`; load the individual task file only when its checklist or context is needed, then query CodeGraph for related source code context when `.codegraph/` is initialized.
- Every implementation task must use a task ID in `PPP_TTT` format, where `PPP` is phase number and `TTT` is task number. Example: `001_002`.
- If no task ID exists for the requested work, add the next task file to the relevant `docs/knowledge/phases/{phase}/` folder and link it from `PHASE_SUMMARY.md` before editing source.
- After implementation, keep the phase task status current and rely on CodeGraph plus `docs/knowledge` for searchable project context instead of writing Markdown memory logs.
- Keep `Program.cs` thin: dependency registration and endpoint mapping only.
- Add new contracts in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs` and models in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Add mock infrastructure in `src/AgentWorkflow.Core/Infrastructure/` before adding real Jira, Notion, Qdrant, Neo4j, GitHub/GitLab, or LLM integrations.
- Use the official OpenAI .NET SDK behind `IAgentReasoningService`; keep fallback behavior runnable when `OPENAI_API_KEY` is not configured.
- Preserve cancellation-token plumbing on async backend APIs.
- Update the README when runtime ports, commands, or API shapes change.

## Repo-Local Skills

- Use `.codex/skills/agent-workflow-platform.md` for this repository's source-of-truth architecture.
- Use `.codex/skills/implement-task.md` for ordinary task implementation from request to verified change.
- Use `.codex/skills/codegraph-memory.md` when initializing, querying, or maintaining CodeGraph as the repo-local memory/index surface.
- Use `.codex/skills/aspnet-core` for ASP.NET Core Minimal API, dependency injection, configuration, middleware, authentication, authorization, testing, performance, and upgrade work.
- Use `.codex/skills/cli-creator` when shaping `src/AgentWorkflow.Cli` into a durable external CLI with stable JSON, `doctor`, auth/config, install path, and companion usage docs.
- Use `.codex/skills/security-threat-model` when explicitly asked for threat modeling, trust boundaries, assets, abuse paths, and mitigations.
- Use `.codex/skills/security-best-practices` for supported security best-practice reviews, especially the TypeScript frontend; prefer `aspnet-core` references for ASP.NET Core security details.

## Repo-Local Agents

- Start with `.codex/agents/lead-task-agent.md` for broad task intake, decomposition, and specialist selection.
- Use `.codex/agents/repository-investigator-agent.md` for repository context and file discovery before implementation.
- Use `.codex/agents/backend-agent.md` for backend work spanning Core, API, CLI, MCP, dependency injection, contracts, provider boundaries, OpenAI reasoning, memory, and external tool abstractions.
- Use `.codex/agents/core-platform-agent.md` for source-of-truth Core changes.
- Use `.codex/agents/frontend-agent.md` for Bun + React UI work.
- Use `.codex/agents/docs-agent.md` for README, AGENTS, diagrams, and `.codex` assets.
- Use `.codex/agents/qa-agent.md` for verification strategy and smoke tests.

## Phase And Memory Workflow

- Phase folders live in `docs/knowledge/phases/`; scan `PHASE_SUMMARY.md` first and load task files only when needed.
- CodeGraph replaces Markdown task memory files for source-derived repo context.
- Run `codegraph init` once per checkout to create `.codegraph/`, then use `codegraph status`, `codegraph query`, `codegraph explore`, or MCP CodeGraph tools before broad file scans.
- Task IDs, checklists, and status remain in `docs/knowledge/phases/`; task outcomes that change durable behavior belong in the related knowledge files. Use targeted `rg`/file reads for Markdown phase and knowledge files because CodeGraph primarily indexes code and supported structured files.
- `AGENTS.md` is the routing surface; durable project knowledge and phase planning belong in `docs/knowledge`, while source-derived memory belongs in CodeGraph.

## Knowledge-first workflow

Before modifying code, always read:

1. `docs/knowledge/index.md`
2. Related service file in `docs/knowledge/services/`
3. Related business rule in `docs/knowledge/business/`
4. Related architecture or ADR files if the task changes design

When behavior changes:

- Update the related knowledge file.
- Add or update tests.
- Mention which knowledge files were changed in the final response.

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
