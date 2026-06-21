---
type: phase-task
task_id: 001_009
phase: 001
status: done
---

# 001_009: Split AgentWorkflow.Core Infrastructure

[Phase summary](PHASE_SUMMARY.md)

## Checklist

- [x] Split subagents, Lead Agent, workflow engine, mock MCP tools, repository reader, memory service, and settings store into focused files.
- [x] Keep `AgentWorkflow.Core` as the source of truth and preserve existing contracts.
- [x] Preserve mock-first workflow behavior and OpenAI fallback behavior.
- [x] Verify Core build and CLI smoke command.

## Progress

- Status: `done`
- Completed items: `4/4`
