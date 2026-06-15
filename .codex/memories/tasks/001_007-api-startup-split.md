# 001_007: Split AgentWorkflow.Api Startup

## Phase

001: Skeleton And Operating System

## Task

001_007: Split AgentWorkflow.Api Startup

## Goal

Refactor `src/AgentWorkflow.Api` so `Program.cs` stays thin while preserving the existing Minimal API routes and behavior.

## Implementation Log

- Added the task entry to `.codex/phases/001-skeleton.md`.
- Moved API framework and Core service registration into `Extensions/ServiceCollectionExtensions.cs`.
- Moved all `/api` Minimal API route mapping into `Endpoints/AgentWorkflowApiEndpoints.cs`.
- Reduced `Program.cs` to host creation, service registration, pipeline setup, endpoint mapping, and run.

## Verification

- `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed with 0 warnings and 0 errors.

## Goal Achieved

Yes. `Program.cs` is thin and the API project still builds.

## Next Idea

Split endpoint groups by feature if the API surface grows beyond the current MVP routes.
