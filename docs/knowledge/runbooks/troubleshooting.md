---
type: runbook
title: Troubleshooting
domain: operations
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - runbook
  - troubleshooting
---

# Troubleshooting

## Purpose

Capture known troubleshooting steps for local development and adapter smoke tests.

## API Build Output Locked

Symptom: build fails because an `.exe` or `.dll` is locked.

Action: stop the running API, CLI, or MCP process and rerun the targeted build.

## OpenAI Reasoning Not Used

Symptom: output says `Set OPENAI_API_KEY to enable OpenAI SDK reasoning.`

Action: set `OPENAI_API_KEY` and optionally `OPENAI_MODEL`, then rerun the workflow.

## MCP Stdio Consumers Fail To Parse Output

Symptom: a stdio client cannot parse server output.

Action: verify protocol responses are written to stdout and diagnostics are written to stderr. The current MCP adapter writes readiness to stderr and JSON responses to stdout.

## Frontend Cannot Reach API

Symptom: task loading or investigation API calls fail.

Action: verify the API is running and set `VITE_API_BASE_URL` if the API is not at `http://localhost:5275/api`.

## Related Knowledge

- [AgentWorkflow.Mcp](../services/agentworkflow-mcp.md)
- [Agent Workflow UI](../services/agent-workflow-ui.md)
- [Build](build.md)
