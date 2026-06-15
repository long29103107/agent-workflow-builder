# Agent Workflow Builder Context

## Purpose

Agent Workflow Builder is a runnable Agent Workflow Orchestration Platform skeleton. It receives a task, gathers context, delegates investigation to subagents, and produces an investigation summary plus execution plan.

## Source Of Truth

`src/AgentWorkflow.Core` is the source of truth for:

- Domain models
- Application interfaces
- Lead Agent orchestration
- Subagents
- Mock integrations
- Memory and repository context
- OpenAI SDK reasoning behind `IAgentReasoningService`

Adapters must stay thin:

- `src/AgentWorkflow.Api`
- `src/AgentWorkflow.Cli`
- `src/AgentWorkflow.Mcp`
- `src/agent-workflow-ui`

## Stack

- Backend: .NET 10
- Frontend: React + Vite using Bun CLI
- API: ASP.NET Core Minimal API
- CLI: .NET console adapter
- MCP: stdio JSON adapter
- Future memory: Qdrant vector memory and Neo4j graph memory
- Future tools: Jira MCP, Notion MCP, GitHub/GitLab repository intelligence

## Current Operating Rule

Every implementation task should be tied to a phase and task ID:

```text
PPP_TTT
```

- `PPP` is the three-digit phase number.
- `TTT` is the three-digit task number inside that phase.
- Example: `001_002` means Phase 001, Task 002.

After implementation, write a memory note under `.codex/memories/tasks/` using the same task ID.
