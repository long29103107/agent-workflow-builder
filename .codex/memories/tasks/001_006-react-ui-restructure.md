# 001_006: Restructure React Investigation Console

## Phase

001: Skeleton And Operating System

## Task

001_006: Restructure React Investigation Console

## Goal

Split the React investigation console into a common frontend structure while preserving the existing behavior and API contracts.

## Implementation Log

- Added a task entry to `.codex/phases/001-skeleton.md`.
- Moved shared API calls into `src/agent-workflow-ui/src/api/client.ts`.
- Moved workflow DTOs into `src/agent-workflow-ui/src/types/workflow.ts`.
- Added `useInvestigationConsole` to own page state, loading, settings, and investigation actions.
- Split presentation into feature components for tasks, investigation lane, settings, run status, timeline, results, and topbar.
- Reduced `main.tsx` to application rendering only.
- Added React type declaration packages for the JSX runtime after the restructure exposed missing `react/jsx-runtime` typings in editors.

## Verification

- `bun run build` passed from `src/agent-workflow-ui`.
- `bun run build` passed again after adding `@types/react` and `@types/react-dom`.

## Goal Achieved

Yes. The UI is now split into conventional API, types, hook, component, and feature folders while preserving the investigation console behavior.

## Next Idea

Add focused component tests or a lightweight browser smoke check once the UI grows more interaction paths.
