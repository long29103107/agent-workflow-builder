---
type: data-model
title: Workflow Domain Models
domain: core
owner: project
status: draft
last_updated: 2026-06-19
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
- `RepositoryContext`: repository path, name, repository connection, important files, detected technologies, and summary.
- `RepositoryConnection`: provider, URL, local path, owner, repository name, default branch, status, and summary.
- `ToolEndpointSettings`: Jira endpoint, Notion endpoint, repository path, repository URL, and repository provider.
- `ScheduleTaskRequest`: task ID, optional priority override, and repository target for a queued execution.
- `ScheduledTask`: queue identity, task metadata, priority, status, timestamps, workflow run ID, and error.
- `ScheduledTaskPriority`: `Low`, `Medium`, `High`, or `Critical`.
- `ScheduledTaskStatus`: `Queued`, `Processing`, `Completed`, or `Failed`.
- `WorkspaceProject`: project workspace identity, name, repository target, provider, and timestamps.
- `WorkspaceUserRequest`: user request content and creation time scoped to a workspace.
- `PlannerLog` and `PlannerStep`: approval status and agent planning breakdown for a workspace request.
- `RequestSubmissionResult` and `PlannerApprovalResult`: stable API responses connecting request intake, planner approval, and generated tasks.

## Persistence

Current persistence is in memory through `InMemoryWorkflowRunStore`, `InMemorySettingsStore`, `InMemoryTaskScheduler`, and the workspace request/planner stores.

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
