---
type: data-model
title: Workflow Domain Models
domain: core
owner: project
status: draft
last_updated: 2026-06-21
tags:
  - data-model
  - workflow
---

# Workflow Domain Models

## Purpose

Document the Core records that define API, CLI, MCP, and UI workflow payloads.

## Models

- `TaskItem`: Jira-like task metadata with source, key, title, description, status, priority, tags, and an optional assigned agent.
- `EngineeringTask`: platform-owned engineering request scoped to a Project, with typed lifecycle state, priority, linked WorkItem IDs, and timestamps.
- `EngineeringTaskStatus`: `New`, investigation and approval stages, implementation and verification stages, pull-request stages, then `Completed` or `Failed`.
- `WorkItem`: source-owned Jira or Notion input linked to one EngineeringTask while preserving its source key, provider status, priority, and tags.
- `WorkflowRun`: run ID, task ID, status, durable stage, one-based attempt, timestamps, optional result, and optional failure details.
- `WorkflowStage`: legal lifecycle states from `Created` through context, repository, memory, investigation, and aggregation work to terminal `Completed` or `Failed`.
- `WorkflowEvent`: timeline event with run ID, agent, type, and message.
- `InvestigationResult`: summary, execution plan, agent messages, repository context, memory items, and graph entities.
- `ExecutionPlan`: title, ordered steps, risks, and open questions.
- `MemoryItem`: vector-memory-style item with tags and creation timestamp.
- `GraphEntity`: graph-memory-style entity with related entity IDs.
- `RepositoryContext`: repository path, name, repository connection, important files, detected technologies, and summary.
- `RepositoryConnection`: provider, URL, local path, owner, repository name, default branch, status, and summary.
- `ToolEndpointSettings`: Jira endpoint, Notion endpoint, repository path, repository URL, and repository provider.
- `ScheduleTaskRequest`: task ID, optional priority override, repository target, and optional assigned agent for a queued execution.
- `ScheduledTask`: queue identity, task metadata, priority, assigned/requested agents, status, timestamps, persisted workflow run ID, heartbeat, lease expiry, and error.
- `ScheduledTaskPriority`: `Low`, `Medium`, `High`, or `Critical`.
- `ScheduledTaskStatus`: `Queued`, `Processing`, `Completed`, or `Failed`.
- `WorkspaceProject`: project workspace identity, name, repository target, provider, and timestamps.
- `WorkspaceUserRequest`: user request content and creation time scoped to a workspace.
- `PlannerLog` and `PlannerStep`: approval status and agent planning breakdown for a workspace request.
- `UpdatePlannerLogRequest` and `AssignTaskAgentRequest`: pending-plan edits and workspace Kanban assignment commands.
- `RequestSubmissionResult` and `PlannerApprovalResult`: stable API responses connecting request intake, planner approval, and generated tasks.

## Persistence

API Projects, EngineeringTasks, WorkItems, WorkflowRuns, and WorkflowEvents use PostgreSQL when `Persistence:Provider` is `PostgreSql`. CLI, MCP, tests, settings, scheduler queue entries, and workspace planner state keep their in-memory implementations unless a persistent provider is explicitly supplied. API enqueue persists the linked WorkflowRun before returning even though the mock-first queue entry itself remains process-local.

`EngineeringTaskSource` projects platform tasks and their primary WorkItem back to the existing `TaskItem` contract. The current Jira IDs, keys, statuses, priorities, and tags remain compatible with API, CLI, MCP, scheduler, and UI consumers.

Project-scoped APIs expose EngineeringTask lists and details, typed lifecycle updates, and linked WorkItem reads/creation without exposing persistence entities.

## Database Models

`AgentWorkflowDbContext` maps `projects`, `engineering_tasks`, `work_items`, `workflow_runs`, and `workflow_events`. Workflow runs persist stage, attempt, JSONB result, and failure details. EF Core migrations preserve existing runs at the `Created` stage with attempt `1`. Qdrant and Neo4j remain derived mock/future providers rather than authoritative workflow state.

## Related Files

- `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`
- `src/AgentWorkflow.Core/Domain/WorkflowStateMachine.cs`
- `src/AgentWorkflow.Core/Infrastructure/Tasks/InMemoryEngineeringTaskStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Tasks/EngineeringTaskSource.cs`
- `src/AgentWorkflow.Core/Infrastructure/Persistence/AgentWorkflowDbContext.cs`
- `src/AgentWorkflow.Core/Infrastructure/Persistence/Migrations/`
- `src/AgentWorkflow.Core/Infrastructure/InMemoryWorkflowRunStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Settings/InMemorySettingsStore.cs`

## Related Knowledge

- [AgentWorkflow.Core](../services/agentworkflow-core.md)
- [Memory And Repository Integrations](../integrations/memory-and-repository.md)
