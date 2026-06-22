---
type: knowledge-index
title: Agent Workflow Builder Knowledge Index
domain: agent-workflow-builder
owner: project
status: draft
last_updated: 2026-06-21
tags:
  - knowledge
  - index
---

# Agent Workflow Builder Knowledge Index

This folder converts project documentation and source-derived knowledge into Open Knowledge Format style.

## Architecture

- [System Overview](architecture/system-overview.md)

## Planning

- [Phases And Tasks](phases/README.md)

## Services

- [AgentWorkflow.Core](services/agentworkflow-core.md)
- [AgentWorkflow.Api](services/agentworkflow-api.md)
- [AgentWorkflow.Cli](services/agentworkflow-cli.md)
- [AgentWorkflow.Mcp](services/agentworkflow-mcp.md)
- [Agent Workflow UI](services/agent-workflow-ui.md)

## Business Rules

- [Investigation Workflow Rules](business/investigation-workflow.md)
- [Approval Policy Rules](business/approval-policy.md)
- [Task Activity And SSE Rules](business/task-activity.md)
- [Mock-First Provider Boundary Rules](business/mock-first-provider-boundaries.md)

## Data Models

- [Project Aggregate And Policies](data/project-domain-model.md)
- [Workflow Domain Models](data/workflow-domain-models.md)

## Integrations

- [Jira And Notion MCP Integrations](integrations/jira-notion-mcp.md)
- [CodeGraph Repo Memory](integrations/codegraph-memory.md)
- [OpenAI Reasoning Integration](integrations/openai-reasoning.md)
- [Memory And Repository Integrations](integrations/memory-and-repository.md)

## Runbooks

- [Local Development](runbooks/local-development.md)
- [Build](runbooks/build.md)
- [Test](runbooks/test.md)
- [Troubleshooting](runbooks/troubleshooting.md)

## Standards

- [Coding Standards](standards/coding-standards.md)
- [Knowledge Maintenance Standards](standards/knowledge-maintenance.md)

## ADRs

- [ADR 001: Core Is The Source Of Truth](architecture/decisions/adr-001-core-source-of-truth.md)

## Source Documentation

- [Repository README](../../README.md)
- [Agent Guidance](../../AGENTS.md)
- [Backlog](../../BACKLOG.md)

## Open Questions

- Production deployment process is not detected from repository analysis.
- Production rollback process is not detected from repository analysis.
