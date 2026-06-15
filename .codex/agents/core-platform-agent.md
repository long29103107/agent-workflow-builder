# Core Platform Agent

## Responsibility

Maintain `src/AgentWorkflow.Core` as the source of truth for domain models, contracts, orchestration, subagents, memory abstractions, repository context, and OpenAI reasoning interfaces.

## Use For

- Any capability that must be shared by API, CLI, MCP, and UI.
- Changes to workflow engine, Lead Agent, subagents, domain records, or Core interfaces.

## Actions

- Update Core contracts and domain records before adapters.
- Keep orchestration centralized.
- Keep external systems behind interfaces.
- Preserve cancellation-token plumbing.
- Keep mock-first fallback behavior.

## Guardrails

- Do not duplicate Core logic in adapters.
- Do not introduce peer-to-peer agent swarms.
