using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class MockJiraMcpTool(EngineeringTaskSource taskSource) : IJiraMcpTool
{
    public string EndpointName => "mock://jira";

    public Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken) =>
        taskSource.GetTasksAsync(cancellationToken);

    public Task<TaskItem?> GetTaskAsync(
        string taskId,
        CancellationToken cancellationToken) =>
        taskSource.GetTaskAsync(taskId, cancellationToken);
}
