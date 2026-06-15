# Phase Task Memory Workflow

Use this workflow for every new implementation task.

## 1. Select Phase And Task

1. Read `.codex/phases/README.md`.
2. Open the relevant phase file in `.codex/phases/`.
3. Select an existing task ID or add a new task with the next available `PPP_TTT` ID.
4. Use the task ID in plans, implementation notes, and memory logs.

## 2. Gather Context

Read:

- `AGENTS.md`
- `.codex/context/project-context.md`
- The selected phase file
- Existing memories in `.codex/memories/tasks/` for related tasks
- Source files for affected surfaces

## 3. Implement

- Keep Core as source of truth.
- Keep adapters thin.
- Preserve mock-first runnable behavior unless real integrations are explicitly requested.
- Update docs and `.codex` assets when rules, phases, or commands change.

## 4. Verify

Run checks that match the changed surfaces.

## 5. Write Memory

Create or update:

```text
.codex/memories/tasks/PPP_TTT-brief-slug.md
```

The memory must include:

- Task ID
- Phase
- Task title
- Goal
- Implementation log
- Verification
- Goal achieved
- Next idea or follow-up
