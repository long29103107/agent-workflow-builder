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

- Add `.codex/context/`.
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
