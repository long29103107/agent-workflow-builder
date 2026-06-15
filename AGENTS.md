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
- `.codex/prompts` contains reusable Codex prompts for feature work, reviews, and MVP runs.
- `.codex/skills` contains repo-local implementation guidance for agent-platform work.

## Implementation Notes

- Keep `Program.cs` thin: dependency registration and endpoint mapping only.
- Add new contracts in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs` and models in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Add mock infrastructure in `src/AgentWorkflow.Core/Infrastructure/` before adding real Jira, Notion, Qdrant, Neo4j, GitHub/GitLab, or LLM integrations.
- Use the official OpenAI .NET SDK behind `IAgentReasoningService`; keep fallback behavior runnable when `OPENAI_API_KEY` is not configured.
- Preserve cancellation-token plumbing on async backend APIs.
- Update the README when runtime ports, commands, or API shapes change.

## Verification

Use these checks for normal changes:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
cd src/agent-workflow-ui
bun run build
```
