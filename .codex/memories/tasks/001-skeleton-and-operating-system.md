# Phase 001 Compact Memory: Skeleton And Operating System

## Phase

001: Skeleton And Operating System

## Purpose

Compact completed Phase 001 task memories into one searchable file. Each task ID remains a heading so future Codex runs can still match phase tasks to implementation history without opening many small files.

## 001_001: Establish Source-Of-Truth Core

- Goal: move orchestration source of truth into `AgentWorkflow.Core` so API, CLI, MCP, and UI share workflow behavior.
- Implementation: added Core domain models, contracts, orchestration, mock integrations, and OpenAI SDK reasoning; kept API, CLI, and MCP thin over Core; removed legacy `AgentFrameworkDemo.Console`; kept `IAgentReasoningService` with deterministic fallback when `OPENAI_API_KEY` is not configured.
- Verification: API, CLI, and MCP builds passed; CLI smoke test returned `Status: Completed`.
- Next idea: use this architecture before real memory, real MCP, repository intelligence, or advanced orchestration.

## 001_002: Setup Bun Frontend Stack

- Goal: make Bun the real frontend package and script runner.
- Implementation: added Bun `packageManager`, generated `bun.lock`, removed npm lockfile, updated frontend Dockerfile to use `oven/bun`, and ignored Bun cache/temp folders.
- Verification: `bun install` completed; `bun run build` passed.
- Next idea: keep frontend commands, Docker setup, and docs Bun-based.

## 001_003: Setup Repo-Local Codex Skills And Agents

- Goal: create repo-local Codex skills and agents for responsibility-based routing.
- Implementation: added `.codex/skills/`, `.codex/agents/`, `.codex/config.toml`, and `AGENTS.md` routing.
- Verification: `.codex/agents` contained README plus responsibility-specific files; `AGENTS.md` mapped agents to actions.
- Next idea: start broad work from `lead-task-agent.md`.

## 001_004: Setup Phase Task Memory Workflow

- Goal: add phase files, task IDs, and memory logging so tasks build from previous context.
- Implementation: added `.codex/phases/`, established `PPP_TTT`, added `.codex/memories/`, and updated AGENTS, config, prompts, and Lead Task Agent guidance. `.codex/context/` was later superseded by `docs/knowledge/` in `001_013`.
- Verification: confirmed phase files, memory structure, matching `001_004` task entry, and references in AGENTS/prompts.
- Next idea: select task ID, read related memories, implement, verify, then update memory.

## 001_005: Visualize Project Workflow

- Goal: create a static project overview page.
- Implementation: added root `index.html` showing UI/API/CLI/MCP adapters flowing into `AgentWorkflow.Core`, plus Lead Agent, subagents, repository context, memory, MCP tools, OpenAI reasoning, and investigation output.
- Verification: confirmed workflow labels and purpose sections; file opens directly as static HTML.
- Next idea: keep `index.html` aligned when architecture, phases, or adapters change.

## 001_006: Restructure React Investigation Console

- Goal: split the React investigation console into common frontend structure while preserving behavior and API contracts.
- Implementation: moved API calls to `src/api/client.ts`, workflow DTOs to `src/types/workflow.ts`, state to `useInvestigationConsole`, and presentation into feature components; reduced `main.tsx`; added React type declarations.
- Verification: `bun run build` passed before and after adding React type packages.
- Next idea: add focused component tests or browser smoke checks as UI paths grow.

## 001_007: Split AgentWorkflow.Api Startup

- Goal: keep API `Program.cs` thin while preserving Minimal API routes and behavior.
- Implementation: moved service registration to `Extensions/ServiceCollectionExtensions.cs`, route mapping to `Endpoints/AgentWorkflowApiEndpoints.cs`, and left `Program.cs` for host setup, DI, pipeline, endpoint mapping, and run.
- Verification: `dotnet build src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` passed.
- Next idea: split endpoint groups by feature if the API grows.

## 001_008: Split AgentWorkflow.Cli Entrypoint

- Goal: keep CLI `Program.cs` thin while preserving positional args, workflow execution, and JSON output.
- Implementation: added `CliOptions`, `CliRunner`, `AddAgentWorkflowCli`, and reduced `Program.cs` to parsing, DI, runner resolution, and exit code.
- Verification: CLI build passed; `dotnet run --project src/AgentWorkflow.Cli -- jira-awb-101 .` returned JSON with `Status: Completed`.
- Next idea: add explicit subcommands such as `investigate` and `doctor` when the CLI needs a durable external command surface.

## 001_009: Split AgentWorkflow.Core Infrastructure

- Goal: split Core infrastructure into focused files while preserving contracts and mock-first behavior.
- Implementation: split agents, `OpenAiLeadAgent`, `WorkflowEngine`, mock MCP tools, repository reader/default path resolver, mock memory service, and settings store; preserved namespaces and DI compatibility.
- Verification: Core build passed; CLI smoke command returned JSON with `Status: Completed`.
- Next idea: move mock seed data into fixture/provider classes when real Jira, Notion, Qdrant, or Neo4j implementations arrive.

## 001_010: Split AgentWorkflow.Mcp Stdio Adapter

- Goal: keep MCP `Program.cs` thin while preserving line-delimited JSON stdio behavior.
- Implementation: added `McpInvestigationRequest`, `McpStdioServer`, `AddAgentWorkflowMcp`, and reduced `Program.cs`; preserved stderr readiness logging so stdout stays JSON-only.
- Verification: MCP build passed; stdio smoke request for `workflow.investigate` returned JSON with `result.status` as `Completed`; first smoke run hit sandboxed NuGet restore and passed after approved dotnet access.
- Next idea: replace the line-delimited prototype with an official MCP SDK transport after the MVP skeleton.

## 001_011: Migrate Project Knowledge To Open Knowledge Format

- Goal: create `docs/knowledge/` as Markdown knowledge for agents and humans.
- Implementation: added architecture, service, business, data, integration, runbook, standard, ADR, and index files with YAML frontmatter and internal links; marked missing or inferred information; added knowledge-first workflow to `AGENTS.md`.
- Verification: API build passed; checked `docs/knowledge/` file list and frontmatter markers.
- Next idea: add documentation linting for frontmatter and broken links once stable.

## 001_012: Prune Open Knowledge Format Noise

- Goal: remove unnecessary knowledge artifacts after adopting `docs/knowledge/`.
- Implementation: removed one-off migration prompt and placeholder-only deployment/rollback runbooks; updated knowledge index and maintenance standards; preserved `.codex` workflow assets.
- Verification: API build passed; scanned docs, AGENTS, phase files, and task memories for removed paths.
- Next idea: add a lightweight docs link checker.

## 001_013: Prune Superseded Codex Context

- Goal: remove `.codex` files made redundant by `docs/knowledge/`.
- Implementation: removed `.codex/context/*` and `.codex/memories/.gitkeep`; updated AGENTS, config, prompts, agents, skills, historical `001_004` note, and knowledge maintenance standard.
- Verification: API build passed; scanned `.codex`, AGENTS, and docs for live references to removed context files.
- Next idea: add a docs checklist for future `.codex` additions.

## 001_014: Prune Repo-Local Codex Agents

- Goal: keep only the seven primary repo-local Codex agents requested by the user.
- Implementation: removed optional API, CLI, MCP, Jira/Notion, memory, planning, OpenAI, and security agent role files; updated `AGENTS.md`, `.codex/agents/README.md`, `backend-agent.md`, and `.codex/prompts/implement-task.md`.
- Verification: checked retained agent files and searched routing docs for removed filenames.
- Next idea: add focused guidance to repo-local skills first if deeper real integrations are needed later.

## 001_015: Compact Phase Task Memories

- Goal: reduce `.codex/memories/tasks/` noise while preserving Phase 001 task history and task IDs.
- Implementation: compacted individual `001_001` through `001_014` memory files into this phase-level memory file; added `001_015` to the phase; updated AGENTS, phase guidance, implement-task prompt, and memory template to allow compact phase memories.
- Verification: inspect `.codex/memories/tasks/` and search this file for retained task IDs.
- Next idea: keep active tasks as focused notes when useful, then fold completed historical tasks into compact phase memories.

## 001_016: Add API Swagger And Scalar

- Goal: expose discoverable API documentation for `AgentWorkflow.Api`.
- Implementation: added `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore`; registered `AddOpenApi()` in API services; added `MapAgentWorkflowApiDocumentation()` to map Swagger/OpenAPI JSON at `/swagger/v1/swagger.json` and Scalar UI at `/scalar/v1`; kept `Program.cs` thin; updated README and knowledge runbooks.
- Verification: build `src/AgentWorkflow.Api/AgentWorkflow.Api.csproj` and inspect docs URLs after running the API.
- Next idea: add endpoint summaries/descriptions when the API contract grows.

## 001_017: Realign Backlog To GitHub PR Workflow

- Goal: make `BACKLOG.md` follow the user's target product flow: Jira/Notion work item, GitHub connection, clone, branch, code change, push, and draft PR.
- Implementation: rewrote the backlog around GitHub repository workspace, work item intake, approval and branch execution, push and draft PR, real code agent, repo intelligence, memory, and advanced orchestration.
- Verification: reviewed `BACKLOG.md` for phase ordering and kept memory/Qdrant/Neo4j after the GitHub-to-PR vertical slice.
- Next idea: convert the new Phase 2 backlog into detailed `.codex/phases` task files when implementation starts.
