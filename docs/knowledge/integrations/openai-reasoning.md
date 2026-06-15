---
type: integration
title: OpenAI Reasoning Integration
domain: integrations
owner: project
status: draft
last_updated: 2026-06-15
tags:
  - integration
  - openai
  - reasoning
---

# OpenAI Reasoning Integration

## Purpose

Summarize investigation output through the official OpenAI .NET SDK when configured.

## Current Implementation

`OpenAiAgentReasoningService` creates an `OpenAI.Chat.ChatClient` only when `OPENAI_API_KEY` is present.

If no API key is configured, it returns deterministic fallback output so local development stays runnable.

## Configuration

- `OPENAI_API_KEY`: required for live OpenAI SDK calls.
- `OPENAI_MODEL`: optional model override. The current default is `gpt-5.1`.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/OpenAiAgentReasoningService.cs`
- `src/AgentWorkflow.Core/Application/WorkflowContracts.cs`

## Related Knowledge

- [Investigation Workflow Rules](../business/investigation-workflow.md)

## Open Questions

- Production model selection policy is not detected from repository analysis.
- Human approval policy for OpenAI-generated plans is not implemented.
