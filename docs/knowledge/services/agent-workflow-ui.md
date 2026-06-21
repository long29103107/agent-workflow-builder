---
type: service
title: Agent Workflow UI
domain: frontend
owner: project
status: draft
last_updated: 2026-06-21
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
- Allow multiple API-backed workspaces, where each workspace represents one project.
- Capture a Jira-style project code when creating a workspace and show it in the project selector.
- Capture a direct user request in the Request page.
- Show previous submitted requests below the request input.
- Submit requests into Agent Planner logs with `Pending approval` status.
- Edit pending planner steps, including their title, detail, and assigned agent, before approval.
- Allow approved planner logs to generate Kanban backlog tasks.
- Show an Agent Planner breakdown in its own routed section.
- Load tasks from the API as the request pool.
- Render the basic Kanban flow: Backlog, Todo, In Progress, Code Review, Testing, and Done.
- Keep Kanban lanes at a readable minimum width inside a contained horizontal scroller; long task, error, and agent text must not expand cards into adjacent lanes.
- Load enabled project agents and assign one to each Kanban task.
- Show a GitHub-style task pipeline for the currently queued or processing Kanban task.
- Queue selected tasks, refresh scheduler state, and process the next priority item.
- Support drag transitions from Backlog to Todo and from Todo to In Progress, backed by workspace scheduler APIs.
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
- Pending planner logs can be edited, expanded, or reduced before approval; approved logs are read-only.
- Planner step owners must be selected from the active project's enabled agents, and the owner becomes the generated task's initial assignment.
- Workspace projects, request history, planner logs, generated tasks, scheduler queues, and repository settings are loaded from workspace-scoped API endpoints.
- Request history is scoped to the active workspace.
- Planner approval and generated Kanban tasks are scoped to the active workspace.
- Kanban agent assignment is scoped to the active workspace and is carried into scheduler items and the investigation's requested-agent selection.
- Repository target and API key fields are scoped to the active workspace in the UI. Repository settings are saved to the workspace API; API keys remain local UI-only state.
- Queueing or running investigation requires a selected task.
- Repository settings save is scoped to the API session because the current settings store is in memory.
- The API key field is not sent to the backend in this slice because the backend settings contract does not contain a secret field.
- Repository URL settings select a mock GitHub workspace target until real clone and checkout are implemented.
- If settings cannot load, the UI shows a local mock settings fallback message.
- Priority and ordering decisions remain in Core; the UI only submits and displays scheduler state.
- Current Core scheduler states map to Backlog from task source, Todo from queued tasks, In Progress from processing tasks, and Done from completed tasks. Code Review and Testing are placeholder lanes until the backend lifecycle expands.
- Backlog cards can be dragged onto Todo to enqueue that exact task. Todo cards can be dragged onto In Progress to process that exact scheduled item. Unsupported placeholder lanes do not accept drops.
- The task pipeline section reflects the active Queued, Processing, or Completed scheduler item. Code Review and Testing remain visual pipeline stages until backend statuses exist for those lifecycle steps.
- Workspace switching reloads isolated requests, planner logs, enabled agents, tasks, scheduler state, and repository settings from the API.

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
