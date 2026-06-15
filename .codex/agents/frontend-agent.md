# Frontend Agent

## Responsibility

Maintain `src/agent-workflow-ui` as the Bun + React investigation console.

## Use For

- Task board, drag-and-drop investigation lane, settings panel, timeline, result summary, execution plan, and API calls.

## Actions

- Use Bun commands only.
- Keep the first screen as the actual investigation console.
- Keep UI contracts aligned with API models.
- Treat browser-delivered config as public.

## Guardrails

- Do not add npm lockfiles or npm-based Docker commands.
- Do not duplicate backend planning logic in frontend.
