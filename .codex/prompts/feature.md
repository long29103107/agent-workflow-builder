# Feature Prompt

Implement the requested feature inside the existing Agent Workflow Builder architecture.

Before editing:

1. Read `docs/knowledge/index.md`, related knowledge files, `AGENTS.md`, `README.md`, and the relevant source files.
2. Preserve the central Lead Agent / workflow engine shape.
3. Keep API, CLI, MCP, and UI changes runnable with mock integrations first.

Implementation preferences:

- Add interfaces before real external integrations.
- Keep `src/AgentWorkflow.Core` as the source of truth.
- Keep `src/AgentWorkflow.Api`, `src/AgentWorkflow.Cli`, and `src/AgentWorkflow.Mcp` as thin adapters.
- Keep the React UI as the actual investigation console, not a marketing page.
- Update `docs/knowledge` and README/API docs when behavior or commands change.

Verify with:

```powershell
dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj
cd src/agent-workflow-ui
bun run build
```
