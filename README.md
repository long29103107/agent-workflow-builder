# Agent Workflow Builder

A runnable skeleton for an Agent Workflow Orchestration Platform. The source of truth is `AgentWorkflow.Core`, a .NET library that owns orchestration, domain contracts, mock integrations, and OpenAI SDK reasoning. API, CLI, MCP, and UI surfaces call into that shared core.

## Architecture

- `.codex` contains repo-local Codex prompts and skills for agent-platform work.
- `AGENTS.md` contains the working guidance for future agent runs in this repo.
- `src/AgentWorkflow.Core` is the source of truth for domain models, interfaces, Lead Agent orchestration, subagents, mock integrations, memory/repository services, workflow engine, and OpenAI SDK reasoning.
- `src/AgentWorkflow.Api` is the thin .NET HTTP backend over `AgentWorkflow.Core`.
- `src/AgentWorkflow.Cli` is the CLI adapter for external consumers.
- `src/AgentWorkflow.Mcp` is the MCP/stdio adapter for external tool hosts.
- `src/agent-workflow-ui` is the React frontend.
- `docker-compose.yml` starts backend, frontend, Neo4j, Qdrant, and Postgres for local development.

## Repository Structure

```text
.
|-- .codex/
|   |-- agents/
|   |-- config.toml
|   |-- context/
|   |-- memories/
|   |-- phases/
|   |-- prompts/
|   `-- skills/
|-- AGENTS.md
|-- BACKLOG.md
|-- REQUEST.md
|-- docker-compose.yml
|-- src/
|   |-- AgentWorkflow.Core/
|   |-- AgentWorkflow.Api/
|   |-- AgentWorkflow.Cli/
|   |-- AgentWorkflow.Mcp/
|   `-- agent-workflow-ui/
`-- README.md
```

## Agent Workflow

1. A user selects a Jira task in the React task board.
2. The user drags it into the Investigate lane and starts a run.
3. The Lead Agent loads mock Jira and Notion context.
4. The Lead Agent reads local repository context.
5. The Lead Agent queries mock vector memory and graph relationships.
6. The Lead Agent delegates to placeholder subagents:
   - Repository Investigator Agent
   - Jira/Notion Context Agent
   - Memory Research Agent
   - Planning Agent
7. The Lead Agent asks `IAgentReasoningService` to summarize via the OpenAI SDK when `OPENAI_API_KEY` is configured, or uses deterministic fallback output when it is not.
8. The workflow engine persists run events in memory.
9. API, CLI, MCP, and UI surfaces can display the same investigation summary and generated execution plan.

## Run Locally

### Backend

```powershell
dotnet run --project src/AgentWorkflow.Api
```

The API listens on `http://localhost:5275` by default.

Optional OpenAI SDK reasoning:

```powershell
$env:OPENAI_API_KEY='your-api-key'
$env:OPENAI_MODEL='gpt-5.1'
dotnet run --project src/AgentWorkflow.Api
```

Optional local repository override:

```powershell
$env:AGENT_WORKFLOW_REPOSITORY_PATH=(Get-Location).Path
dotnet run --project src/AgentWorkflow.Api
```

### Frontend

```powershell
cd src/agent-workflow-ui
bun install
bun run dev
```

Open `http://localhost:5173`.

To point the UI at a different API:

```powershell
$env:VITE_API_BASE_URL='http://localhost:5086/api'
bun run dev
```

### CLI

```powershell
dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .
```

### MCP stdio adapter

```powershell
dotnet run --project src/AgentWorkflow.Mcp
```

Send one JSON request per line:

```json
{"method":"workflow.investigate","taskId":"jira-awb-101","repositoryPath":".","requestedAgents":[]}
```

### Docker Compose

```powershell
docker compose up --build
```

Services:

- Frontend: `http://localhost:5173`
- Backend API: `http://localhost:5086`
- Neo4j browser: `http://localhost:7474`
- Qdrant: `http://localhost:6333`
- Postgres: `localhost:5432`

## API List

- `GET /api/health`
- `GET /api/tasks`
- `POST /api/workflows/investigate`
- `GET /api/workflows/{runId}`
- `GET /api/workflows/{runId}/events`
- `GET /api/memory/search?query=workflow`
- `POST /api/memory`
- `GET /api/repos/context?path=.`
- `GET /api/settings`
- `POST /api/settings`

Example investigation request:

```json
{
  "taskId": "jira-awb-101",
  "repositoryPath": ".",
  "requestedAgents": []
}
```

## Current Mock Integrations

- Jira tasks are served by `MockJiraMcpTool`.
- Notion context is served by `MockNotionContextTool`.
- Repository context is served by `LocalRepositoryReader`.
- Vector and graph memory are served by `MockMemoryService`.
- Lead Agent summarization is served by `OpenAiAgentReasoningService`; it uses the official OpenAI .NET SDK when `OPENAI_API_KEY` is set and deterministic fallback output otherwise.
- MCP endpoint and repository defaults are served by `InMemorySettingsStore`.
- Workflow runs are stored in `InMemoryWorkflowRunStore`.

The interfaces are intentionally stable so real MCP, Qdrant, Neo4j, GitHub/GitLab, LLM, and persistent run storage implementations can replace the mocks later.

## Roadmap

See [BACKLOG.md](BACKLOG.md).
