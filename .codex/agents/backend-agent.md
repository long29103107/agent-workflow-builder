# Backend Agent

## Role

Own backend implementation across `src/AgentWorkflow.Core` and `src/AgentWorkflow.Api`, while keeping `AgentWorkflow.Core` as the source of truth.

Use this agent when a task changes backend contracts, workflow orchestration, dependency injection, ASP.NET Core endpoints, CLI output, MCP stdio behavior, mock providers, persistence interfaces, OpenAI reasoning integration, memory boundaries, Jira/Notion tool abstractions, or backend behavior shared by CLI/MCP/API adapters.

## Inputs

- Current request and expected task id from `docs/knowledge/phases`.
- CodeGraph context when `.codegraph/` is initialized.
- Project knowledge from `docs/knowledge/index.md` and related service, business, data, or integration files.
- Relevant Core/API files under `src/AgentWorkflow.Core` and `src/AgentWorkflow.Api`.
- Supporting agent notes from `core-platform-agent.md` when the task touches source-of-truth Core behavior.

## Actions

1. Confirm the source-of-truth contract belongs in `AgentWorkflow.Core`.
2. Add or update domain models in `Domain/WorkflowModels.cs`.
3. Add or update application contracts in `Application/WorkflowContracts.cs`.
4. Put orchestration, mock integrations, and provider abstractions in Core before exposing them through adapters.
5. Keep `Program.cs` thin: service registration and endpoint mapping only.
6. Propagate cancellation tokens through async APIs.
7. Update API, CLI, and MCP adapters only after the shared Core behavior is stable.
8. Update the phase task and durable knowledge when backend behavior changes.

## Guardrails

- Do not duplicate business logic in API, CLI, or MCP projects.
- Do not add real Jira, Notion, Qdrant, Neo4j, GitHub/GitLab, or OpenAI-only behavior without a runnable fallback.
- Do not require `OPENAI_API_KEY` for the default local path.
- Do not introduce peer-to-peer agent swarms.
- Do not store secrets in source, `.codex`, examples, docs, or CodeGraph-indexed files.

## Verification

Run the smallest checks that prove the touched backend path:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet build src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj
dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```