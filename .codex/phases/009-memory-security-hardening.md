# Phase 009: Memory, Security, And Workflow Hardening

Goal: add durable learning, authorization, recovery, and policy hardening after the approved task-to-PR workflow works end to end.

## Tasks

### 009_001: Ingest Durable Run Memory

Things to do:

- Extract useful decisions, file findings, outcomes, review notes, and failures from completed runs.
- Link memory to project, task, run, agent, and evidence IDs.
- Skip secrets, credentials, hidden reasoning, and noisy transient logs.
- Add retention and deletion policy.

Status: planned

### 009_002: Add Qdrant Vector Search

Things to do:

- Implement vector search behind the Core memory boundary.
- Keep PostgreSQL as the authoritative workflow store.
- Add collection, embedding, filtering, and reindex configuration.
- Preserve mock fallback for local tests.

Status: planned

### 009_003: Add Neo4j Graph Memory And Entity Linking

Things to do:

- Link project, task, repository, file, branch, commit, PR, decision, and agent entities.
- Add deterministic identity and relationship rules.
- Keep graph writes derived, auditable, and rebuildable.
- Preserve mock fallback.

Status: planned

### 009_004: Add Authentication, Authorization, And Tenancy

Things to do:

- Add user identity and project ownership.
- Authorize project settings, approvals, artifacts, GitHub actions, and merge.
- Keep credential and installation secrets server-side.
- Add audit events for access and policy decisions.

Status: planned

### 009_005: Add Recovery And Safe Retry Policies

Things to do:

- Add recovery paths for clone, execution, verification, push, PR, CI, and provider failures.
- Retry only classified safe operations.
- Preserve failed artifacts and evidence for diagnosis.
- Add backup and restore procedures for authoritative state.

Status: planned

### 009_006: Add Parallelism And Policy Hardening

Things to do:

- Parallelize only independent read-only or explicitly approved subagent work.
- Preserve central orchestration and deterministic aggregation.
- Add tests proving agents cannot bypass approval gates, protected paths, workspace roots, or tool policy.
- Harden the full plan-to-merge workflow.

Status: planned
