---
name: agent-workflow-platform
description: Apply Agent Workflow Builder architecture rules when changing orchestration, agents, memory, MCP tools, repository context, provider boundaries, or the investigation UI.
---

# Agent Workflow Platform

Use this skill for changes that cross or redefine the platform's architectural boundaries.

## Rules

- Keep `src/AgentWorkflow.Core` as the source of truth.
- Keep the Lead Agent as the single orchestrator.
- Keep subagents stateless and replaceable behind `ISubagent`.
- Use mock implementations first, with explicit provider boundaries.
- Add contracts before infrastructure implementations.
- Keep domain records in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Keep API, CLI, MCP, and React client contracts synchronized.
- Keep OpenAI SDK usage behind `IAgentReasoningService`.

## Extension Points

- Jira and Notion remain behind their MCP tool interfaces.
- Repository intelligence remains behind `IRepositoryReader`.
- Qdrant and Neo4j remain behind `IMemoryService` until specialized contracts are required.
- Run persistence remains behind `IWorkflowRunStore`.
