namespace AgentWorkflow.Core.Domain;

public sealed record TaskItem(
    string Id,
    string Source,
    string Key,
    string Title,
    string Description,
    string Status,
    string Priority,
    IReadOnlyList<string> Tags);

public sealed record WorkflowRun(
    Guid Id,
    string TaskId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    InvestigationResult? Result);

public sealed record WorkflowEvent(
    Guid Id,
    Guid RunId,
    DateTimeOffset Timestamp,
    string Agent,
    string Type,
    string Message);

public sealed record AgentMessage(
    string AgentName,
    string Role,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record InvestigationResult(
    string Summary,
    ExecutionPlan Plan,
    IReadOnlyList<AgentMessage> AgentMessages,
    RepositoryContext RepositoryContext,
    IReadOnlyList<MemoryItem> MemoryItems,
    IReadOnlyList<GraphEntity> GraphEntities);

public sealed record ExecutionPlan(
    string Title,
    IReadOnlyList<ExecutionStep> Steps,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> OpenQuestions);

public sealed record ExecutionStep(
    int Order,
    string Title,
    string Description,
    string OwnerAgent,
    string Status);

public sealed record MemoryItem(
    string Id,
    string Title,
    string Content,
    IReadOnlyList<string> Tags,
    DateTimeOffset CreatedAt);

public sealed record GraphEntity(
    string Id,
    string Type,
    string Name,
    IReadOnlyDictionary<string, string> Properties,
    IReadOnlyList<string> RelatedEntityIds);

public sealed record RepositoryContext(
    string Path,
    string Name,
    RepositoryConnection Connection,
    IReadOnlyList<string> ImportantFiles,
    IReadOnlyList<string> Technologies,
    string Summary);

public sealed record RepositoryConnection(
    string Provider,
    string? Url,
    string? LocalPath,
    string Owner,
    string Name,
    string DefaultBranch,
    string Status,
    string Summary);

public sealed record InvestigationRequest(
    string TaskId,
    string? RepositoryPath,
    string? RepositoryUrl,
    IReadOnlyList<string>? RequestedAgents);

public sealed record ToolEndpointSettings(
    string JiraMcpEndpoint,
    string NotionMcpEndpoint,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider);

public sealed record AgentWorkContext(
    TaskItem Task,
    string NotionContext,
    RepositoryContext Repository,
    IReadOnlyList<MemoryItem> Memories,
    IReadOnlyList<GraphEntity> GraphEntities);

public sealed record SubagentResult(
    string AgentName,
    string Summary,
    IReadOnlyList<ExecutionStep> SuggestedSteps,
    IReadOnlyList<string> Risks,
    IReadOnlyList<string> OpenQuestions);

public sealed record AgentReasoningRequest(
    string TaskKey,
    string TaskTitle,
    string RepositoryName,
    IReadOnlyList<string> AgentSummaries);

public sealed record AgentReasoningResult(
    string Summary,
    IReadOnlyList<string> SuggestedRisks,
    IReadOnlyList<string> OpenQuestions);
