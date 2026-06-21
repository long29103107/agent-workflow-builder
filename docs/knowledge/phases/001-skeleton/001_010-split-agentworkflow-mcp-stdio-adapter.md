---
type: phase-task
schema_version: 1
task_id: 001_010
phase: 001
status: done
updated_at: unknown
depends_on: none
verification: not_recorded
outcome: migrated_from_legacy_phase_status
---

# 001_010: Split AgentWorkflow.Mcp Stdio Adapter

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Move MCP service registration out of `Program.cs`.
- [x] Move stdio JSON request loop into a server class.
- [x] Move MCP request contracts into a focused file.
- [x] Preserve stdout JSON response behavior and stderr readiness logging.
- [x] Verify the MCP project build and a stdio smoke request.

## Progress

- Status: `done`
- Completed items: `5/5`
