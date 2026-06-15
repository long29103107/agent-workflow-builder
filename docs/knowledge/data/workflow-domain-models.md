---
type: data-model
title: Workflow Domain Models
domain: core
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - data-model
  - workflow
---

# Workflow Domain Models

## Purpose

Document the Core records that define API, CLI, MCP, and UI workflow payloads.

## Models

- `TaskItem`: Jira-like task metadata with source, key, title, description, status, priority, and tags.
- `WorkflowRun`: run ID, task ID, status, timestamps, and optional result.
- `WorkflowEvent`: timeline event with run ID, agent, type, and message.
- `InvestigationResult`: summary, execution plan, agent messages, repository context, memory items, and graph entities.
- `ExecutionPlan`: title, ordered steps, risks, and open questions.
- `MemoryItem`: vector-memory-style item with tags and creation timestamp.
- `GraphEntity`: graph-memory-style entity with related entity IDs.
- `RepositoryContext`: repository path, name, important files, detected technologies, and summary.
- `ToolEndpointSettings`: Jira endpoint, Notion endpoint, and repository path.

## Persistence

Current persistence is in memory through `InMemoryWorkflowRunStore` and `InMemorySettingsStore`.

## Database Models

No EF Core models, migrations, SQL schema, or production database model files are detected from repository analysis.

Docker Compose provisions Postgres, Neo4j, and Qdrant for future real implementations.

## Related Files

- `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`
- `src/AgentWorkflow.Core/Infrastructure/InMemoryWorkflowRunStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Settings/InMemorySettingsStore.cs`

## Related Knowledge

- [AgentWorkflow.Core](../services/agentworkflow-core.md)
- [Memory And Repository Integrations](../integrations/memory-and-repository.md)
