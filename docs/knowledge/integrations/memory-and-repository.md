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
- `IGitHubRepositoryAuthenticator` creates authenticated GitHub clone targets while keeping display URLs credential-free.
- `IRepositoryWorkspaceService` provisions a sandbox, clones a repository, detects default branch, base SHA, important files, and project type, and returns credential-free clone evidence.
- `IRepositoryWorkspaceService` prepares implementation branches by fetching the default branch, detaching to the selected base SHA, cleaning and hard-resetting the workspace, force-updating the policy branch, and checking it out.
- `IRepositoryWorkspaceService` finalizes workspaces by creating repository metadata, diff, status, and log artifacts, applying generated-artifact retention, and then destroying successful workspaces or quarantining failed workspaces.
- `IExecutionSandboxProvider` owns workspace-scoped provision, code, command, Git, artifact, and destroy contracts.
- `SandboxWorkspacePolicy` is enforced before sandbox actions to keep paths workspace-relative, reject protected paths, deny external writes unless explicitly allowed, and block deployment commands by default.
- `MockExecutionSandboxProvider` provides deterministic in-memory sandbox leases, lifecycle events, command results, and artifacts for tests and local runs.
- `LocalDockerExecutionSandboxProvider` provisions local Docker containers when `AGENT_WORKFLOW_SANDBOX_PROVIDER=docker`.
- Docker sandbox provisioning applies CPU and memory limits, disables networking by default, rejects credential-like environment variables, rejects writable protected host mounts, and redacts stdout/stderr.
- `MockMemoryService` stores memory items in process memory.
- `MockMemoryService` returns static graph relationships for task, repository, and workflow-memory context.
- CodeGraph provides repository-local source-derived code memory for Codex workflow context and replaces `.codex/memories/` Markdown task logs. This is separate from runtime workflow memory exposed through `IMemoryService`; Markdown phase and knowledge files are still read directly.

## Planned Implementation

Inferred from source code, Docker Compose, and backlog:

- Qdrant will provide vector memory.
- Neo4j will provide graph memory.
- Richer repository intelligence is planned after the sandbox and GitHub workspace boundary.
- GitLab connectors may be added later through the same repository connection boundary.

## Configuration

- `AGENT_WORKFLOW_REPOSITORY_PATH`: optional default repository path.
- `AGENT_WORKFLOW_REPOSITORY_URL`: optional default GitHub repository URL for mock workspace targeting.
- Docker Compose sets `Neo4j__Uri`, `Qdrant__Endpoint`, and `Postgres__ConnectionString`, but current source does not consume them for real persistence.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/Repository/LocalRepositoryReader.cs`
- `src/AgentWorkflow.Core/Infrastructure/Repository/MockRepositoryConnectionService.cs`
- `src/AgentWorkflow.Core/Infrastructure/Repository/GitHubRepositoryAuthenticator.cs`
- `src/AgentWorkflow.Core/Infrastructure/Repository/RepositoryWorkspaceService.cs`
- `src/AgentWorkflow.Core/Infrastructure/Sandbox/MockExecutionSandboxProvider.cs`
- `src/AgentWorkflow.Core/Infrastructure/Sandbox/LocalDockerExecutionSandboxProvider.cs`
- `src/AgentWorkflow.Core/Infrastructure/Memory/MockMemoryService.cs`
- `docker-compose.yml`
- `.codex/skills/codegraph-memory/SKILL.md`

## Related Knowledge

- [Workflow Domain Models](../data/workflow-domain-models.md)
- [Mock-First Provider Boundary Rules](../business/mock-first-provider-boundaries.md)
- [CodeGraph Repo Memory](codegraph-memory.md)

## Open Questions

- Real Qdrant collection schema is not detected from repository analysis.
- Real Neo4j labels and relationship schema are not detected from repository analysis.
- Real repository provider authentication is not detected from repository analysis.
