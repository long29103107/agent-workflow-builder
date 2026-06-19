# Phase 003: Durable Orchestration, Approval, And Evidence

Goal: make every workflow resumable, auditable, and unable to bypass approval policy.

## Tasks

### 003_001: Add Durable Workflow State Machine

Things to do:

- Model workflow stages and legal transitions in Core.
- Keep the Lead Agent as the single transition authority.
- Persist current stage, attempt, result, and failure details.
- Reject invalid or out-of-order transitions.

Status: planned

### 003_002: Add Background Workflow Worker

Things to do:

- Move long-running workflow execution out of HTTP requests.
- Persist and enqueue stage work before returning from the API.
- Add lease, cancellation, heartbeat, and graceful-shutdown behavior.
- Keep adapters thin.

Status: planned

### 003_003: Add Structured Evidence And Artifacts

Things to do:

- Add AgentExecution, EvidenceItem, and Artifact models.
- Store structured rationale, source references, actions, and tool results.
- Keep evidence append-only and redact secrets.
- Do not persist hidden chain-of-thought.

Status: planned

### 003_004: Add Approval Policy Engine

Things to do:

- Model investigation-plan, implementation, pull-request, and merge gates.
- Bind approvals to artifact hashes, target branches, or commit SHAs.
- Invalidate stale approvals when approved inputs change.
- Enforce policy in Core and write adapters, not only in the UI.

Status: planned

### 003_005: Add Task History And SSE Activity

Things to do:

- Project workflow, agent, approval, evidence, and artifact events into task history.
- Add a resumable server-sent event stream.
- Keep event ordering and correlation IDs stable.
- Preserve the current event endpoint during migration.

Status: planned

### 003_006: Add Idempotency And Stage Recovery

Things to do:

- Add idempotency keys to stage commands and external writes.
- Add safe retry rules for transient read operations.
- Prevent blind retry of commit, push, PR, or merge actions.
- Add recovery tests for interrupted workflows.

Status: planned

### 003_007: Add Mock-First Priority Task Scheduler

Things to do:

- Add a Core-owned in-memory priority queue over the current task source.
- Claim queued tasks in priority and FIFO order, then process them through the shared workflow engine.
- Add thin API endpoints to enqueue, inspect, and process the next task.
- Add React queue controls and monitoring without duplicating scheduling rules.
- Add automated Core tests for priority order, FIFO tie-breaking, validation, and processing.
- Defer CLI/MCP contract changes and durable background processing to their planned phases.

Status: done
