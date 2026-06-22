using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryTaskScheduler(
    ITaskSource taskSource,
    IWorkspaceTaskSource workspaceTaskSource,
    IWorkflowEngine workflowEngine,
    TimeProvider timeProvider) : ITaskScheduler
{
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(5);
    private readonly Lock _sync = new();
    private readonly Dictionary<Guid, ScheduledEntry> _entries = [];
    private readonly SemaphoreSlim _workAvailable = new(0);
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

            var assignedAgent = request.AssignedAgent ?? task.AssignedAgent;
            var investigationRequest = new InvestigationRequest(
                task.Id,
                request.RepositoryPath,
                request.RepositoryUrl,
                request.RequestedAgents ??
                    (string.IsNullOrWhiteSpace(assignedAgent) ? [] : [assignedAgent]),
                request.WorkspaceId);
            var run = workflowEngine.QueueInvestigation(investigationRequest);

            var item = new ScheduledTask(
                Guid.NewGuid(),
                task.Id,
                task.Title,
                request.Priority ?? ParsePriority(task.Priority),
                ScheduledTaskStatus.Queued,
                timeProvider.GetUtcNow(),
                null,
                null,
                run.Id,
                null,
                request.WorkspaceId,
                assignedAgent,
                RequestedAgents: request.RequestedAgents);

            _entries[item.Id] = new ScheduledEntry(
                item,
                investigationRequest,
                _nextSequence++);

            _workAvailable.Release();

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

    public Task WaitForWorkAsync(CancellationToken cancellationToken) =>
        _workAvailable.WaitAsync(cancellationToken);

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
            RecoverExpiredLeases();
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
            RecoverExpiredLeases();
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
        using var heartbeatCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeatTask = HeartbeatAsync(claimed, heartbeatCancellation.Token);

        try
        {
            var run = await workflowEngine.ExecuteInvestigationAsync(
                claimed.Item.WorkflowRunId
                    ?? throw new InvalidOperationException("Scheduled task has no workflow run."),
                claimed.InvestigationRequest,
                cancellationToken);

            lock (_sync)
            {
                claimed.Item = claimed.Item with
                {
                    Status = string.Equals(run.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        ? ScheduledTaskStatus.Completed
                        : ScheduledTaskStatus.Failed,
                    CompletedAt = timeProvider.GetUtcNow(),
                    WorkflowRunId = run.Id,
                    Error = string.Equals(run.Status, "Completed", StringComparison.OrdinalIgnoreCase)
                        ? null
                        : $"Workflow run ended with status '{run.Status}'.",
                    LeaseExpiresAt = null
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
                    Error = null,
                    LastHeartbeatAt = null,
                    LeaseExpiresAt = null
                };

                _workAvailable.Release();
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
                    CompletedAt = timeProvider.GetUtcNow(),
                    Error = ex.Message,
                    LeaseExpiresAt = null
                };

                return claimed.Item;
            }
        }
        finally
        {
            await heartbeatCancellation.CancelAsync();
            await heartbeatTask;
        }
    }

    private void Claim(ScheduledEntry entry)
    {
        var now = timeProvider.GetUtcNow();
        entry.Item = entry.Item with
        {
            Status = ScheduledTaskStatus.Processing,
            StartedAt = now,
            Error = null,
            LastHeartbeatAt = now,
            LeaseExpiresAt = now.Add(LeaseDuration)
        };
    }

    private async Task HeartbeatAsync(ScheduledEntry entry, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(HeartbeatInterval, timeProvider);

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                lock (_sync)
                {
                    if (entry.Item.Status != ScheduledTaskStatus.Processing)
                    {
                        return;
                    }

                    var now = timeProvider.GetUtcNow();
                    entry.Item = entry.Item with
                    {
                        LastHeartbeatAt = now,
                        LeaseExpiresAt = now.Add(LeaseDuration)
                    };
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void RecoverExpiredLeases()
    {
        var now = timeProvider.GetUtcNow();
        foreach (var entry in _entries.Values.Where(entry =>
                     entry.Item.Status == ScheduledTaskStatus.Processing &&
                     entry.Item.LeaseExpiresAt <= now))
        {
            entry.Item = entry.Item with
            {
                Status = ScheduledTaskStatus.Queued,
                StartedAt = null,
                LastHeartbeatAt = null,
                LeaseExpiresAt = null,
                Error = "The previous worker lease expired; the task was requeued."
            };
            _workAvailable.Release();
        }
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
        InvestigationRequest investigationRequest,
        long sequence)
    {
        public ScheduledTask Item { get; set; } = item;
        public InvestigationRequest InvestigationRequest { get; } = investigationRequest;
        public long Sequence { get; } = sequence;
    }
}
