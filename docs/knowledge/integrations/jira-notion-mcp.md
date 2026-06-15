---
type: integration
title: Jira And Notion MCP Integrations
domain: integrations
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - integration
  - jira
  - notion
  - mcp
---

# Jira And Notion MCP Integrations

## Purpose

Provide task and planning context to workflow investigations.

## Current Implementation

The current repository uses mock providers:

- `MockJiraMcpTool` implements `IJiraMcpTool` and `ITaskSource`.
- `MockNotionContextTool` implements `INotionContextTool`.

## Planned Implementation

Inferred from source code and backlog: real Jira and Notion MCP clients should replace the mocks behind the same interfaces.

## Configuration

- Current default Jira endpoint: `mock://jira`
- Current default Notion endpoint: `mock://notion`
- Settings are held in memory per API process.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/Mcp/MockJiraMcpTool.cs`
- `src/AgentWorkflow.Core/Infrastructure/Mcp/MockNotionContextTool.cs`
- `src/AgentWorkflow.Core/Infrastructure/Settings/InMemorySettingsStore.cs`

## Related Knowledge

- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)

## Open Questions

- Real MCP authentication details are not detected from repository analysis.
- Authoritative Notion database/page structure is not detected from repository analysis.
