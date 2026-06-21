---
type: data-model
title: Project Aggregate And Policies
domain: core
owner: project
status: implemented
last_updated: 2026-06-21
tags:
  - data-model
  - project
  - policy
---

# Project Aggregate And Policies

## Purpose

Define the platform-owned Project aggregate that controls repository context and the safety policy applied before future repository write automation.

## Aggregate

`Project` contains:

- `Code`: a unique Jira-style key of 2-10 letters or digits, normalized to uppercase.
- `ProjectRepositorySettings`: provider, local path, URL, and default branch.
- `ProjectGitHubSettings`: owner, repository name, and optional installation ID.
- `ProjectAgentSettings`: enabled specialist agents and explicit-selection behavior.
- `ProjectCodingStandardSettings`: instruction files and project rules.
- `ProjectCommandSettings`: setup, build, test, and lint commands plus timeout.
- `ProjectBranchPolicy`: base branch, implementation branch prefix, and force-push policy.
- `ProjectProtectedPathPolicy`: protected repository-relative paths and production-environment protection.
- `ProjectApprovalPolicy`: investigation-plan, implementation, pull-request, and merge approval gates.

## Validation

`ProjectPolicyValidator` rejects missing project/repository values, unsafe branch policies, force push, incomplete agent/coding/command policies, invalid timeouts, rooted or parent-traversing paths, disabled production-environment protection, and disabled approval gates.

Project stores reject duplicate project codes. If a code is omitted during creation, Core derives it from the project name; updates preserve the existing code unless a replacement is supplied.

Validation failures use `ProjectPolicyValidationException` with structured `ProjectValidationError` items.

## Defaults And Compatibility

`ProjectPolicyDefaults` creates a runnable mock-first policy for the current repository. The default project ID is `workspace-default`, preserving the existing workspace API and UI selection.

The workspace compatibility store projects `Project` records into `WorkspaceProject` responses and delegates workspace create/update operations to `IProjectStore`.

Planner approval uses the owning Project code to issue task keys such as `AWB-1`, `AWB-2`, and `AWB-3`. Numbering increases independently within each project and repeated approval is idempotent.

The Project API exposes list, get, create, update, and delete operations. The seeded `workspace-default` Project is protected from API deletion because it anchors compatibility routes and default task data.

## Persistence

`PostgresProjectStore` is authoritative when PostgreSQL persistence is configured. It stores the validated Project aggregate as JSONB with indexed identity and timestamps. `InMemoryProjectStore` remains the explicit fallback when no PostgreSQL connection is supplied.

## Related Files

- `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`
- `src/AgentWorkflow.Core/Domain/ProjectPolicyDefaults.cs`
- `src/AgentWorkflow.Core/Domain/ProjectPolicyValidationException.cs`
- `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`
- `src/AgentWorkflow.Core/Infrastructure/Projects/InMemoryProjectStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Persistence/PostgresProjectStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Projects/ProjectPolicyValidator.cs`
- `src/AgentWorkflow.Core/Infrastructure/Workspaces/InMemoryWorkspaceStore.cs`
