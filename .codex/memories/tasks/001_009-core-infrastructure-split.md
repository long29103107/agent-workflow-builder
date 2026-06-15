# 001_009: Split AgentWorkflow.Core Infrastructure

## Phase

001: Skeleton And Operating System

## Task

001_009: Split AgentWorkflow.Core Infrastructure

## Goal

Refactor `src/AgentWorkflow.Core` infrastructure into focused files while preserving the existing contracts, mock-first behavior, and shared source-of-truth role.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Split `Agents.cs` into individual subagent files, `OpenAiLeadAgent`, and `WorkflowEngine`.
- Split `MockExternalTools.cs` into mock MCP tools, repository reader/default path resolver, mock memory service, and settings store.
- Kept existing namespaces and DI registrations compatible with the current API, CLI, MCP, and UI adapters.

## Verification

- `dotnet build src/AgentWorkflow.Core/AgentWorkflow.Core.csproj` passed with 0 warnings and 0 errors.
- `dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .` passed and returned JSON with `Status: Completed`.

## Goal Achieved

Yes. Core infrastructure is split into focused files and the shared workflow still completes successfully.

## Next Idea

Move mock seed data into fixture/provider classes once real Jira, Notion, Qdrant, or Neo4j implementations are introduced.
