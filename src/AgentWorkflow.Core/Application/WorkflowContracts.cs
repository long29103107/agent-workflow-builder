using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Application;

public interface ITaskSource
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetTaskAsync(string taskId, CancellationToken cancellationToken);
}

public interface ITaskScheduler
{
    Task<ScheduledTask> EnqueueAsync(ScheduleTaskRequest request, CancellationToken cancellationToken);
    IReadOnlyList<ScheduledTask> GetScheduledTasks();
    IReadOnlyList<ScheduledTask> GetScheduledTasks(string workspaceId);
    ScheduledTask? GetScheduledTask(Guid scheduledTaskId);
    Task<ScheduledTask?> ProcessAsync(Guid scheduledTaskId, string workspaceId, CancellationToken cancellationToken);
    Task<ScheduledTask?> ProcessNextAsync(CancellationToken cancellationToken);
    Task<ScheduledTask?> ProcessNextAsync(string workspaceId, CancellationToken cancellationToken);
    Task WaitForWorkAsync(CancellationToken cancellationToken);
}

public interface IEngineeringTaskStore
{
    Task<IReadOnlyList<EngineeringTask>> GetTasksAsync(
        string? projectId,
        CancellationToken cancellationToken);
    Task<EngineeringTask?> GetTaskAsync(string taskId, CancellationToken cancellationToken);
    Task<EngineeringTask> CreateTaskAsync(
        CreateEngineeringTaskRequest request,
        CancellationToken cancellationToken);
    Task<EngineeringTask?> UpdateStatusAsync(
        string taskId,
        EngineeringTaskStatus status,
        CancellationToken cancellationToken);
}

public interface IWorkItemStore
{
    Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(
        string engineeringTaskId,
        CancellationToken cancellationToken);
    Task<WorkItem?> GetWorkItemAsync(string workItemId, CancellationToken cancellationToken);
    Task<WorkItem> AddWorkItemAsync(
        string engineeringTaskId,
        CreateWorkItemRequest request,
        CancellationToken cancellationToken);
}

public interface IWorkspaceStore
{
    Task<IReadOnlyList<WorkspaceProject>> GetWorkspacesAsync(CancellationToken cancellationToken);
    Task<WorkspaceProject?> GetWorkspaceAsync(string workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceProject> CreateWorkspaceAsync(CreateWorkspaceRequest request, CancellationToken cancellationToken);
    Task<WorkspaceProject?> UpdateWorkspaceAsync(string workspaceId, UpdateWorkspaceRequest request, CancellationToken cancellationToken);
}

public interface IProjectStore
{
    Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken);
    Task<Project?> GetProjectAsync(string projectId, CancellationToken cancellationToken);
    Task<Project> CreateProjectAsync(CreateProjectRequest request, CancellationToken cancellationToken);
    Task<Project?> UpdateProjectAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteProjectAsync(string projectId, CancellationToken cancellationToken);
}

public interface IProjectPolicyValidator
{
    IReadOnlyList<ProjectValidationError> Validate(CreateProjectRequest request);
    IReadOnlyList<ProjectValidationError> Validate(UpdateProjectRequest request);
}

public interface IRequestIntakeStore
{
    Task<IReadOnlyList<WorkspaceUserRequest>> GetRequestsAsync(string workspaceId, CancellationToken cancellationToken);
    Task<WorkspaceUserRequest> CreateRequestAsync(string workspaceId, CreateWorkspaceUserRequest request, CancellationToken cancellationToken);
}

public interface IPlannerLogStore
{
    Task<IReadOnlyList<PlannerLog>> GetPlannerLogsAsync(string workspaceId, CancellationToken cancellationToken);
    Task<PlannerLog?> GetPlannerLogAsync(string workspaceId, string plannerLogId, CancellationToken cancellationToken);
    Task<PlannerLog> CreatePendingPlannerLogAsync(string workspaceId, WorkspaceUserRequest request, CancellationToken cancellationToken);
    Task<PlannerLog?> UpdatePlannerLogAsync(string workspaceId, string plannerLogId, UpdatePlannerLogRequest request, CancellationToken cancellationToken);
    Task<PlannerApprovalResult?> ApprovePlannerLogAsync(string workspaceId, string plannerLogId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> GetGeneratedTasksAsync(string workspaceId, CancellationToken cancellationToken);
    Task<TaskItem?> GetGeneratedTaskAsync(string workspaceId, string taskId, CancellationToken cancellationToken);
}

public interface ITaskAssignmentStore
{
    Task<IReadOnlyDictionary<string, string>> GetAssignmentsAsync(
        string workspaceId,
        CancellationToken cancellationToken);
    Task<string> AssignAgentAsync(
        string workspaceId,
        string taskId,
        string agentName,
        CancellationToken cancellationToken);
}

public interface IWorkspaceTaskSource
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(string workspaceId, CancellationToken cancellationToken);
    Task<TaskItem?> GetTaskAsync(string workspaceId, string taskId, CancellationToken cancellationToken);
}

public interface IWorkspaceSettingsStore
{
    Task<ToolEndpointSettings?> GetSettingsAsync(string workspaceId, CancellationToken cancellationToken);
    Task<ToolEndpointSettings?> UpdateSettingsAsync(string workspaceId, ToolEndpointSettings settings, CancellationToken cancellationToken);
}

public interface IJiraMcpTool : ITaskSource
{
    string EndpointName { get; }
}

public interface INotionContextTool
{
    string EndpointName { get; }
    Task<string> GetTaskContextAsync(TaskItem task, CancellationToken cancellationToken);
}

public interface IRepositoryReader
{
    Task<RepositoryContext> GetContextAsync(RepositoryConnection connection, CancellationToken cancellationToken);
}

public interface IRepositoryConnectionService
{
    RepositoryConnection GetConnection();
    RepositoryConnection UpdateConnection(RepositoryConnection connection);
    RepositoryConnection ResolveConnection(string? repositoryPath, string? repositoryUrl);
}

public interface IGitHubRepositoryAuthenticator
{
    RepositoryCloneTarget CreateCloneTarget(
        RepositoryConnection connection,
        string? accessToken);
}

public interface IRepositoryWorkspaceService
{
    Task<RepositoryWorkspace> CloneAsync(
        RepositoryCloneRequest request,
        CancellationToken cancellationToken);
    Task<RepositoryBranchPreparation> PrepareBranchAsync(
        RepositoryBranchPreparationRequest request,
        CancellationToken cancellationToken);
    Task<RepositoryWorkspaceFinalization> FinalizeAsync(
        RepositoryWorkspaceFinalizationRequest request,
        CancellationToken cancellationToken);
}

public interface IExecutionSandboxProvider
{
    Task<SandboxWorkspaceLease> ProvisionAsync(
        SandboxProvisionRequest request,
        CancellationToken cancellationToken);
    Task<SandboxActionResult> ApplyCodeAsync(
        SandboxCodeActionRequest request,
        CancellationToken cancellationToken);
    Task<SandboxActionResult> RunGitAsync(
        SandboxGitActionRequest request,
        CancellationToken cancellationToken);
    Task<SandboxCommandResult> ExecuteCommandAsync(
        SandboxCommandActionRequest request,
        CancellationToken cancellationToken);
    Task<SandboxArtifact> CaptureArtifactAsync(
        SandboxArtifactRequest request,
        CancellationToken cancellationToken);
    Task<SandboxWorkspaceLease> DestroyAsync(
        SandboxDestroyRequest request,
        CancellationToken cancellationToken);
    IReadOnlyList<SandboxLifecycleEvent> GetLifecycleEvents(Guid leaseId);
    IReadOnlyList<SandboxArtifact> GetArtifacts(Guid leaseId);
}

public interface IMemoryService
{
    Task<MemoryItem> StoreMemoryAsync(MemoryItem item, CancellationToken cancellationToken);
    Task<IReadOnlyList<MemoryItem>> SearchVectorMemoryAsync(string query, CancellationToken cancellationToken);
    Task<IReadOnlyList<GraphEntity>> ReadGraphRelationshipsAsync(string entityId, CancellationToken cancellationToken);
    Task LinkTaskRepositoryEntityAsync(string taskId, string repositoryName, string entityName, CancellationToken cancellationToken);
}

public interface IAgentReasoningService
{
    Task<AgentReasoningResult> SummarizeInvestigationAsync(AgentReasoningRequest request, CancellationToken cancellationToken);
}

public interface ISubagent
{
    string Name { get; }
    Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken);
}

public interface ILeadAgent
{
    Task<InvestigationResult> InvestigateAsync(
        InvestigationRequest request,
        Func<WorkflowStage, string, string, Task> advanceStage,
        CancellationToken cancellationToken);
}

public interface IWorkflowEngine
{
    WorkflowRun QueueInvestigation(InvestigationRequest request);
    Task<WorkflowRun> ExecuteInvestigationAsync(
        Guid runId,
        InvestigationRequest request,
        CancellationToken cancellationToken);
    Task<WorkflowRun> StartInvestigationAsync(InvestigationRequest request, CancellationToken cancellationToken);
}

public interface IApprovalPolicyEngine
{
    Task<ApprovalRecord> ApproveAsync(
        string projectId,
        string taskId,
        ApproveGateRequest request,
        CancellationToken cancellationToken);
    Task<ApprovalRecord?> EnsureAuthorizedAsync(
        ApprovalAuthorizationRequest request,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<ApprovalRecord>> GetApprovalsAsync(
        string projectId,
        string taskId,
        CancellationToken cancellationToken);
}

public interface IApprovalStore
{
    Task<IReadOnlyList<ApprovalRecord>> GetApprovalsAsync(
        string projectId,
        string taskId,
        CancellationToken cancellationToken);
    Task<ApprovalRecord> AddApprovalAsync(
        ApprovalRecord approval,
        CancellationToken cancellationToken);
    Task<ApprovalRecord> InvalidateApprovalAsync(
        Guid approvalId,
        string reason,
        DateTimeOffset invalidatedAt,
        CancellationToken cancellationToken);
}

public interface IWorkflowRunStore
{
    WorkflowRun CreateRun(string taskId);
    WorkflowRun? GetRun(Guid runId);
    IReadOnlyList<WorkflowEvent> GetEvents(Guid runId);
    void AddEvent(Guid runId, string agent, string type, string message);
    WorkflowRun BeginRecoveryAttempt(Guid runId);
    WorkflowRun TransitionRun(Guid runId, WorkflowStageCommand command);
    WorkflowRun CompleteRun(Guid runId, InvestigationResult result, string idempotencyKey);
    WorkflowRun FailRun(Guid runId, string reason, string idempotencyKey);
}

public interface IWorkflowEvidenceStore
{
    AgentExecution StartExecution(Guid runId, string agentName);
    AgentExecution CompleteExecution(Guid executionId, AgentExecutionStatus status);
    EvidenceItem AppendEvidence(
        Guid runId,
        Guid agentExecutionId,
        EvidenceKind kind,
        string summary,
        string? sourceReference = null,
        string? action = null,
        string? toolName = null,
        string? toolResult = null);
    Artifact AppendArtifact(
        Guid runId,
        Guid? agentExecutionId,
        string name,
        string type,
        string content,
        string contentType);
    WorkflowEvidenceBundle GetEvidence(Guid runId);
}

public interface ISecretRedactor
{
    string Redact(string value);
}

public interface ITaskActivityStore
{
    Task<TaskActivity> AppendAsync(
        string taskId,
        Guid? workflowRunId,
        Guid correlationId,
        TaskActivityCategory category,
        string type,
        string summary,
        CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskActivity>> GetAfterAsync(
        string taskId,
        long afterSequence,
        int limit,
        CancellationToken cancellationToken);
}

public interface ISettingsStore
{
    ToolEndpointSettings GetSettings();
    ToolEndpointSettings UpdateSettings(ToolEndpointSettings settings);
}
