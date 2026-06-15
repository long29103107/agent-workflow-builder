namespace AgentWorkflow.Mcp.Contracts;

public sealed record McpInvestigationRequest(
    string Method,
    string TaskId,
    string? RepositoryPath,
    IReadOnlyList<string>? RequestedAgents);
