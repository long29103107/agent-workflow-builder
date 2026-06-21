using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class EngineeringTaskSource(
    IEngineeringTaskStore taskStore,
    IWorkItemStore workItemStore) : ITaskSource
{
    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken)
    {
        var tasks = await taskStore.GetTasksAsync(projectId: null, cancellationToken);
        var projections = new List<TaskItem>(tasks.Count);

        foreach (var task in tasks)
        {
            projections.Add(await ProjectAsync(task, cancellationToken));
        }

        return projections;
    }

    public async Task<TaskItem?> GetTaskAsync(
        string taskId,
        CancellationToken cancellationToken)
    {
        var task = await taskStore.GetTaskAsync(taskId, cancellationToken);
        if (task is not null)
        {
            return await ProjectAsync(task, cancellationToken);
        }

        var tasks = await taskStore.GetTasksAsync(projectId: null, cancellationToken);
        foreach (var candidate in tasks)
        {
            var workItems = await workItemStore.GetWorkItemsAsync(candidate.Id, cancellationToken);
            if (workItems.Any(item => string.Equals(
                item.SourceKey,
                taskId,
                StringComparison.OrdinalIgnoreCase)))
            {
                return await ProjectAsync(candidate, cancellationToken);
            }
        }

        return null;
    }

    private async Task<TaskItem> ProjectAsync(
        EngineeringTask task,
        CancellationToken cancellationToken)
    {
        var workItems = await workItemStore.GetWorkItemsAsync(task.Id, cancellationToken);
        var primary = workItems.FirstOrDefault();

        return new TaskItem(
            task.Id,
            primary?.Source.ToString() ?? "AgentWorkflow",
            primary?.SourceKey ?? task.Id,
            task.Title,
            task.Description,
            primary?.Status ?? task.Status.ToString(),
            primary?.Priority ?? task.Priority.ToString(),
            primary?.Tags ?? []);
    }
}
