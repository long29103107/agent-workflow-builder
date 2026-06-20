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
    Task<ScheduledTask?> ProcessNextAsync(CancellationToken cancellationToken);
    Task<ScheduledTask?> ProcessNextAsync(string workspaceId, CancellationToken cancellationToken);
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
    Task<PlannerApprovalResult?> ApprovePlannerLogAsync(string workspaceId, string plannerLogId, CancellationToken cancellationToken);
    Task<IReadOnlyList<TaskItem>> GetGeneratedTasksAsync(string workspaceId, CancellationToken cancellationToken);
    Task<TaskItem?> GetGeneratedTaskAsync(string workspaceId, string taskId, CancellationToken cancellationToken);
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
    Task<InvestigationResult> InvestigateAsync(InvestigationRequest request, Action<string, string> emitEvent, CancellationToken cancellationToken);
}

public interface IWorkflowEngine
{
    Task<WorkflowRun> StartInvestigationAsync(InvestigationRequest request, CancellationToken cancellationToken);
}

public interface IWorkflowRunStore
{
    WorkflowRun CreateRun(string taskId);
    WorkflowRun? GetRun(Guid runId);
    IReadOnlyList<WorkflowEvent> GetEvents(Guid runId);
    void AddEvent(Guid runId, string agent, string type, string message);
    WorkflowRun CompleteRun(Guid runId, InvestigationResult result);
    WorkflowRun FailRun(Guid runId, string reason);
}

public interface ISettingsStore
{
    ToolEndpointSettings GetSettings();
    ToolEndpointSettings UpdateSettings(ToolEndpointSettings settings);
}
