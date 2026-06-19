# Backlog

## Product Goal

Build an AI Engineering Workspace where users manage software projects, submit engineering tasks, and let a central Lead Agent coordinate replaceable specialist agents from investigation through an approved GitHub pull request.

The platform must keep every workflow auditable, enforce approval gates in Core, execute repository writes only inside isolated workspaces, and preserve API, CLI, MCP, and React as thin adapters.

## Delivery Principles

- Keep `src/AgentWorkflow.Core` as the source of truth for domain models, orchestration, approval policy, agent contracts, and evidence rules.
- Keep one central Lead Agent / workflow engine; do not introduce peer-to-peer agent swarms.
- Add durable state, approval, and evidence before enabling code or GitHub write actions.
- Keep real external systems behind Core interfaces and preserve mock fallbacks while each vertical slice is built.
- Execute code changes, tests, Git operations, and generated artifacts inside isolated per-run workspaces.
- Preserve current investigation behavior through compatibility endpoints until replacement flows are verified.
- Never persist secrets or hidden chain-of-thought; persist structured rationale, source references, decisions, tool results, and artifacts.

## Phase 1: Runnable Skeleton

Status: delivered.

- React investigation console
- .NET API, CLI, and MCP adapters
- Central Lead Agent and replaceable investigation subagents
- Mock Jira, Notion, repository, memory, and graph providers
- Basic workflow events and OpenAI reasoning fallback
- Swagger/OpenAPI and Scalar API documentation
- Mock-first GitHub repository connection boundary

## Phase 2: Platform Foundation

Goal: create the durable Project and Engineering Task foundation before adding repository write automation.

- Restore the solution-level build and add automated test projects
- Preserve the delivered GitHub repository connection boundary
- Add `Project` with repository, GitHub, agent, command, protected-path, and approval settings
- Add `EngineeringTask` with the requested lifecycle
- Separate platform tasks from Jira/Notion source work items
- Replace string workflow statuses with typed state
- Add PostgreSQL persistence behind Core store interfaces
- Add project/task APIs while preserving current compatibility routes
- Seed a default project for the current local repository

## Phase 3: Durable Orchestration, Approval, And Evidence

Goal: make workflows resumable, auditable, and unable to bypass user approval.

- Add a durable workflow state machine owned by the central orchestrator
- Add a background worker so long-running workflows do not execute inside HTTP requests
- Add structured `AgentExecution`, `ApprovalRequest`, `EvidenceItem`, and `Artifact` models
- Enforce investigation-plan, implementation, pull-request, and merge approval gates
- Bind approvals to artifact hashes or commit SHAs and invalidate stale approvals
- Add append-only task history and safe structured rationale
- Add idempotency, correlation IDs, cancellation, and stage recovery
- Stream activity to the UI through SSE first

## Phase 4: Sandbox And GitHub Workspace

Goal: execute all repository operations inside isolated per-run workspaces.

- Add `IExecutionSandbox` and workspace lifecycle contracts in Core
- Implement a local Docker sandbox provider for the MVP
- Clone GitHub repositories into per-run workspace roots
- Detect default branch, base SHA, repository metadata, and project commands
- Enforce CPU, memory, timeout, network, environment, and filesystem limits
- Enforce protected paths and block production-environment mutation
- Checkout clean base state and create implementation branches
- Capture diffs and artifacts before any commit or push
- Destroy or quarantine workspaces according to retention policy

## Phase 5: Investigation And Architecture Agents

Goal: produce an evidence-backed implementation plan from the selected Project, Task, and cloned repository.

- Normalize Jira and Notion inputs into a shared `WorkItem` source model
- Add real Jira and Notion providers behind Core interfaces with mock fallbacks
- Upgrade repository intelligence with code search, dependency analysis, file summarization, and symbol discovery
- Formalize Investigator Agent output: related files, business flow, technical flow, risks, complexity, and plan
- Add Architect Agent to validate the plan and architecture consistency
- Store typed investigation and architecture artifacts with source references
- Require investigation-plan approval before the workspace becomes writable
- Move the current investigation console into the Task View

## Phase 6: Implementation, Testing, And Review

Goal: turn an approved plan into a reviewable diff without leaving the sandbox.

- Add Coder Agent behind a Core execution contract
- Apply only approved plan steps inside the isolated workspace
- Record why each file was selected and changed
- Add Tester Agent to generate tests, run configured commands, and analyze failures
- Add Reviewer Agent for maintainability, security, performance, and architectural consistency
- Capture command logs with secret redaction
- Produce diff, test, review, and release-note artifacts
- Require implementation approval against the final diff and verification result
- Allow reject, edit, retry, or regenerate before commit

## Phase 7: Pull Request Lifecycle

Goal: publish only approved work and keep GitHub state linked to the task.

- Add GitHub App authentication and installation/repository authorization
- Create commits from approved artifacts
- Push implementation branches without force by default
- Add PR Agent for title, body, release notes, and linked evidence
- Require pull-request approval before PR creation or ready-for-review transition
- Create and refresh GitHub pull requests
- Track reviews, comments, status checks, Actions workflow runs, and failures
- Require merge approval bound to the current head SHA
- Merge only when policy and status checks still pass
- Preserve branch, commit, PR, check, and merge evidence in task history

## Phase 8: Engineering Workspace UI And Observability

Goal: expose the platform as a usable engineering control plane.

- Add Projects, Tasks, Agents, Pull Requests, and Activity Feed navigation
- Add Task View tabs for investigation, plan, agents, evidence, diff, tests, review, approvals, and PR state
- Add Project settings for commands, conventions, protected paths, agents, and approval policy
- Add Agent View with execution history and active status
- Track token usage, cost, runtime, agent executions, tool calls, success rate, and failure rate
- Add project/run dashboards and filters
- Add OpenTelemetry traces and metrics without leaking prompts, secrets, or source content
- Add artifact retention and download controls

## Phase 9: Memory, Security, And Workflow Hardening

Goal: improve learning and reliability after the approved task-to-PR vertical slice works end to end.

- Persist completed-run decisions and outcomes as durable memory
- Add Qdrant vector search and Neo4j graph providers as derived indexes
- Link project, task, repository, file, branch, commit, PR, decision, and agent entities
- Add authentication, authorization, ownership, and tenancy boundaries
- Add safe retry policies for transient read operations
- Add recovery for clone, execution, verification, push, PR, and CI failures
- Add parallel execution only for independent read-only or explicitly approved subagent work
- Add policy tests proving agents cannot bypass approvals or protected paths
- Add audit retention, redaction, backup, and restore procedures
- Harden the full plan-to-merge workflow

## Default Approval Semantics

1. **Approve Investigation Plan** — authorizes repository writes in the sandbox.
2. **Approve Implementation** — approves the final diff and verification artifacts; authorizes commit and push.
3. **Approve Pull Request** — approves PR title, body, target branch, and release notes.
4. **Approve Merge** — approves merge of the exact current head SHA after required checks pass.

Any material artifact, diff, target branch, or head-SHA change invalidates the corresponding approval.

## First Next Task

Start with Phase 2 engineering baseline and Project/Task foundation. Do not implement the Coder Agent, push, PR creation, or merge until durable orchestration, structured evidence, approval enforcement, and isolated workspace controls are complete.
