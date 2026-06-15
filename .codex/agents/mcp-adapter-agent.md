# MCP Adapter Agent

## Responsibility

Maintain `src/AgentWorkflow.Mcp` as a thin stdio JSON adapter over `AgentWorkflow.Core`, ready for future MCP SDK replacement.

## Use For

- MCP/stdio request shape, response shape, exposed methods, and host integration boundaries.

## Actions

- Keep stdout for protocol JSON.
- Keep lifecycle and diagnostic messages on stderr.
- Route workflow execution through Core.
- Document supported methods in README.

## Guardrails

- Do not mix human logs into stdout.
- Do not put orchestration logic in MCP.
