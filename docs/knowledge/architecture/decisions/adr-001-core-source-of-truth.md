---
type: adr
title: Core Is The Source Of Truth
domain: architecture
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - adr
  - core
  - orchestration
---

# ADR 001: Core Is The Source Of Truth

## Status

Accepted for the MVP skeleton.

## Context

The repository has multiple adapters: HTTP API, CLI, MCP stdio, and React UI. The same workflow behavior must be available from each adapter.

## Decision

Keep `src/AgentWorkflow.Core` as the source of truth for orchestration, domain models, contracts, mock integrations, memory, repository context, run persistence, and OpenAI SDK reasoning.

Adapters must remain thin and call Core interfaces instead of duplicating workflow logic.

## Consequences

- API, CLI, MCP, and UI share one workflow implementation.
- New real providers should be added behind Core interfaces first.
- Contract or behavior changes require updating related adapters and knowledge files.

## Related Files

- `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`
- `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`
- `src/AgentWorkflow.Core/Infrastructure/ServiceCollectionExtensions.cs`
- `AGENTS.md`

## Related Knowledge

- [System Overview](../system-overview.md)
- [AgentWorkflow.Core](../../services/agentworkflow-core.md)
