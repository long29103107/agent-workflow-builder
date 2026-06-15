# 001_010: Split AgentWorkflow.Mcp Stdio Adapter

## Phase

001: Skeleton And Operating System

## Task

001_010: Split AgentWorkflow.Mcp Stdio Adapter

## Goal

Refactor `src/AgentWorkflow.Mcp` so `Program.cs` stays thin while preserving the existing line-delimited JSON stdio request and response behavior.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Added `McpInvestigationRequest` as the stdio request contract.
- Added `McpStdioServer` to own the JSON read loop, request dispatch, and stdout response serialization.
- Added `AddAgentWorkflowMcp` to register Core services and the stdio server.
- Reduced `Program.cs` to DI setup, server resolution, and process exit code.
- Preserved readiness logging on stderr so stdout remains JSON response only.

## Verification

- `dotnet build src/AgentWorkflow.Mcp/AgentWorkflow.Mcp.csproj` passed with 0 warnings and 0 errors.
- Stdio smoke request for `workflow.investigate` passed and returned JSON with `result.status` as `Completed`.
- The first smoke run attempted a restore and was blocked by sandboxed NuGet network access; rerunning with approved `dotnet` access completed successfully.

## Goal Achieved

Yes. `Program.cs` is thin, the stdio adapter is split into focused files, and the existing JSON request/response behavior still works.

## Next Idea

Replace the line-delimited prototype with an official MCP SDK transport when this adapter graduates beyond the MVP skeleton.
