---
type: service
title: Agent Workflow UI
domain: frontend
owner: project
status: draft
last_updated: 2026-06-16
tags:
  - service
  - react
  - vite
---

# Agent Workflow UI

## Purpose

Provide the React investigation console for selecting tasks, saving mock endpoint settings, starting investigations, and viewing results.

## Responsibilities

- Load tasks from the API.
- Load and update API session settings.
- Start workflow investigations against a local repository path or mock GitHub repository URL.
- Fetch workflow events.
- Render task, investigation, settings, run status, timeline, and result views.

## Main APIs / Entry Points

```powershell
cd src/agent-workflow-ui
bun run dev
```

The UI defaults to `http://localhost:5275/api` unless `VITE_API_BASE_URL` is set.

## Dependencies

- React 19
- Vite 7
- Bun package manager
- [AgentWorkflow.Api](agentworkflow-api.md)

## Data Models

TypeScript types mirror Core workflow records in `src/agent-workflow-ui/src/types/workflow.ts`.

## Business Rules

- Investigation cannot start without a selected task.
- Settings save is scoped to the API session because the current settings store is in memory.
- Repository URL settings select a mock GitHub workspace target until real clone and checkout are implemented.
- If settings cannot load, the UI shows a local mock settings fallback message.

## Configuration

- `VITE_API_BASE_URL`: API base URL.

## Related Files

- `src/agent-workflow-ui/src/api/client.ts`
- `src/agent-workflow-ui/src/hooks/useInvestigationConsole.ts`
- `src/agent-workflow-ui/package.json`

## Related Knowledge

- [Local Development](../runbooks/local-development.md)
- [Investigation Workflow Rules](../business/investigation-workflow.md)
