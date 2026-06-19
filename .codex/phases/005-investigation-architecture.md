# Phase 005: Investigation And Architecture Agents

Goal: produce an evidence-backed implementation plan from the selected Project, Task, and cloned repository.

## Tasks

### 005_001: Normalize Jira And Notion WorkItems

Things to do:

- Add a shared WorkItem source model.
- Include description, acceptance criteria, source key, and source links.
- Map current mock Jira and Notion data without changing existing behavior.
- Link WorkItems to platform-owned EngineeringTasks.

Status: planned

### 005_002: Add Real Jira And Notion Providers

Things to do:

- Implement provider boundaries with mock fallbacks.
- Keep credentials out of UI payloads, logs, evidence, and memory.
- Emit source-fetch evidence and clear failure modes.
- Preserve cancellation and timeout behavior.

Status: planned

### 005_003: Add Repository Intelligence

Things to do:

- Add code search, dependency analysis, symbol discovery, and file summarization.
- Detect project types and likely build/test commands.
- Return bounded source-linked findings.
- Exclude generated, dependency, cache, build, and secret files.

Status: planned

### 005_004: Formalize Investigator Agent

Things to do:

- Produce related files, business flow, technical flow, risks, complexity, and recommended plan.
- Attach source references and selection rationale.
- Keep the agent read-only and replaceable behind Core contracts.
- Store a typed investigation artifact.

Status: planned

### 005_005: Add Architect Agent

Things to do:

- Validate the plan against Project standards and current architecture.
- Detect cross-module, security, maintainability, and performance concerns.
- Propose safer alternatives without modifying the repository.
- Store a typed architecture-review artifact.

Status: planned

### 005_006: Add Plan Review And Approval UI

Things to do:

- Move the current investigation console into the Task View.
- Show investigation, architecture, evidence, risks, and plan artifacts.
- Allow approve, reject, or request edits.
- Keep the workspace read-only until plan approval succeeds.

Status: planned

