# Phase 002 Memory: Platform Foundation

## Phase

002: Platform Foundation

## 002_001: Add GitHub Repository Connection Boundary

- Goal: introduce a mock-first GitHub repository target boundary so workflow runs can carry a repository URL before real clone and checkout are implemented.
- Implementation: added `RepositoryConnection` and `IRepositoryConnectionService` in Core; added `MockRepositoryConnectionService`; changed repository context reads to resolve a local path or mock GitHub URL; exposed repository connection through API endpoints, settings, CLI `--repo-url`, MCP request payload, and UI settings fields; updated README and knowledge docs.
- Verification: `dotnet build src\AgentWorkflow.Api\AgentWorkflow.Api.csproj` passed; `dotnet build src\AgentWorkflow.Cli\AgentWorkflow.Cli.csproj` passed; `dotnet build src\AgentWorkflow.Mcp\AgentWorkflow.Mcp.csproj` passed; CLI smoke with `--repo-url https://github.com/example/repository` returned `Status: Completed` and `mock-git://example/repository`; MCP stdio smoke returned `status: Completed` with the same mock GitHub context. `bun run build` could not run because `vite` was missing and `bun install` needed tempdir access; user denied escalated install.
- Next idea: complete 002_002 by restoring the solution and automated test baseline before Project, EngineeringTask, persistence, and sandbox work.

## 002_002: Restore Solution And Test Baseline

- Goal: restore one solution-level build/test gate before expanding the platform.
- Implementation: added AgentWorkflowBuilder.slnx with all four source projects, AgentWorkflow.Core.Tests, and AgentWorkflow.Api.Tests; added xUnit unit tests and ASP.NET Core integration-test infrastructure.
- Verification: solution restore completed from the existing package cache; solution build passed with 0 warnings and 0 errors; dotnet test passed 6 tests; existing CLI and MCP investigation smoke flows remained Completed.
- Next idea: continue with 002_003 Project aggregate and policies while keeping the scheduler compatible with the current mock TaskItem source.
