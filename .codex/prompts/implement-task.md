# Implement Task Prompt

Use this prompt when implementing a product, engineering, or integration task in this repository.

## Operating Mode

You are the implementation agent for Agent Workflow Builder. Your job is to take a task from idea to verified change while preserving the source-of-truth architecture.

Before editing:

1. Read `docs/knowledge/index.md`, related knowledge files, `AGENTS.md`, `README.md`, and `.codex/phases/README.md`.
2. Select an existing phase/task ID in `PPP_TTT` format or add a new task to the relevant `.codex/phases/*.md` file.
3. Read the selected phase file and any related `.codex/memories/tasks/*.md` files.
4. Identify which surfaces are affected:
   - `src/AgentWorkflow.Core`
   - `src/AgentWorkflow.Api`
   - `src/AgentWorkflow.Cli`
   - `src/AgentWorkflow.Mcp`
   - `src/agent-workflow-ui`
   - `.codex`
5. Check whether repo-local agents and skills apply:
   - `.codex/agents/lead-task-agent.md`
   - `.codex/agents/core-platform-agent.md`
   - `.codex/agents/api-adapter-agent.md`
   - `.codex/agents/cli-adapter-agent.md`
   - `.codex/agents/mcp-adapter-agent.md`
   - `.codex/agents/frontend-agent.md`
   - `.codex/agents/docs-agent.md`
   - `.codex/agents/qa-agent.md`
   - `.codex/skills/agent-workflow-platform.md`
   - `.codex/skills/aspnet-core`
   - `.codex/skills/cli-creator`
   - `.codex/skills/security-threat-model`
   - `.codex/skills/security-best-practices`
6. State a short plan before edits when the task changes architecture, public contracts, external integrations, security posture, or more than one surface.

## Implementation Rules

- Keep `src/AgentWorkflow.Core` as the source of truth.
- Keep API, CLI, MCP, and UI as adapters over Core.
- Add or change domain models and interfaces in Core first.
- Prefer mock-first, runnable behavior before real providers.
- Preserve cancellation-token plumbing in async .NET code.
- Keep OpenAI SDK usage behind `IAgentReasoningService`.
- Keep frontend commands Bun-based.
- Keep the phase file and task memory synchronized by task ID.
- Update `docs/knowledge`, `README.md`, `AGENTS.md`, `.codex/prompts`, or `.codex/skills` when runtime behavior or workflow rules change.
- Do not touch unrelated generated files, caches, or previous user changes.

## Delivery Checklist

Run the narrowest useful checks for the task. For broad changes, prefer:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
dotnet build src/AgentWorkflow.Cli/AgentWorkflow.Cli.csproj
dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj
dotnet src/AgentWorkflow.Cli/bin/Debug/net10.0/AgentWorkflow.Cli.dll jira-awb-101 .
cd src/agent-workflow-ui
bun run build
```

If restore/network is blocked, request scoped approval for the exact command.

## Task Memory

After implementation, create or update a note under `.codex/memories/tasks/` using `.codex/memories/task-memory-template.md`.

The memory filename must start with the selected task ID:

```text
.codex/memories/tasks/PPP_TTT-short-slug.md
```

Record phase, task, implementation log, verification, goal achieved, and next idea.
