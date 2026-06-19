# Phases And Tasks

Tasks use this ID format:

```text
PPP_TTT
```

- `PPP`: three-digit phase number.
- `TTT`: three-digit task number inside the phase.
- Example: `001_002` means Phase 001, Task 002.

## Phase Index

- `001-skeleton.md`: runnable skeleton and repo-local operating system.
- `002-platform-foundation.md`: engineering baseline, Project, EngineeringTask, typed lifecycle, PostgreSQL persistence.
- `003-durable-orchestration.md`: background worker, durable state machine, approvals, evidence, artifacts.
- `004-sandbox-github-workspace.md`: isolated execution, GitHub clone, checkout, branch, protected-path policy.
- `005-investigation-architecture.md`: WorkItem intake, repo intelligence, Investigator, Architect, plan approval.
- `006-implementation-testing-review.md`: Coder, Tester, Reviewer, diffs, verification, implementation approval.
- `007-pull-request-lifecycle.md`: GitHub App, commit, push, PR Agent, checks, merge approval.
- `008-workspace-ui-observability.md`: engineering workspace UI, activity, usage, cost, metrics, traces.
- `009-memory-security-hardening.md`: durable memory, Qdrant, Neo4j, auth, recovery, policy hardening.

## Working Rule

Before implementing a task:

1. Select or create a task ID in a phase file.
2. Read matching memories in `.codex/memories/tasks/`; completed tasks may be compacted into phase-level memory files.
3. Implement the task.
4. Write or update a memory note that includes the same task ID.
