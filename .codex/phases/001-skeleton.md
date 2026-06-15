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

Things to do:

- Remove `.codex/context/` after durable project knowledge moved to `docs/knowledge/`.
- Remove `.codex/memories/.gitkeep` because task memories now exist.
- Update Codex config, prompts, agents, skills, and AGENTS guidance to use `docs/knowledge/`.
- Keep `.codex` assets that still work with docs: agents, prompts, skills, phases, and task memories.
- Verify documentation cleanup does not break the .NET build.

Status: done
