# Backlog

## Product Goal

Build an agent workflow platform that can take work from Jira or Notion, connect to a GitHub repository, clone the project, investigate the task, create an implementation branch, make code changes, push the branch, and open a draft pull request.

## Phase 1: Runnable Skeleton

- React UI
- .NET API
- Mock agents
- Mock MCP tools
- Mock repo reader
- Basic workflow run
- Swagger/OpenAPI and Scalar API docs

## Phase 2: GitHub Repository Workspace

- GitHub repository connection settings
- GitHub authentication boundary
- Clone repository into an isolated workflow workspace
- Detect default branch and repository metadata
- Checkout and clean workspace per workflow run
- Repository context reads from the cloned target repository

## Phase 3: Work Item Intake

- Shared `WorkItem` model for Jira and Notion tasks
- Mock Jira/Notion adapters mapped into `WorkItem`
- Jira MCP provider behind the work item boundary
- Notion MCP provider behind the work item boundary
- Safe auth/config for task sources
- Source links and acceptance criteria in investigation context

## Phase 4: Plan Approval And Branch Execution

- Investigation plan generated from the cloned repo and selected work item
- Human approval gate before code execution
- Branch naming rules such as `agent/{taskId}-{short-title}`
- Create implementation branch in the cloned workspace
- Mock code-change executor for the first vertical slice
- Diff summary and execution timeline in the UI
- Commit generated changes after review

## Phase 5: Push And Draft PR

- Push implementation branch to GitHub
- Create draft pull request
- Add PR title/body from work item and execution summary
- Link PR URL back to the workflow run
- Track push and PR creation events
- Handle branch/PR failure states safely

## Phase 6: Real Code Agent

- Replace mock code-change executor with a real code agent boundary
- Apply approved plan steps to the cloned workspace
- Preserve generated diff for review before commit
- Run configured verification commands before push
- Capture logs without storing secrets

## Phase 7: Repo Intelligence Improvements

- Code search
- Dependency graph
- File summarization
- Symbol and project structure discovery
- Test/build command detection

## Phase 8: Memory And Learning

- Persist workflow runs and task outcomes
- Memory ingestion from completed runs
- Qdrant vector search
- Neo4j graph memory
- Entity linking across task, repo, branch, PR, and decision nodes

## Phase 9: Advanced Orchestration

- Retry policy for safe transient failures
- Approval gates for risky tool actions
- Human-in-the-loop review and edit flow
- Parallel subagent execution
- Plan-to-PR workflow hardening
