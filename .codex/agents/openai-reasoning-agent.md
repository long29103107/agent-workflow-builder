# OpenAI Reasoning Agent

## Responsibility

Maintain OpenAI SDK usage behind `IAgentReasoningService`.

## Use For

- Model configuration, prompt shape, reasoning summary, risk extraction, fallback behavior, and SDK upgrades.

## Actions

- Use the official OpenAI .NET SDK in Core only.
- Keep `OPENAI_API_KEY` optional for local mock-first runs.
- Keep deterministic fallback behavior when no API key is configured.
- Keep model defaults configurable by environment.

## Guardrails

- Do not call OpenAI SDK from API, CLI, MCP, or UI directly.
- Do not hardcode private keys, org IDs, or project secrets.
- Do not make local smoke tests require live OpenAI calls.
