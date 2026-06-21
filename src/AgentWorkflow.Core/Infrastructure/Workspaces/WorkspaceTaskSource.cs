using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class WorkspaceTaskSource(
    ITaskSource taskSource,
    IPlannerLogStore plannerLogStore,
    ITaskAssignmentStore assignmentStore) : IWorkspaceTaskSource
{
    public async Task<IReadOnlyList<TaskItem>> GetTasksAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var generated = await plannerLogStore.GetGeneratedTasksAsync(workspaceId, cancellationToken);
        var sourceTasks = await taskSource.GetTasksAsync(cancellationToken);
        var assignments = await assignmentStore.GetAssignmentsAsync(workspaceId, cancellationToken);
        return [
            .. generated.Select(task => ApplyAssignment(task, assignments)),
            .. sourceTasks.Select(task => ApplyAssignment(task, assignments))
        ];
    }

    public async Task<TaskItem?> GetTaskAsync(
        string workspaceId,
        string taskId,
        CancellationToken cancellationToken)
    {
        var task = await plannerLogStore.GetGeneratedTaskAsync(workspaceId, taskId, cancellationToken)
            ?? await taskSource.GetTaskAsync(taskId, cancellationToken);
        if (task is null)
        {
            return null;
        }

        var assignments = await assignmentStore.GetAssignmentsAsync(workspaceId, cancellationToken);
        return ApplyAssignment(task, assignments);
    }

    private static TaskItem ApplyAssignment(
        TaskItem task,
        IReadOnlyDictionary<string, string> assignments) =>
        task with
        {
            AssignedAgent = assignments.GetValueOrDefault(task.Id) ?? task.AssignedAgent
        };
}
