# Phase 008: Engineering Workspace UI And Observability

Goal: expose projects, tasks, agents, pull requests, approvals, evidence, and operational usage as one engineering control plane.

## Tasks

### 008_001: Add Workspace Navigation

Things to do:

- Add Projects, Tasks, Agents, Pull Requests, and Activity Feed routes.
- Keep the current investigation console available during migration.
- Add responsive loading, empty, error, and permission states.
- Keep API access in the frontend client layer.

Status: planned

### 008_002: Add Project Dashboard And Settings

Things to do:

- Show repository connection, task counts, active runs, PRs, and activity.
- Edit agent configuration, commands, conventions, protected paths, and approval policy.
- Keep secrets out of browser-readable settings.
- Surface policy validation clearly.

Status: planned

### 008_003: Add Full Task View

Things to do:

- Add overview, investigation, plan, agents, evidence, diff, tests, review, approvals, and PR tabs.
- Show current stage and pending gate.
- Support approve, reject, edit, retry, and cancel actions.
- Stream activity without polling the full run.

Status: planned

### 008_004: Add Agent, Pull Request, And Activity Views

Things to do:

- Show active agents, execution status, history, artifacts, and failures.
- Show PR state, checks, reviews, and linked tasks.
- Add a filterable cross-project activity feed.
- Keep evidence links navigable.

Status: planned

### 008_005: Add Usage And Cost Tracking

Things to do:

- Track token usage, estimated cost, runtime, agent executions, and tool calls.
- Compute success and failure rates by project, workflow, stage, and agent.
- Store pricing/version metadata with cost estimates.
- Make missing provider usage explicit.

Status: planned

### 008_006: Add OpenTelemetry And Artifact Controls

Things to do:

- Add structured logs, traces, metrics, and correlation IDs.
- Redact secrets, prompts, source content, and sensitive command output.
- Add artifact retention, access, and download controls.
- Add dashboards for queue, stage, sandbox, provider, and workflow health.

Status: planned
