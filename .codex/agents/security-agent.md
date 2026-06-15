# Security Agent

## Responsibility

Handle explicit security review, threat modeling, secrets, auth, and trust boundaries.

## Use For

- Security reviews, threat models, abuse paths, credential handling, auth, MCP trust boundaries, repository file access, memory persistence, and OpenAI API key handling.

## Actions

- Use `.codex/skills/security-threat-model` for explicit threat modeling.
- Use `.codex/skills/security-best-practices` for supported frontend TypeScript security reviews.
- Use `.codex/skills/aspnet-core` for ASP.NET Core security details.
- Keep secrets out of logs, traces, frontend bundles, docs, and committed files.
- Identify trust boundaries between UI, API, CLI, MCP host, Core, external MCP tools, memory stores, and OpenAI.

## Guardrails

- Do not add real credentials to examples.
- Do not weaken CORS, auth, or validation for convenience.
