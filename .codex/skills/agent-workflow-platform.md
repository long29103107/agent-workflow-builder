# Agent Workflow Platform Skill

Use this repo-local skill when changing orchestration, agents, memory, MCP tools, repository context, or the investigation UI.

## Rules

- Keep `src/AgentWorkflow.Core` as the source of truth.
- Keep the Lead Agent as the single orchestrator.
- Keep subagents stateless and replaceable behind `ISubagent`.
- Use mock implementations first, with TODOs where real providers belong.
- Add or update interfaces in `src/AgentWorkflow.Core/Application/WorkflowContracts.cs` before infrastructure implementations.
- Keep domain records in `src/AgentWorkflow.Core/Domain/WorkflowModels.cs`.
- Keep API, CLI, MCP, and React client contracts in sync.
- Keep OpenAI SDK usage behind `IAgentReasoningService`.

## Extension Points

- Jira: replace `MockJiraMcpTool` behind `IJiraMcpTool`.
- Notion: replace `MockNotionContextTool` behind `INotionContextTool`.
- Repository intelligence: replace `LocalRepositoryReader` behind `IRepositoryReader`.
- Qdrant and Neo4j: replace `MockMemoryService` behind `IMemoryService`.
- OpenAI reasoning: extend `OpenAiAgentReasoningService` behind `IAgentReasoningService`.
- Run persistence: replace `InMemoryWorkflowRunStore` behind `IWorkflowRunStore`.
