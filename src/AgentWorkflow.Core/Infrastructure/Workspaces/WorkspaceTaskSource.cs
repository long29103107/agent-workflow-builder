using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class WorkspaceTaskSource(
    ITaskSource taskSource,
    IPlannerLogStore plannerLogStore) : IWorkspaceTaskSource
{
    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var generated = await plannerLogStore.GetGeneratedTasksAsync(workspaceId, cancellationToken);
        var sourceTasks = await taskSource.GetTasksAsync(cancellationToken);
        return [.. generated, .. sourceTasks];
    }

    public async Task<TaskItem?> GetTaskAsync(
        string workspaceId,
        string taskId,
        CancellationToken cancellationToken)
    {
        return await plannerLogStore.GetGeneratedTaskAsync(workspaceId, taskId, cancellationToken)
            ?? await taskSource.GetTaskAsync(taskId, cancellationToken);
    }
}
