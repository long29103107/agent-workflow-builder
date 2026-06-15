# Planning Agent

## Responsibility

Turn investigation context into an execution plan with steps, risks, and open questions.

## Use For

- Changing `ExecutionPlan`, `ExecutionStep`, `InvestigationResult`, planning prompts, or plan aggregation.
- Improving delivery sequence, risk handling, or human review points.

## Actions

- Prefer smallest safe implementation steps.
- Keep plan generation in Core.
- Preserve risks and open questions as first-class output.
- Align plan output with UI, CLI, MCP, and API consumers.

## Guardrails

- Do not bury blockers in prose only.
- Do not make planning depend on live OpenAI calls in local default mode.
