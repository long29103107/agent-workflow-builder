# Phase 001: Skeleton And Operating System

Goal: keep the MVP runnable while establishing the source-of-truth architecture and repo-local Codex workflow.

## Tasks

### 001_001: Establish Source-Of-Truth Core

Things to do:

- Keep `AgentWorkflow.Core` as source of truth.
- Keep API, CLI, MCP, and UI as thin adapters.
- Preserve mock-first workflow execution.
- Verify API, CLI, and MCP builds.

Status: done

### 001_002: Setup Bun Frontend Stack

Things to do:

- Use Bun CLI for frontend install and scripts.
- Keep React + Vite UI runnable.
- Remove npm lockfile.
- Add Bun lockfile and Docker support.

Status: done

### 001_003: Setup Repo-Local Codex Skills And Agents

Things to do:

- Install repo-local skills under `.codex/skills/`.
- Create `.codex/agents/` roles split by responsibility.
- Update `AGENTS.md` with skill and agent routing.

Status: done

### 001_004: Setup Phase Task Memory Workflow

Superseded by `001_022`, which replaces Markdown task memory files with CodeGraph.

Things to do:

- Add durable project context, later superseded by `docs/knowledge/`.
- Add `.codex/phases/`.
- Add task ID convention `PPP_TTT`.
- Add task memory templates and initial memories.
- Update AGENTS and prompts to use phases and memories.

Status: done

### 001_005: Visualize Project Workflow

Things to do:

- Create root `index.html`.
- Show project purpose.
- Draw workflow from UI/API/CLI/MCP to Core and outputs.

Status: done

### 001_006: Restructure React Investigation Console

Things to do:

- Split the React UI into common feature folders.
- Keep API calls, workflow types, state hooks, and presentation components separate.
- Preserve the existing investigation console behavior.
- Verify the Bun frontend build.

Status: done

### 001_007: Split AgentWorkflow.Api Startup

Things to do:

- Move API service registration out of `Program.cs`.
- Move Minimal API endpoint mapping out of `Program.cs`.
- Preserve the existing HTTP routes and response behavior.
- Verify the API project build.

Status: done

### 001_008: Split AgentWorkflow.Cli Entrypoint

Things to do:

- Move CLI argument parsing out of `Program.cs`.
- Move CLI workflow execution and JSON output into a runner.
- Move CLI service registration into an extension method.
- Preserve the existing command arguments and JSON output behavior.
- Verify the CLI project build and smoke command.

Status: done

### 001_009: Split AgentWorkflow.Core Infrastructure

Things to do:

- Split subagents, Lead Agent, workflow engine, mock MCP tools, repository reader, memory service, and settings store into focused files.
- Keep `AgentWorkflow.Core` as the source of truth and preserve existing contracts.
- Preserve mock-first workflow behavior and OpenAI fallback behavior.
- Verify Core build and CLI smoke command.

Status: done

### 001_010: Split AgentWorkflow.Mcp Stdio Adapter

Things to do:

- Move MCP service registration out of `Program.cs`.
- Move stdio JSON request loop into a server class.
- Move MCP request contracts into a focused file.
- Preserve stdout JSON response behavior and stderr readiness logging.
- Verify the MCP project build and a stdio smoke request.

Status: done

### 001_011: Migrate Project Knowledge To Open Knowledge Format

Things to do:

- Create `docs/knowledge/` with Markdown files that use YAML frontmatter.
- Convert repository overview, architecture, service, business, data, integration, runbook, standard, and ADR knowledge.
- Mark inferred or unclear information explicitly.
- Update `AGENTS.md` with the knowledge-first workflow.
- Verify documentation changes do not break the .NET build.

Status: done

### 001_012: Prune Open Knowledge Format Noise

Things to do:

- Remove one-off migration prompt files after the knowledge base exists.
- Remove placeholder knowledge files that only document missing processes.
- Keep `docs/knowledge/` concise for Codex and humans.
- Preserve `.codex` workflow files used for task tracking and repo-local skills.
- Verify documentation cleanup does not break the .NET build.

Status: done

### 001_013: Prune Superseded Codex Context

Superseded in part by `001_022`, which removes `.codex/memories` from active routing.

Things to do:

- Remove `.codex/context/` after durable project knowledge moved to `docs/knowledge/`.
- Remove `.codex/memories/.gitkeep` because task memories now exist.
- Update Codex config, prompts, agents, skills, and AGENTS guidance to use `docs/knowledge/`.
- Keep `.codex` assets that still work with docs: agents, prompts, skills, phases, and task memories.
- Verify documentation cleanup does not break the .NET build.

Status: done

### 001_014: Prune Repo-Local Codex Agents

Things to do:

- Keep only the seven primary repo-local Codex agents.
- Remove optional specialist agent files that are not needed for the current MVP workflow.
- Update `AGENTS.md`, `.codex/agents/README.md`, and prompts so future work routes through the smaller agent set.
- Add a matching task memory entry.

Status: done

### 001_015: Compact Phase Task Memories

Superseded by `001_022`, which replaces compact Markdown task memories with CodeGraph.

Things to do:

- Compact completed Phase 001 task memories into one phase-level summary file.
- Preserve every task ID and high-signal implementation/verification notes.
- Update AGENTS, prompts, phase guidance, and memory template to describe compact memories.
- Keep `.codex/memories/tasks/` useful without one file per historical task.

Status: done

### 001_016: Add API Swagger And Scalar

Memory note requirement superseded by `001_022`; API documentation URLs remain current.

Things to do:

- Add OpenAPI/Swagger JSON for `AgentWorkflow.Api`.
- Add Scalar API reference UI.
- Keep API startup thin through extension methods.
- Update README, knowledge, and task memory with the documentation URLs.
- Verify the API project build.

Status: done

### 001_017: Realign Backlog To GitHub PR Workflow

Things to do:

- Rewrite `BACKLOG.md` around the target flow from Jira/Notion work item to GitHub clone, branch, code change, push, and draft PR.
- Move memory and advanced orchestration later in the roadmap.
- Preserve mock-first vertical slice sequencing.

Status: done

### 001_018: Realign Phase Files To GitHub PR Workflow

Things to do:

- Update `.codex/phases/README.md` to match the GitHub-to-PR roadmap.
- Replace old phase files for real MCP, repo intelligence, and advanced orchestration with the new phase sequence.
- Add phase files for work item intake, plan approval and branch execution, push and draft PR, real code agent, repo intelligence, memory, and advanced orchestration.
- Update the compact Phase 001 memory.

Status: done

### 001_019: Add Adapter Usage Guide

Things to do:

- Create a standalone Vietnamese HTML guide for using and smoke-testing API, UI, CLI, and MCP.
- Keep commands, ports, payloads, and expected results aligned with current source and knowledge runbooks.
- Clearly distinguish adapters that require the API from adapters that call Core directly.

Status: done

### 001_020: Realign Backlog To AI Engineering Workspace

Things to do:

- Update BACKLOG.md from the investigation-only roadmap to the full Project-to-approved-PR engineering workspace.
- Move persistence, approval, evidence, and sandbox controls before code and GitHub write agents.
- Realign Phase 002 through Phase 009 files with the new dependency order.
- Preserve completed Phase 001 work and the delivered 002_001 repository connection boundary.

Status: done

### 001_021: Add Swagger UI

Things to do:

- Keep the existing built-in OpenAPI JSON endpoint and Scalar UI.
- Add Swagger UI at /swagger using the existing /swagger/v1/swagger.json document.
- Keep API documentation registration and mapping outside Program.cs.
- Update README and API knowledge, then verify build and integration tests.

Status: done

### 001_022: Replace Markdown Task Memories With CodeGraph

Things to do:

- Use CodeGraph as the repo-local searchable code/task context index.
- Remove `.codex/memories` Markdown task memory workflow from active routing.
- Update AGENTS, config, prompts, agents, phase guidance, README, and knowledge docs.
- Ignore `.codegraph/` local SQLite indexes and document install/init commands.

Status: done

### 001_023: Restructure UI Into Agent Workspace Dashboard

Things to do:

- Replace the investigation-console layout with a dashboard shell and sidebar.
- Add a request intake area for direct user requests.
- Show an Agent Planner breakdown from the current request and selected task.
- Add a Kanban-style processing section for backlog, queued, processing, and completed work.
- Keep repository and API key configuration in a sidebar section without changing backend secret contracts.

Status: done

### 001_024: Add Kanban Task Pipeline Status

Things to do:

- Add a GitHub-style pipeline status section for the currently queued or processing task.
- Show pipeline stages when a Todo item starts processing.
- Keep the Kanban board and pipeline components split by responsibility.
- Verify the Bun frontend build.

Status: done
sib
### 001_025: Add Multiple UI Workspaces

Things to do:

- Add multiple local workspaces to the React dashboard.
- Treat each workspace as a project with its own request, planner, local Kanban, repository target, and API key state.
- Add workspace selection and creation in the sidebar.
- Keep API-backed task source and scheduler behavior compatible with the current backend contract.
- Verify the Bun frontend build.

Status: done
