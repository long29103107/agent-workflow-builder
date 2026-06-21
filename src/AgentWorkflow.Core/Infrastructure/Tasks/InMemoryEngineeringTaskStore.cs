using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryEngineeringTaskStore : IEngineeringTaskStore, IWorkItemStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, EngineeringTask> _tasks = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, WorkItem> _workItems = new(StringComparer.OrdinalIgnoreCase);

    public InMemoryEngineeringTaskStore()
    {
        foreach (var seed in EngineeringTaskDefaults.Create())
        {
            CreateTask(seed.TaskId, seed.Request, seed.LegacyStatus);
        }
    }

    public Task<IReadOnlyList<EngineeringTask>> GetTasksAsync(
        string? projectId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var tasks = _tasks.Values
                .Where(task => string.IsNullOrWhiteSpace(projectId) ||
                    string.Equals(task.ProjectId, projectId, StringComparison.OrdinalIgnoreCase))
                .OrderBy(task => task.CreatedAt)
                .ToList();
            return Task.FromResult<IReadOnlyList<EngineeringTask>>(tasks);
        }
    }

    public Task<EngineeringTask?> GetTaskAsync(
        string taskId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_tasks.GetValueOrDefault(taskId));
        }
    }

    public Task<EngineeringTask> CreateTaskAsync(
        CreateEngineeringTaskRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request);

        lock (_sync)
        {
            return Task.FromResult(CreateTask(
                Guid.NewGuid().ToString("N"),
                request,
                legacyStatus: "Backlog"));
        }
    }

    public Task<EngineeringTask?> UpdateStatusAsync(
        string taskId,
        EngineeringTaskStatus status,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_tasks.TryGetValue(taskId, out var task))
            {
                return Task.FromResult<EngineeringTask?>(null);
            }

            var now = DateTimeOffset.UtcNow;
            var updated = task with
            {
                Status = status,
                UpdatedAt = now,
                CompletedAt = status is EngineeringTaskStatus.Completed or EngineeringTaskStatus.Failed
                    ? now
                    : null
            };
            _tasks[taskId] = updated;
            return Task.FromResult<EngineeringTask?>(updated);
        }
    }

    public Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(
        string engineeringTaskId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var workItems = _workItems.Values
                .Where(item => string.Equals(
                    item.EngineeringTaskId,
                    engineeringTaskId,
                    StringComparison.OrdinalIgnoreCase))
                .OrderBy(item => item.Id)
                .ToList();
            return Task.FromResult<IReadOnlyList<WorkItem>>(workItems);
        }
    }

    public Task<WorkItem?> GetWorkItemAsync(
        string workItemId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_workItems.GetValueOrDefault(workItemId));
        }
    }

    public Task<WorkItem> AddWorkItemAsync(
        string engineeringTaskId,
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Validate(request);

        lock (_sync)
        {
            if (!_tasks.TryGetValue(engineeringTaskId, out var task))
            {
                throw new KeyNotFoundException($"Engineering task '{engineeringTaskId}' was not found.");
            }

            var workItem = CreateWorkItem(engineeringTaskId, request);
            _workItems[workItem.Id] = workItem;
            _tasks[engineeringTaskId] = task with
            {
                WorkItemIds = [.. task.WorkItemIds, workItem.Id],
                UpdatedAt = DateTimeOffset.UtcNow
            };
            return Task.FromResult(workItem);
        }
    }

    private EngineeringTask CreateTask(
        string taskId,
        CreateEngineeringTaskRequest request,
        string legacyStatus)
    {
        Validate(request);
        var now = DateTimeOffset.UtcNow;
        var workItems = request.WorkItems
            .Select(item => CreateWorkItem(taskId, item with
            {
                Status = string.IsNullOrWhiteSpace(item.Status) ? legacyStatus : item.Status
            }))
            .ToList();
        var task = new EngineeringTask(
            taskId,
            request.ProjectId.Trim(),
            request.Title.Trim(),
            request.Description.Trim(),
            EngineeringTaskStatus.New,
            request.Priority,
            workItems.Select(item => item.Id).ToList(),
            now,
            now,
            CompletedAt: null);

        _tasks[task.Id] = task;
        foreach (var workItem in workItems)
        {
            _workItems[workItem.Id] = workItem;
        }

        return task;
    }

    private static WorkItem CreateWorkItem(
        string engineeringTaskId,
        CreateWorkItemRequest request) =>
        new(
            Guid.NewGuid().ToString("N"),
            engineeringTaskId,
            request.Source,
            request.SourceKey.Trim(),
            request.Title.Trim(),
            request.Description.Trim(),
            request.Status.Trim(),
            request.Priority.Trim(),
            request.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());

    private static void Validate(CreateEngineeringTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectId))
        {
            throw new ArgumentException("Project ID is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Task title is required.", nameof(request));
        }

        if (request.WorkItems is null)
        {
            throw new ArgumentException("Work items are required.", nameof(request));
        }

        foreach (var workItem in request.WorkItems)
        {
            Validate(workItem);
        }
    }

    private static void Validate(CreateWorkItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceKey))
        {
            throw new ArgumentException("Work item source key is required.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Work item title is required.", nameof(request));
        }
    }

}
