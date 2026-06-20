---
type: service
title: Agent Workflow UI
domain: frontend
owner: project
status: draft
last_updated: 2026-06-20
tags:
  - service
  - react
  - vite
---

# Agent Workflow UI

## Purpose

Provide the React agent workspace dashboard for capturing user requests, showing previous requests, showing a planner breakdown, processing work through a Kanban board, saving repository settings, and viewing run output.

## Responsibilities

- Render a dashboard shell with sidebar navigation.
- Split the dashboard into client-side routes: `/request`, `/planner`, `/kanban`, and `/configuration`.
- Keep page and section components split by responsibility.
- Capture a direct user request in the Request page.
- Show previous submitted requests below the request input.
- Submit requests into Agent Planner logs with `Pending approval` status.
- Allow approved planner logs to generate Kanban backlog tasks.
- Show an Agent Planner breakdown in its own routed section.
- Load tasks from the API as the request pool.
- Render the basic Kanban flow: Backlog, Todo, In Progress, Code Review, Testing, and Done.
- Show a GitHub-style task pipeline for the currently queued or processing Kanban task.
- Queue selected tasks, refresh scheduler state, and process the next priority item.
- Load and update repository/API session settings.
- Keep an API key field in UI session state only.
- Start workflow investigations against a local repository path or mock GitHub repository URL.
- Fetch workflow events and render run status, timeline, and results.

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

- Planner logs are generated from submitted free-form request text and wait for user approval before creating Kanban tasks.
- Request history is local UI session state in this slice.
- Planner approval and generated Kanban tasks are local UI session state in this slice.
- Queueing or running investigation requires a selected task.
- Repository settings save is scoped to the API session because the current settings store is in memory.
- The API key field is not sent to the backend in this slice because the backend settings contract does not contain a secret field.
- Repository URL settings select a mock GitHub workspace target until real clone and checkout are implemented.
- If settings cannot load, the UI shows a local mock settings fallback message.
- Priority and ordering decisions remain in Core; the UI only submits and displays scheduler state.
- Current Core scheduler states map to Backlog from task source, Todo from queued tasks, In Progress from processing tasks, and Done from completed tasks. Code Review and Testing are placeholder lanes until the backend lifecycle expands.
- Todo cards can be started from the Kanban board, including by dropping a Todo card onto In Progress. Local planner-generated tasks move directly to Processing; API-backed queued tasks use the current process-next scheduler endpoint.
- The task pipeline section reflects the active Queued, Processing, or Completed scheduler item. Code Review and Testing remain visual pipeline stages until backend statuses exist for those lifecycle steps.

## Configuration

- `VITE_API_BASE_URL`: API base URL.

## Related Files

- `src/agent-workflow-ui/src/api/client.ts`
- `src/agent-workflow-ui/src/hooks/useInvestigationConsole.ts`
- `src/agent-workflow-ui/src/App.tsx`
- `src/agent-workflow-ui/src/layout/DashboardSidebar.tsx`
- `src/agent-workflow-ui/src/pages/RequestPage.tsx`
- `src/agent-workflow-ui/src/routes/workspaceRoutes.ts`
- `src/agent-workflow-ui/src/sections/PlannerSection.tsx`
- `src/agent-workflow-ui/src/sections/KanbanSection.tsx`
- `src/agent-workflow-ui/src/sections/PipelineStatusSection.tsx`
- `src/agent-workflow-ui/src/sections/ConfigurationSection.tsx`
- `src/agent-workflow-ui/src/styles.css`
- `src/agent-workflow-ui/package.json`

## Related Knowledge

- [Local Development](../runbooks/local-development.md)
- [Investigation Workflow Rules](../business/investigation-workflow.md)
