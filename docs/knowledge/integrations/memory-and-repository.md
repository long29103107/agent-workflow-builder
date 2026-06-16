---
type: integration
title: Memory And Repository Integrations
domain: integrations
owner: project
status: draft
last_updated: 2026-06-16
tags:
  - integration
  - memory
  - repository
---

# Memory And Repository Integrations

## Purpose

Provide repository context, vector-style memory, and graph-style relationships to investigation runs.

## Current Implementation

- `MockRepositoryConnectionService` resolves either a local repository path or a GitHub repository URL behind `IRepositoryConnectionService`.
- `LocalRepositoryReader` inspects local files and returns up to 12 representative files.
- When a GitHub URL is provided, repository context is currently a mock GitHub workspace summary; real clone and checkout are planned in Phase 002.
- `MockMemoryService` stores memory items in process memory.
- `MockMemoryService` returns static graph relationships for task, repository, and workflow-memory context.

## Planned Implementation

Inferred from source code, Docker Compose, and backlog:

- Qdrant will provide vector memory.
- Neo4j will provide graph memory.
- GitHub connectors will clone repositories into isolated workflow workspaces before richer repository intelligence runs.
- GitLab connectors may be added later through the same repository connection boundary.

## Configuration

- `AGENT_WORKFLOW_REPOSITORY_PATH`: optional default repository path.
- `AGENT_WORKFLOW_REPOSITORY_URL`: optional default GitHub repository URL for mock workspace targeting.
- Docker Compose sets `Neo4j__Uri`, `Qdrant__Endpoint`, and `Postgres__ConnectionString`, but current source does not consume them for real persistence.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/Repository/LocalRepositoryReader.cs`
- `src/AgentWorkflow.Core/Infrastructure/Repository/MockRepositoryConnectionService.cs`
- `src/AgentWorkflow.Core/Infrastructure/Memory/MockMemoryService.cs`
- `docker-compose.yml`

## Related Knowledge

- [Workflow Domain Models](../data/workflow-domain-models.md)
- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)

## Open Questions

- Real Qdrant collection schema is not detected from repository analysis.
- Real Neo4j labels and relationship schema are not detected from repository analysis.
- Real repository provider authentication is not detected from repository analysis.
