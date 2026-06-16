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
- `002-github-repository-workspace.md`: GitHub repository connection, clone workspace, checkout, repository context.
- `003-real-mcp.md`: Jira MCP, Notion MCP, auth, tool execution logs.
- `004-repo-intelligence.md`: GitHub/GitLab, code search, dependency graph, file summarization.
- `005-advanced-orchestration.md`: parallel agents, retries, approval gates, human review, plan-to-PR.

## Working Rule

Before implementing a task:

1. Select or create a task ID in a phase file.
2. Read matching memories in `.codex/memories/tasks/`; completed tasks may be compacted into phase-level memory files.
3. Implement the task.
4. Write or update a memory note that includes the same task ID.
