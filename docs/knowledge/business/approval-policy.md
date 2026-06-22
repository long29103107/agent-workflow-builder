---
type: business-rule
title: Approval Policy Rules
domain: workflow
owner: project
status: implemented
last_updated: 2026-06-22
tags:
  - approval
  - policy
  - workflow
---

# Approval Policy Rules

## Purpose

Prevent planner, implementation, pull-request, and merge writes from using approval granted for different inputs.

## Gates And Bindings

- `InvestigationPlan` requires the SHA-256 hash of the exact plan snapshot.
- `Implementation` requires the SHA-256 hash of the exact implementation artifact.
- `PullRequest` requires an artifact hash and the Project protected base branch.
- `Merge` requires the protected target branch and exact commit SHA.

## Enforcement

- `IApprovalPolicyEngine` is the Core authority for approval creation and authorization.
- Approval creation is idempotent for the same project, task, gate, and normalized binding.
- Approving or authorizing changed inputs invalidates any active approval for the previous binding.
- Guarded EngineeringTask transitions require an exact active approval: plan before `ReadyForImplementation`, implementation before `ReadyForPullRequest`, pull request before `PullRequestOpen`, and merge before `Completed`.
- Workspace planner approval hashes the current request ID and planner steps, records the approval, then authorizes that exact binding before generating tasks.
- UI state is informative only and cannot bypass Core authorization.

## Persistence

`IApprovalStore` is in memory for the mock fallback and PostgreSQL-backed when configured. PostgreSQL stores approval history in `approvals`; invalidation changes lifecycle status but retains the original binding and approver audit data.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/Approvals/ApprovalPolicyEngine.cs`
- `src/AgentWorkflow.Core/Infrastructure/Approvals/InMemoryApprovalStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Persistence/PostgresApprovalStore.cs`
- `src/AgentWorkflow.Api/Endpoints/ProjectTaskApiEndpoints.cs`
