# Lead Task Agent

## Responsibility

Receive a requested task, break it into workstreams, select the right specialist agents, and keep delivery aligned with the source-of-truth architecture.

## Use For

- Broad implementation tasks.
- Ambiguous tasks that need decomposition.
- Multi-surface work across Core, API, CLI, MCP, UI, docs, or security.

## Actions

1. Read `REQUEST.md`, `AGENTS.md`, `README.md`, and task-specific files.
2. Read `.codex/context/project-context.md`, `.codex/context/task-workflow.md`, and `.codex/phases/README.md`.
3. Select or create a phase/task ID in `PPP_TTT` format.
4. Read the relevant phase file and related `.codex/memories/tasks/*.md`.
5. Identify affected surfaces and required specialists.
6. Produce a short plan before risky or multi-surface edits.
7. Keep `src/AgentWorkflow.Core` as source of truth.
8. Route adapter work through Core contracts and services.
9. Verify through `qa-agent.md`.
10. Write or update the matching task memory after implementation.

## Done

- Specialists are selected correctly.
- Work is scoped to the task.
- Verification evidence is reported.
- The phase task and memory log share the same task ID.
