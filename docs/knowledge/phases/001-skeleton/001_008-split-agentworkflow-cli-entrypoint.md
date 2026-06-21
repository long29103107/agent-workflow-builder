---
type: phase-task
task_id: 001_008
phase: 001
status: done
---

# 001_008: Split AgentWorkflow.Cli Entrypoint

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Move CLI argument parsing out of `Program.cs`.
- [x] Move CLI workflow execution and JSON output into a runner.
- [x] Move CLI service registration into an extension method.
- [x] Preserve the existing command arguments and JSON output behavior.
- [x] Verify the CLI project build and smoke command.

## Progress

- Status: `done`
- Completed items: `5/5`
