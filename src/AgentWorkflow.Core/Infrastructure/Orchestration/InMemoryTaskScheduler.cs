using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryTaskScheduler(
    ITaskSource taskSource,
    IWorkspaceTaskSource workspaceTaskSource,
    IWorkflowEngine workflowEngine) : ITaskScheduler
{
    private readonly Lock _sync = new();
    private readonly Dictionary<Guid, ScheduledEntry> _entries = [];
    private long _nextSequence;

    public async Task<ScheduledTask> EnqueueAsync(
        ScheduleTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.TaskId))
        {
            throw new ArgumentException("TaskId is required.", nameof(request));
        }

        var task = request.WorkspaceId is null
            ? await taskSource.GetTaskAsync(request.TaskId, cancellationToken)
            : await workspaceTaskSource.GetTaskAsync(request.WorkspaceId, request.TaskId, cancellationToken);
        if (task is null)
        {
            throw new ArgumentException($"Task '{request.TaskId}' was not found.", nameof(request));
        }

        lock (_sync)
        {
            var hasActiveDuplicate = _entries.Values.Any(entry =>
                string.Equals(entry.Item.TaskId, task.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(entry.Item.WorkspaceId, request.WorkspaceId, StringComparison.OrdinalIgnoreCase) &&
                entry.Item.Status is ScheduledTaskStatus.Queued or ScheduledTaskStatus.Processing);

            if (hasActiveDuplicate)
            {
                throw new InvalidOperationException($"Task '{task.Id}' is already queued or processing.");
            }

            var item = new ScheduledTask(
                Guid.NewGuid(),
                task.Id,
                task.Title,
                request.Priority ?? ParsePriority(task.Priority),
                ScheduledTaskStatus.Queued,
                DateTimeOffset.UtcNow,
                null,
                null,
                null,
                null,
                request.WorkspaceId,
                request.AssignedAgent ?? task.AssignedAgent);

            _entries[item.Id] = new ScheduledEntry(
                item,
                request.RepositoryPath,
                request.RepositoryUrl,
                _nextSequence++);

            return item;
        }
    }

    public IReadOnlyList<ScheduledTask> GetScheduledTasks()
    {
        return GetScheduledTasksCore(workspaceId: null, filterByWorkspace: false);
    }

    public IReadOnlyList<ScheduledTask> GetScheduledTasks(string workspaceId)
    {
        return GetScheduledTasksCore(workspaceId, filterByWorkspace: true);
    }

    private IReadOnlyList<ScheduledTask> GetScheduledTasksCore(string? workspaceId, bool filterByWorkspace)
    {
        lock (_sync)
        {
            return _entries.Values
                .Where(entry => !filterByWorkspace ||
                    string.Equals(entry.Item.WorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => StatusOrder(entry.Item.Status))
                .ThenByDescending(entry => entry.Item.Priority)
                .ThenBy(entry => entry.Sequence)
                .Select(entry => entry.Item)
                .ToList();
        }
    }

    public ScheduledTask? GetScheduledTask(Guid scheduledTaskId)
    {
        lock (_sync)
        {
            return _entries.TryGetValue(scheduledTaskId, out var entry)
                ? entry.Item
                : null;
        }
    }

    public async Task<ScheduledTask?> ProcessNextAsync(CancellationToken cancellationToken)
    {
        return await ProcessNextCoreAsync(workspaceId: null, filterByWorkspace: false, cancellationToken);
    }

    public async Task<ScheduledTask?> ProcessNextAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        return await ProcessNextCoreAsync(workspaceId, filterByWorkspace: true, cancellationToken);
    }

    public async Task<ScheduledTask?> ProcessAsync(
        Guid scheduledTaskId,
        string workspaceId,
        CancellationToken cancellationToken)
    {
        ScheduledEntry? claimed;

        lock (_sync)
        {
            claimed = _entries.GetValueOrDefault(scheduledTaskId);
            if (claimed is null ||
                claimed.Item.Status != ScheduledTaskStatus.Queued ||
                !string.Equals(claimed.Item.WorkspaceId, workspaceId, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            Claim(claimed);
        }

        return await ProcessClaimedAsync(claimed, cancellationToken);
    }

    private async Task<ScheduledTask?> ProcessNextCoreAsync(
        string? workspaceId,
        bool filterByWorkspace,
        CancellationToken cancellationToken)
    {
        ScheduledEntry? claimed;

        lock (_sync)
        {
            claimed = _entries.Values
                .Where(entry => entry.Item.Status == ScheduledTaskStatus.Queued &&
                    (!filterByWorkspace || string.Equals(
                        entry.Item.WorkspaceId,
                        workspaceId,
                        StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(entry => entry.Item.Priority)
                .ThenBy(entry => entry.Sequence)
                .FirstOrDefault();

            if (claimed is null)
            {
                return null;
            }

            Claim(claimed);
        }

        return await ProcessClaimedAsync(claimed, cancellationToken);
    }

    private async Task<ScheduledTask> ProcessClaimedAsync(
        ScheduledEntry claimed,
        CancellationToken cancellationToken)
    {
        try
        {
            var run = await workflowEngine.StartInvestigationAsync(
                new InvestigationRequest(
                    claimed.Item.TaskId,
                    claimed.RepositoryPath,
                    claimed.RepositoryUrl,
                    RequestedAgents: string.IsNullOrWhiteSpace(claimed.Item.AssignedAgent)
                        ? []
                        : [claimed.Item.AssignedAgent],
                    WorkspaceId: claimed.Item.WorkspaceId),
                cancellationToken);

            lock (_sync)
            {
                claimed.Item = claimed.Item with
                {
                    Status = string.Equals(run.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        ? ScheduledTaskStatus.Completed
                        : ScheduledTaskStatus.Failed,
                    CompletedAt = DateTimeOffset.UtcNow,
                    WorkflowRunId = run.Id,
                    Error = string.Equals(run.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : $"Workflow run ended with status '{run.Status}'."
                };

                return claimed.Item;
            }
        }
        catch (OperationCanceledException)
        {
            lock (_sync)
            {
                claimed.Item = claimed.Item with
                {
                    Status = ScheduledTaskStatus.Queued,
                    StartedAt = null,
                    Error = null
                };
            }

            throw;
        }
        catch (Exception ex)
        {
            lock (_sync)
            {
                claimed.Item = claimed.Item with
                {
                    Status = ScheduledTaskStatus.Failed,
                    CompletedAt = DateTimeOffset.UtcNow,
                    Error = ex.Message
                };

                return claimed.Item;
            }
        }
    }

    private static void Claim(ScheduledEntry entry)
    {
        entry.Item = entry.Item with
        {
            Status = ScheduledTaskStatus.Processing,
            StartedAt = DateTimeOffset.UtcNow,
            Error = null
        };
    }

    private static ScheduledTaskPriority ParsePriority(string priority) =>
        Enum.TryParse<ScheduledTaskPriority>(priority, ignoreCase: true, out var parsed)
            ? parsed
            : ScheduledTaskPriority.Medium;

    private static int StatusOrder(ScheduledTaskStatus status) => status switch
    {
        ScheduledTaskStatus.Processing => 0,
        ScheduledTaskStatus.Queued => 1,
        ScheduledTaskStatus.Failed => 2,
        ScheduledTaskStatus.Completed => 3,
        _ => 4
    };

    private sealed class ScheduledEntry(
        ScheduledTask item,
        string? repositoryPath,
        string? repositoryUrl,
        long sequence)
    {
        public ScheduledTask Item { get; set; } = item;
        public string? RepositoryPath { get; } = repositoryPath;
        public string? RepositoryUrl { get; } = repositoryUrl;
        public long Sequence { get; } = sequence;
    }
}
