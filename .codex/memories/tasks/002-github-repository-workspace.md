# Phase 002 Memory: GitHub Repository Workspace

## Phase

002: GitHub Repository Workspace

## 002_001: Add GitHub Repository Connection Boundary

- Goal: introduce a mock-first GitHub repository target boundary so workflow runs can carry a repository URL before real clone and checkout are implemented.
- Implementation: added `RepositoryConnection` and `IRepositoryConnectionService` in Core; added `MockRepositoryConnectionService`; changed repository context reads to resolve a local path or mock GitHub URL; exposed repository connection through API endpoints, settings, CLI `--repo-url`, MCP request payload, and UI settings fields; updated README and knowledge docs.
- Verification: `dotnet build src\AgentWorkflow.Api\AgentWorkflow.Api.csproj` passed; `dotnet build src\AgentWorkflow.Cli\AgentWorkflow.Cli.csproj` passed; `dotnet build src\AgentWorkflow.Mcp\AgentWorkflow.Mcp.csproj` passed; CLI smoke with `--repo-url https://github.com/example/repository` returned `Status: Completed` and `mock-git://example/repository`; MCP stdio smoke returned `status: Completed` with the same mock GitHub context. `bun run build` could not run because `vite` was missing and `bun install` needed tempdir access; user denied escalated install.
- Next idea: clone the configured GitHub target into an isolated workflow workspace.
