---
type: architecture
title: System Overview
domain: agent-workflow-builder
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - architecture
  - system
---

# System Overview

Agent Workflow Builder is a runnable skeleton for an Agent Workflow Orchestration Platform.

## Purpose

The system receives a task, gathers repository and planning context, delegates investigation to focused subagents, and returns an investigation summary plus an execution plan.

## Architecture

`AgentWorkflow.Core` is the source of truth for domain models, contracts, orchestration, mock integrations, repository context, memory, run persistence, and OpenAI SDK reasoning.

Adapters stay thin:

- [AgentWorkflow.Api](../services/agentworkflow-api.md) exposes HTTP routes.
- [AgentWorkflow.Cli](../services/agentworkflow-cli.md) runs the workflow from positional command arguments.
- [AgentWorkflow.Mcp](../services/agentworkflow-mcp.md) accepts line-delimited JSON over stdio.
- [Agent Workflow UI](../services/agent-workflow-ui.md) provides the React investigation console.

## Workflow

1. A task is selected.
2. The Lead Agent loads mock Jira and Notion context.
3. Repository context is read from a local path.
4. Mock vector memory and graph relationships are queried.
5. Subagents produce summaries, risks, open questions, and suggested execution steps.
6. Reasoning is summarized through OpenAI when configured, otherwise deterministic fallback output is used.
7. The workflow engine stores run status and timeline events in memory.

## Deployment Shape

Local Docker Compose starts API, UI, Neo4j, Qdrant, and Postgres. Real Neo4j, Qdrant, and Postgres implementations are planned but not wired in the current source.

## Related Knowledge

- [ADR 001: Core Is The Source Of Truth](decisions/adr-001-core-source-of-truth.md)
- [Investigation Workflow Rules](../business/investigation-workflow.md)
- [Memory And Repository Integrations](../integrations/memory-and-repository.md)

## Open Questions

- Production deployment topology is not detected from repository analysis.
- Authentication and authorization rules are not detected from repository analysis.
