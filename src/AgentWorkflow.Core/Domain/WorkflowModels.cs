namespace AgentWorkflow.Core.Domain;

public sealed record TaskItem(
    string Id,
    string Source,
    string Key,
    string Title,
    string Description,
    string Status,
    string Priority,
    IReadOnlyList<string> Tags,
    string? AssignedAgent = null);

public enum EngineeringTaskStatus
{
    New,
    Investigating,
    AwaitingPlanApproval,
    ReadyForImplementation,
    Implementing,
    Verifying,
    AwaitingImplementationApproval,
    ReadyForPullRequest,
    PullRequestOpen,
    Completed,
    Failed
}

public enum WorkItemSource
{
    Jira,
    Notion
}

public sealed record EngineeringTask(
    string Id,
    string ProjectId,
    string Title,
    string Description,
    EngineeringTaskStatus Status,
    ScheduledTaskPriority Priority,
    IReadOnlyList<string> WorkItemIds,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? CompletedAt);

public sealed record WorkItem(
    string Id,
    string EngineeringTaskId,
    WorkItemSource Source,
    string SourceKey,
    string Title,
    string Description,
    string Status,
    string Priority,
    IReadOnlyList<string> Tags);

public sealed record CreateEngineeringTaskRequest(
    string ProjectId,
    string Title,
    string Description,
    ScheduledTaskPriority Priority,
    IReadOnlyList<CreateWorkItemRequest> WorkItems);

public sealed record CreateProjectTaskRequest(
    string Title,
    string Description,
    ScheduledTaskPriority Priority,
    IReadOnlyList<CreateWorkItemRequest> WorkItems);

public sealed record UpdateEngineeringTaskStatusRequest(
    EngineeringTaskStatus Status,
    ApprovalBinding? ApprovalBinding = null);

public sealed record EngineeringTaskDetails(
    EngineeringTask Task,
    IReadOnlyList<WorkItem> WorkItems);

public sealed record CreateWorkItemRequest(
    WorkItemSource Source,
    string SourceKey,
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
    string? WorkspaceId = null,
    string? AssignedAgent = null,
    IReadOnlyList<string>? RequestedAgents = null);

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
    string? WorkspaceId = null,
    string? AssignedAgent = null,
    DateTimeOffset? LastHeartbeatAt = null,
    DateTimeOffset? LeaseExpiresAt = null,
    IReadOnlyList<string>? RequestedAgents = null);

public sealed record WorkspaceProject(
    string Id,
    string Name,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string Code = "AWB");

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
    DateTimeOffset UpdatedAt,
    string Code = "AWB");

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

public enum ApprovalGate
{
    InvestigationPlan,
    Implementation,
    PullRequest,
    Merge
}

public enum ApprovalStatus
{
    Approved,
    Invalidated
}

public sealed record ApprovalBinding(
    string? ArtifactHash,
    string? TargetBranch,
    string? CommitSha);

public sealed record ApprovalRecord(
    Guid Id,
    string ProjectId,
    string TaskId,
    Guid? WorkflowRunId,
    ApprovalGate Gate,
    ApprovalStatus Status,
    ApprovalBinding Binding,
    string ApprovedBy,
    DateTimeOffset ApprovedAt,
    DateTimeOffset? InvalidatedAt,
    string? InvalidationReason);

public sealed record ApproveGateRequest(
    ApprovalGate Gate,
    ApprovalBinding Binding,
    string ApprovedBy,
    Guid? WorkflowRunId = null);

public sealed record ApprovalAuthorizationRequest(
    string ProjectId,
    string TaskId,
    ApprovalGate Gate,
    ApprovalBinding Binding);

public sealed record CreateProjectRequest(
    string Name,
    ProjectRepositorySettings Repository,
    ProjectGitHubSettings GitHub,
    ProjectAgentSettings Agents,
    ProjectCodingStandardSettings CodingStandards,
    ProjectCommandSettings Commands,
    ProjectBranchPolicy BranchPolicy,
    ProjectProtectedPathPolicy ProtectedPaths,
    ProjectApprovalPolicy ApprovalPolicy,
    string Code = "");

public sealed record UpdateProjectRequest(
    string Name,
    ProjectRepositorySettings Repository,
    ProjectGitHubSettings GitHub,
    ProjectAgentSettings Agents,
    ProjectCodingStandardSettings CodingStandards,
    ProjectCommandSettings Commands,
    ProjectBranchPolicy BranchPolicy,
    ProjectProtectedPathPolicy ProtectedPaths,
    ProjectApprovalPolicy ApprovalPolicy,
    string Code = "");

public sealed record ProjectValidationError(
    string Field,
    string Message);

public sealed record WorkspaceDefaults(
    string Name,
    string RepositoryPath,
    string RepositoryUrl,
    string RepositoryProvider,
    string? Code = null);

public sealed record CreateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider,
    string? Code = null);

public sealed record UpdateWorkspaceRequest(
    string Name,
    string? RepositoryPath,
    string? RepositoryUrl,
    string? RepositoryProvider,
    string? Code = null);

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

public sealed record UpdatePlannerLogRequest(IReadOnlyList<PlannerStep> Steps);

public sealed record AssignTaskAgentRequest(string AgentName);

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
    IReadOnlyList<TaskItem> Tasks,
    ApprovalRecord Approval);

public enum WorkflowStage
{
    Created,
    LoadingTaskContext,
    ResolvingRepository,
    LoadingMemory,
    Investigating,
    Aggregating,
    Completed,
    Failed
}

public sealed record WorkflowRun(
    Guid Id,
    string TaskId,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    InvestigationResult? Result,
    WorkflowStage Stage = WorkflowStage.Created,
    int Attempt = 1,
    string? FailureDetails = null);

public sealed record WorkflowStageCommand(
    WorkflowStage Stage,
    string IdempotencyKey);

public enum WorkflowOperationKind
{
    TransientRead,
    Commit,
    Push,
    CreatePullRequest,
    Merge
}

public sealed record ExternalWriteCommand(
    WorkflowOperationKind Operation,
    string IdempotencyKey);

public sealed record WorkflowEvent(
    Guid Id,
    Guid RunId,
    DateTimeOffset Timestamp,
    string Agent,
    string Type,
    string Message);

public enum AgentExecutionStatus
{
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum EvidenceKind
{
    Rationale,
    SourceReference,
    Action,
    ToolResult
}

public sealed record AgentExecution(
    Guid Id,
    Guid RunId,
    string AgentName,
    AgentExecutionStatus Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt);

public sealed record EvidenceItem(
    Guid Id,
    Guid RunId,
    Guid AgentExecutionId,
    EvidenceKind Kind,
    string Summary,
    string? SourceReference,
    string? Action,
    string? ToolName,
    string? ToolResult,
    DateTimeOffset CreatedAt);

public sealed record Artifact(
    Guid Id,
    Guid RunId,
    Guid? AgentExecutionId,
    string Name,
    string Type,
    string Content,
    string ContentType,
    DateTimeOffset CreatedAt);

public sealed record WorkflowEvidenceBundle(
    IReadOnlyList<AgentExecution> AgentExecutions,
    IReadOnlyList<EvidenceItem> EvidenceItems,
    IReadOnlyList<Artifact> Artifacts);

public enum TaskActivityCategory
{
    Workflow,
    Agent,
    Approval,
    Evidence,
    Artifact
}

public sealed record TaskActivity(
    long Sequence,
    Guid Id,
    string TaskId,
    Guid? WorkflowRunId,
    Guid CorrelationId,
    TaskActivityCategory Category,
    string Type,
    string Summary,
    DateTimeOffset Timestamp);

public sealed record TaskActivityHistory(
    IReadOnlyList<TaskActivity> Items,
    long LastSequence);

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
