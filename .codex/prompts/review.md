# Review Prompt

Review changes for the Agent Workflow Builder MVP.

Use CodeGraph for repo-local context when `.codegraph/` is initialized; otherwise fall back to focused `rg` and file reads.

Prioritize:

- Runtime regressions in the end-to-end investigation flow.
- Broken API contracts between `src/AgentWorkflow.Api` and `src/agent-workflow-ui`.
- Missing cancellation-token plumbing in async backend paths.
- Drift from the central Lead Agent / workflow engine architecture.
- Docs that no longer match ports, paths, commands, or API shapes.

Treat mock integrations as intentional unless the change explicitly asks for real Jira, Notion, Qdrant, Neo4j, Git, or LLM providers.
