---
type: service
title: AgentWorkflow.Core
domain: core
owner: project
status: draft
last_updated: 2026-06-20
tags:
  - service
  - core
---

# AgentWorkflow.Core

## Purpose

Own the shared workflow orchestration, contracts, domain models, mock providers, repository context, memory behavior, run storage, and OpenAI reasoning.

## Responsibilities

- Resolve task context through `ITaskSource`.
- Coordinate the Lead Agent and subagents.
- Resolve repository connection targets, then query repository, memory, Jira, and Notion abstractions.
- Produce `WorkflowRun`, `InvestigationResult`, `ExecutionPlan`, and event timeline data.
- Queue task executions by priority and process claimed items through the shared workflow engine.
- Own the Project aggregate, project-policy validation, and in-memory Project store.
- Project current workspace records as a compatibility surface over the Project store.
- Keep fallback behavior runnable without `OPENAI_API_KEY`.

## Main APIs / Entry Points

- `IWorkflowEngine.StartInvestigationAsync`
- `ITaskScheduler.EnqueueAsync`
- `ITaskScheduler.ProcessNextAsync`
- `ILeadAgent.InvestigateAsync`
- `IRepositoryConnectionService.ResolveConnection`
- `IProjectStore.GetProjectsAsync`
- `IProjectPolicyValidator.Validate`
- `AddAgentWorkflowCore`

## Dependencies

- `Microsoft.Extensions.DependencyInjection`
- OpenAI .NET SDK through `OpenAI.Chat`

## Data Models

See [Project Aggregate And Policies](../data/project-domain-model.md) and [Workflow Domain Models](../data/workflow-domain-models.md).

## Business Rules

- [Investigation Workflow Rules](../business/investigation-workflow.md)
- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)

## Configuration

- `OPENAI_API_KEY`: enables OpenAI SDK reasoning.
- `OPENAI_MODEL`: optional model override, defaults to `gpt-5.1`.
- `AGENT_WORKFLOW_REPOSITORY_PATH`: optional repository path default.
- `AGENT_WORKFLOW_REPOSITORY_URL`: optional mock GitHub repository URL default.

## Related Files

- `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`
- `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`
- `src/AgentWorkflow.Core/Infrastructure/`

## Related Knowledge

- [System Overview](../architecture/system-overview.md)
- [ADR 001: Core Is The Source Of Truth](../architecture/decisions/adr-001-core-source-of-truth.md)

## Open Questions

- Persistent run storage implementation is not detected from repository analysis.
- The scheduler store is intentionally in memory until Phase 3 durable orchestration.
- Project persistence is intentionally in memory until Phase 2 PostgreSQL persistence.
