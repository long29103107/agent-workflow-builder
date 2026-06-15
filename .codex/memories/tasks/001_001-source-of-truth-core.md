# 001_001: Establish Source-Of-Truth Core

## Phase

001: Skeleton And Operating System

## Task

001_001: Establish Source-Of-Truth Core

## Goal

Move orchestration source of truth into `AgentWorkflow.Core` so API, CLI, MCP, and UI can share the same workflow behavior.

## Implementation Log

- Added `src/AgentWorkflow.Core` for domain models, contracts, orchestration, mock integrations, and OpenAI SDK reasoning.
- Kept `src/AgentWorkflow.Api`, `src/AgentWorkflow.Cli`, and `src/AgentWorkflow.Mcp` as thin adapters over Core.
- Removed legacy `AgentFrameworkDemo.Console` to avoid Semantic Kernel demo drift.
- Kept OpenAI SDK usage behind `IAgentReasoningService` with deterministic fallback when `OPENAI_API_KEY` is not set.

## Verification

- API build passed.
- CLI build passed.
- MCP build passed.
- CLI smoke test returned `Status: Completed`.

## Goal Achieved

Yes. Core is the shared source of truth and adapters route through it.

## Next Idea

Use this architecture before implementing real memory, real MCP, repository intelligence, or advanced orchestration.
