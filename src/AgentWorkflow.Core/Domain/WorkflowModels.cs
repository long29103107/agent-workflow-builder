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

public enum ScheduledTaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum ScheduledTaskStatus
{
    Queued,
    Processing,
    Completed,
    Failed
}

public sealed record ScheduleTaskRequest(
    string TaskId,
    ScheduledTaskPriority? Priority,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? WorkspaceId = null);

public sealed record ScheduledTask(
    Guid Id,
    string TaskId,
    string TaskTitle,
    ScheduledTaskPriority Priority,
    ScheduledTaskStatus Status,
    DateTimeOffset QueuedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    Guid? WorkflowRunId,
    string? Error,
    string? WorkspaceId = null);

public sealed record WorkspaceProject(
    string Id,
    string Name,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record Project(
    string Id,
    string Name,
    ProjectRepositorySettings Repository,
    ProjectGitHubSettings GitHub,
    ProjectAgentSettings Agents,
    ProjectCodingStandardSettings CodingStandards,
    ProjectCommandSettings Commands,
    ProjectBranchPolicy BranchPolicy,
    ProjectProtectedPathPolicy ProtectedPaths,
    ProjectApprovalPolicy ApprovalPolicy,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record ProjectRepositorySettings(
    string Provider,
    string LocalPath,
    string Url,
    string DefaultBranch);

public sealed record ProjectGitHubSettings(
    string Owner,
    string Repository,
    long? InstallationId);

public sealed record ProjectAgentSettings(
    IReadOnlyList<string> EnabledAgentNames,
    bool RequireExplicitSelection);

public sealed record ProjectCodingStandardSettings(
    IReadOnlyList<string> InstructionFiles,
    IReadOnlyList<string> Rules);

public sealed record ProjectCommandSettings(
    IReadOnlyList<string> Setup,
    IReadOnlyList<string> Build,
    IReadOnlyList<string> Test,
    IReadOnlyList<string> Lint,
    int TimeoutSeconds);

public sealed record ProjectBranchPolicy(
    string BaseBranch,
    string BranchPrefix,
    bool AllowForcePush);

public sealed record ProjectProtectedPathPolicy(
    IReadOnlyList<string> Paths,
    bool BlockProductionEnvironmentFiles);

public sealed record ProjectApprovalPolicy(
    bool RequireInvestigationPlanApproval,
    bool RequireImplementationApproval,
    bool RequirePullRequestApproval,
    bool RequireMergeApproval);

public sealed record CreateProjectRequest(
    string Name,
    ProjectRepositorySettings Repository,
    ProjectGitHubSettings GitHub,
    ProjectAgentSettings Agents,
    ProjectCodingStandardSettings CodingStandards,
    ProjectCommandSettings Commands,
    ProjectBranchPolicy BranchPolicy,
    ProjectProtectedPathPolicy ProtectedPaths,
    ProjectApprovalPolicy ApprovalPolicy);

public sealed record UpdateProjectRequest(
    string Name,
    ProjectRepositorySettings Repository,
    ProjectGitHubSettings GitHub,
    ProjectAgentSettings Agents,
    ProjectCodingStandardSettings CodingStandards,
    ProjectCommandSettings Commands,
    ProjectBranchPolicy BranchPolicy,
    ProjectProtectedPathPolicy ProtectedPaths,
    ProjectApprovalPolicy ApprovalPolicy);

public sealed record ProjectValidationError(
    string Field,
    string Message);

public sealed record WorkspaceDefaults(
    string Name,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider);

public sealed record CreateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider);

public sealed record UpdateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider);

public sealed record WorkspaceUserRequest(
    string Id,
    string WorkspaceId,
    string Content,
    DateTimeOffset CreatedAt);

public sealed record CreateWorkspaceUserRequest(string Content);

public enum PlannerLogStatus
{
    PendingApproval,
    Approved
}

public sealed record PlannerStep(
    string Title,
    string Detail,
    string Owner);

public sealed record PlannerLog(
    string Id,
    string WorkspaceId,
    string RequestId,
    string Request,
    PlannerLogStatus Status,
    IReadOnlyList<PlannerStep> Steps,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record RequestSubmissionResult(
    WorkspaceUserRequest Request,
    PlannerLog PlannerLog);

public sealed record PlannerApprovalResult(
    PlannerLog PlannerLog,
    IReadOnlyList<TaskItem> Tasks);

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
    IReadOnlyList<string>? RequestedAgents,
    string? WorkspaceId = null);

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
