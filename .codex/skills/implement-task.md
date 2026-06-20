# Implement Task Skill

Use this repo-local skill when the user asks to implement, wire, fix, or deliver a task in Agent Workflow Builder.

## Workflow

1. Understand the requested task and affected surfaces.
2. Read `docs/knowledge/index.md`, related knowledge files, `AGENTS.md`, and relevant source files before editing.
3. Query CodeGraph for related source code context when `.codegraph/` is initialized; fall back to targeted `rg` and file reads when unavailable.
4. Use `src/AgentWorkflow.Core` as the source of truth for domain models, interfaces, orchestration, agents, memory, MCP abstractions, and OpenAI SDK reasoning.
5. Keep entrypoints thin:
   - `src/AgentWorkflow.Api`
   - `src/AgentWorkflow.Cli`
   - `src/AgentWorkflow.Mcp`
   - `src/agent-workflow-ui`
6. Implement mock-first behavior unless the user explicitly asks for a real provider.
7. Update docs, knowledge files, and repo-local agent assets when behavior, commands, or conventions change.
8. Verify with the smallest useful command set.
9. Summarize changed files, verification, and any follow-up risks.

## Surface Rules

- Core change: update contracts/models first, then infrastructure and adapters.
- API change: keep `Program.cs` focused on endpoint mapping and DI.
- CLI change: keep output stable and machine-readable.
- MCP change: keep stdio JSON contract explicit and compatible with future MCP SDK replacement.
- UI change: use Bun commands and keep the investigation console as the first screen.
- Security-sensitive change: use `.codex/skills/security-threat-model` or `.codex/skills/security-best-practices` when explicitly requested.

## Done Definition

- Code compiles for affected .NET projects.
- UI builds with `bun run build` when frontend changed.
- CLI smoke test passes when Core orchestration changed.
- README/AGENTS and related `docs/knowledge` files stay aligned with actual runtime behavior.
- No unrelated generated files are modified intentionally.
