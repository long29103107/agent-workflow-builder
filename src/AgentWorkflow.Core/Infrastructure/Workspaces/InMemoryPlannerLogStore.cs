using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryPlannerLogStore : IPlannerLogStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, List<PlannerLog>> _logs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<TaskItem>> _tasks = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<PlannerLog>> GetPlannerLogsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<PlannerLog>>(
                _logs.TryGetValue(workspaceId, out var logs)
                    ? logs.OrderByDescending(log => log.CreatedAt).ToList()
                    : []);
        }
    }

    public Task<PlannerLog?> GetPlannerLogAsync(
        string workspaceId,
        string plannerLogId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var log = _logs.TryGetValue(workspaceId, out var logs)
                ? logs.FirstOrDefault(item => string.Equals(item.Id, plannerLogId, StringComparison.OrdinalIgnoreCase))
                : null;
            return Task.FromResult(log);
        }
    }

    public Task<PlannerLog> CreatePendingPlannerLogAsync(
        string workspaceId,
        WorkspaceUserRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var log = new PlannerLog(
            Guid.NewGuid().ToString("N"),
            workspaceId,
            request.Id,
            request.Content,
            PlannerLogStatus.PendingApproval,
            CreatePlannerSteps(request.Content),
            now,
            now);

        lock (_sync)
        {
            if (!_logs.TryGetValue(workspaceId, out var logs))
            {
                logs = [];
                _logs[workspaceId] = logs;
            }

            logs.Add(log);
        }

        return Task.FromResult(log);
    }

    public Task<PlannerApprovalResult?> ApprovePlannerLogAsync(
        string workspaceId,
        string plannerLogId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_logs.TryGetValue(workspaceId, out var logs))
            {
                return Task.FromResult<PlannerApprovalResult?>(null);
            }

            var index = logs.FindIndex(log =>
                string.Equals(log.Id, plannerLogId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Task.FromResult<PlannerApprovalResult?>(null);
            }

            var current = logs[index];
            if (!_tasks.TryGetValue(workspaceId, out var tasks))
            {
                tasks = [];
                _tasks[workspaceId] = tasks;
            }

            var generated = tasks
                .Where(task => task.Id.StartsWith($"planner-{current.Id}-", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (current.Status == PlannerLogStatus.PendingApproval)
            {
                current = current with
                {
                    Status = PlannerLogStatus.Approved,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                logs[index] = current;
                generated = CreateTasks(current).ToList();
                tasks.InsertRange(0, generated);
            }

            return Task.FromResult<PlannerApprovalResult?>(new PlannerApprovalResult(current, generated));
        }
    }

    public Task<IReadOnlyList<TaskItem>> GetGeneratedTasksAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.TryGetValue(workspaceId, out var tasks) ? tasks.ToList() : []);
        }
    }

    public Task<TaskItem?> GetGeneratedTaskAsync(
        string workspaceId,
        string taskId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var task = _tasks.TryGetValue(workspaceId, out var tasks)
                ? tasks.FirstOrDefault(item =>
                    string.Equals(item.Id, taskId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Key, taskId, StringComparison.OrdinalIgnoreCase))
                : null;
            return Task.FromResult(task);
        }
    }

    private static IReadOnlyList<PlannerStep> CreatePlannerSteps(string request) =>
    [
        new("Capture request", request, "Request intake"),
        new("Ground in work item", "Create workspace-scoped work items for execution.", "Agent planner"),
        new("Plan execution", "Break the request into repository investigation, implementation, verification, and review slices.", "Lead agent"),
        new("Prepare processing", "Queue approved tasks and process the next workspace priority item.", "Scheduler")
    ];

    private static IEnumerable<TaskItem> CreateTasks(PlannerLog log) =>
        log.Steps.Select((step, index) => new TaskItem(
            $"planner-{log.Id}-{index + 1}",
            "agent-planner",
            $"PLAN-{index + 1}",
            step.Title,
            step.Detail,
            "Backlog",
            index == 0 ? "High" : "Medium",
            [step.Owner, "planner"]));
}
