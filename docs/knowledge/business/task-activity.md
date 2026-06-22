---
type: business-rule
title: Task Activity And SSE Rules
domain: workflow
owner: project
status: implemented
last_updated: 2026-06-22
tags:
  - activity
  - history
  - sse
---

# Task Activity And SSE Rules

## Purpose

Provide one ordered, resumable task timeline across workflow, agent, approval, evidence, and artifact activity.

## History

- `TaskActivity.Sequence` is a durable, globally monotonic cursor and the authoritative ordering key.
- Every item carries task ID, optional workflow run ID, and stable correlation ID.
- History reads use an exclusive `afterSequence` cursor and a page size from 1 through 500.
- Activity summaries pass through the Core secret redactor before persistence.
- In-memory and PostgreSQL stores are append-only; PostgreSQL uses an identity-backed `bigint` sequence.

## SSE

- `GET /api/tasks/{taskId}/activity` emits sequence as SSE `id`, category as `event`, and the typed activity as JSON `data`.
- A valid `Last-Event-ID` header takes precedence over the initial `afterSequence` query during reconnect.
- Replay batches are limited to 1 through 500. A full batch closes the response so the client reconnects from the last delivered ID rather than buffering unbounded history.
- Idle streams emit heartbeat comments every 15 seconds and honor request cancellation.
- The existing `GET /api/workflows/{runId}/events` endpoint remains unchanged during migration.

## Related Files

- `src/AgentWorkflow.Core/Infrastructure/Activity/InMemoryTaskActivityStore.cs`
- `src/AgentWorkflow.Core/Infrastructure/Persistence/PostgresTaskActivityStore.cs`
- `src/AgentWorkflow.Api/Endpoints/TaskApiEndpoints.cs`
