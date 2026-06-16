namespace AgentWorkflow.Mcp.Contracts;

public sealed record McpInvestigationRequest(
    string Method,
    string TaskId,
    string? RepositoryPath,
    string? RepositoryUrl,
    IReadOnlyList<string>? RequestedAgents);
