---
type: business-rule
title: Mock-First Provider Boundary Rules
domain: platform
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - business-rule
  - mocks
  - integrations
---

# Mock-First Provider Boundary Rules

## Purpose

Keep the MVP runnable while making future real integrations replaceable.

## Rules

- External systems must sit behind Core interfaces.
- Mock implementations are preferred before real Jira, Notion, Qdrant, Neo4j, GitHub/GitLab, or persistent storage providers.
- Adapters must not bypass Core to call external systems directly.
- Real providers should replace mocks through dependency injection.

## Validation

- `AddAgentWorkflowCore` is the central registration point for current mock services.
- API, CLI, and MCP register Core instead of registering provider implementations themselves.

## Edge Cases

- Docker Compose starts Neo4j, Qdrant, and Postgres, but the current app still uses mock/in-memory implementations.

## Related Services

- [AgentWorkflow.Core](../services/agentworkflow-core.md)
- [AgentWorkflow.Mcp](../services/agentworkflow-mcp.md)

## Related Tests

Not detected from repository analysis.

## Related Source Files

- `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`
- `src/AgentWorkflow.Core/Infrastructure/ServiceCollectionExtensions.cs`
- `docker-compose.yml`

## Open Questions

- Authentication flow for real Jira and Notion MCP tools is not detected from repository analysis.
