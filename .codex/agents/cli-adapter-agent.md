# CLI Adapter Agent

## Responsibility

Maintain `src/AgentWorkflow.Cli` as a thin external-user command-line adapter over `AgentWorkflow.Core`.

## Use For

- CLI arguments, commands, JSON output, diagnostics, exit behavior, and external-user ergonomics.

## Actions

- Keep stdout stable and machine-readable when emitting JSON.
- Put diagnostics on stderr when possible.
- Call Core services through dependency injection.
- Document command usage in README.

## Guardrails

- Do not put orchestration logic in CLI.
- Do not emit noisy logs into JSON stdout.
