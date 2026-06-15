---
type: standard
title: Coding Standards
domain: engineering
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - standard
  - coding
---

# Coding Standards

## Purpose

Capture project-specific implementation rules for future agents and humans.

## Standards

- Keep `src/AgentWorkflow.Core` as the source of truth.
- Keep API, CLI, MCP, and UI as thin adapters over Core.
- Keep one central Lead Agent / workflow engine responsible for orchestration.
- Keep subagents replaceable behind `ISubagent`.
- Keep external systems behind interfaces.
- Prefer mock-first runnable behavior before adding real providers.
- Preserve cancellation-token plumbing on async backend APIs.
- Keep `Program.cs` files thin.
- Use Bun for frontend dependency and script execution.

## Source Of Truth For Changes

- Add contracts in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`.
- Add domain models in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Add mock infrastructure under `src/AgentWorkflow.Core/Infrastructure/`.

## Related Knowledge

- [ADR 001: Core Is The Source Of Truth](../architecture/decisions/adr-001-core-source-of-truth.md)
- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)
