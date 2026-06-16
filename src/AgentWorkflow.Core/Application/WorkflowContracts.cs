using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Application;

public interface ITaskSource
{
    Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken);
    Task<TaskItem?> GetTaskAsync(string taskId, CancellationToken cancellationToken);
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
