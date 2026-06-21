using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresEngineeringTaskStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory,
    IProjectStore projectStore) : IEngineeringTaskStore, IWorkItemStore
{
    public async Task<IReadOnlyList<EngineeringTask>> GetTasksAsync(
        string? projectId,
        CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var query = context.EngineeringTasks.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(projectId))
        {
            query = query.Where(task => task.ProjectId == projectId);
        }

        var entities = await query.OrderBy(task => task.CreatedAt).ToListAsync(cancellationToken);
        return await ToDomainsAsync(context, entities, cancellationToken);
    }

    public async Task<EngineeringTask?> GetTaskAsync(
        string taskId,
        CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.EngineeringTasks
            .AsNoTracking()
            .SingleOrDefaultAsync(task => task.Id == taskId, cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var workItemIds = await GetWorkItemIdsAsync(context, taskId, cancellationToken);
        return ToDomain(entity, workItemIds);
    }

    public async Task<EngineeringTask> CreateTaskAsync(
        CreateEngineeringTaskRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        if (await projectStore.GetProjectAsync(request.ProjectId, cancellationToken) is null)
        {
            throw new KeyNotFoundException($"Project '{request.ProjectId}' was not found.");
        }

        var taskId = Guid.NewGuid().ToString("N");
        var now = DateTimeOffset.UtcNow;
        var entity = CreateTaskEntity(taskId, request, now);
        var workItems = request.WorkItems
            .Select(item => CreateWorkItemEntity(taskId, item))
            .ToList();

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.EngineeringTasks.Add(entity);
        context.WorkItems.AddRange(workItems);
        await context.SaveChangesAsync(cancellationToken);
        return ToDomain(entity, workItems.Select(item => item.Id).ToList());
    }

    public async Task<EngineeringTask?> UpdateStatusAsync(
        string taskId,
        EngineeringTaskStatus status,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.EngineeringTasks.SingleOrDefaultAsync(
            task => task.Id == taskId,
            cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        entity.Status = status.ToString();
        entity.UpdatedAt = now;
        entity.CompletedAt = status is EngineeringTaskStatus.Completed or EngineeringTaskStatus.Failed
            ? now
            : null;
        await context.SaveChangesAsync(cancellationToken);
        var workItemIds = await GetWorkItemIdsAsync(context, taskId, cancellationToken);
        return ToDomain(entity, workItemIds);
    }

    public async Task<IReadOnlyList<WorkItem>> GetWorkItemsAsync(
        string engineeringTaskId,
        CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await context.WorkItems
            .AsNoTracking()
            .Where(item => item.EngineeringTaskId == engineeringTaskId)
            .OrderBy(item => item.Id)
            .ToListAsync(cancellationToken);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<WorkItem?> GetWorkItemAsync(
        string workItemId,
        CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.WorkItems
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == workItemId, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<WorkItem> AddWorkItemAsync(
        string engineeringTaskId,
        CreateWorkItemRequest request,
        CancellationToken cancellationToken)
    {
        Validate(request);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var task = await context.EngineeringTasks.SingleOrDefaultAsync(
            item => item.Id == engineeringTaskId,
            cancellationToken);
        if (task is null)
        {
            throw new KeyNotFoundException($"Engineering task '{engineeringTaskId}' was not found.");
        }

        var entity = CreateWorkItemEntity(engineeringTaskId, request);
        context.WorkItems.Add(entity);
        task.UpdatedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        await projectStore.GetProjectAsync(ProjectPolicyDefaults.DefaultProjectId, cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        foreach (var seed in EngineeringTaskDefaults.Create())
        {
            if (await context.EngineeringTasks.AnyAsync(
                task => task.Id == seed.TaskId,
                cancellationToken))
            {
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            context.EngineeringTasks.Add(CreateTaskEntity(seed.TaskId, seed.Request, now));
            context.WorkItems.AddRange(seed.Request.WorkItems.Select((item, index) =>
                CreateWorkItemEntity(
                    seed.TaskId,
                    item with { Status = seed.LegacyStatus },
                    $"workitem-{seed.TaskId}-{index + 1}")));
        }

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Fixed seed IDs make concurrent initialization idempotent.
        }
    }

    private static async Task<IReadOnlyList<EngineeringTask>> ToDomainsAsync(
        AgentWorkflowDbContext context,
        IReadOnlyList<EngineeringTaskEntity> entities,
        CancellationToken cancellationToken)
    {
        var taskIds = entities.Select(task => task.Id).ToList();
        var workItems = await context.WorkItems
            .AsNoTracking()
            .Where(item => taskIds.Contains(item.EngineeringTaskId))
            .Select(item => new { item.EngineeringTaskId, item.Id })
            .ToListAsync(cancellationToken);
        var workItemIds = workItems
            .GroupBy(item => item.EngineeringTaskId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group.Select(item => item.Id).Order().ToList(),
                StringComparer.OrdinalIgnoreCase);

        return entities.Select(entity => ToDomain(
            entity,
            workItemIds.GetValueOrDefault(entity.Id) ?? [])).ToList();
    }

    private static async Task<IReadOnlyList<string>> GetWorkItemIdsAsync(
        AgentWorkflowDbContext context,
        string taskId,
        CancellationToken cancellationToken) =>
        await context.WorkItems
            .AsNoTracking()
            .Where(item => item.EngineeringTaskId == taskId)
            .OrderBy(item => item.Id)
            .Select(item => item.Id)
            .ToListAsync(cancellationToken);

    private static EngineeringTaskEntity CreateTaskEntity(
        string taskId,
        CreateEngineeringTaskRequest request,
        DateTimeOffset now) =>
        new()
        {
            Id = taskId,
            ProjectId = request.ProjectId.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = EngineeringTaskStatus.New.ToString(),
            Priority = request.Priority.ToString(),
            CreatedAt = now,
            UpdatedAt = now
        };

    private static WorkItemEntity CreateWorkItemEntity(
        string engineeringTaskId,
        CreateWorkItemRequest request,
        string? workItemId = null) =>
        new()
        {
            Id = workItemId ?? Guid.NewGuid().ToString("N"),
            EngineeringTaskId = engineeringTaskId,
            Source = request.Source.ToString(),
            SourceKey = request.SourceKey.Trim(),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status.Trim(),
            Priority = request.Priority.Trim(),
            Tags = request.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray()
        };

    private static EngineeringTask ToDomain(
        EngineeringTaskEntity entity,
        IReadOnlyList<string> workItemIds) =>
        new(
            entity.Id,
            entity.ProjectId,
            entity.Title,
            entity.Description,
            Enum.Parse<EngineeringTaskStatus>(entity.Status),
            Enum.Parse<ScheduledTaskPriority>(entity.Priority),
            workItemIds,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CompletedAt);

    private static WorkItem ToDomain(WorkItemEntity entity) =>
        new(
            entity.Id,
            entity.EngineeringTaskId,
            Enum.Parse<WorkItemSource>(entity.Source),
            entity.SourceKey,
            entity.Title,
            entity.Description,
            entity.Status,
            entity.Priority,
            entity.Tags);

    private static void Validate(CreateEngineeringTaskRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ProjectId) || string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Project ID and task title are required.", nameof(request));
        }

        foreach (var workItem in request.WorkItems)
        {
            Validate(workItem);
        }
    }

    private static void Validate(CreateWorkItemRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SourceKey) || string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Work item source key and title are required.", nameof(request));
        }
    }
}
