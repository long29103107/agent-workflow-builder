using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresTaskActivityStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory,
    ISecretRedactor redactor,
    TimeProvider timeProvider) : ITaskActivityStore
{
    public async Task<TaskActivity> AppendAsync(
        string taskId,
        Guid? workflowRunId,
        Guid correlationId,
        TaskActivityCategory category,
        string type,
        string summary,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("A correlation ID is required.", nameof(correlationId));
        }

        var entity = new TaskActivityEntity
        {
            Id = Guid.NewGuid(),
            TaskId = InMemoryTaskActivityStore.Require(taskId, nameof(taskId)),
            WorkflowRunId = workflowRunId,
            CorrelationId = correlationId,
            Category = category.ToString(),
            Type = InMemoryTaskActivityStore.Require(type, nameof(type)),
            Summary = redactor.Redact(InMemoryTaskActivityStore.Require(summary, nameof(summary))),
            Timestamp = timeProvider.GetUtcNow()
        };
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.TaskActivities.Add(entity);
        await context.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    public async Task<IReadOnlyList<TaskActivity>> GetAfterAsync(
        string taskId,
        long afterSequence,
        int limit,
        CancellationToken cancellationToken)
    {
        InMemoryTaskActivityStore.ValidateCursor(afterSequence, limit);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.TaskActivities
                .AsNoTracking()
                .Where(item => item.TaskId == taskId && item.Sequence > afterSequence)
                .OrderBy(item => item.Sequence)
                .Take(limit)
                .ToListAsync(cancellationToken))
            .Select(ToDomain)
            .ToList();
    }

    private static TaskActivity ToDomain(TaskActivityEntity item) => new(
        item.Sequence,
        item.Id,
        item.TaskId,
        item.WorkflowRunId,
        item.CorrelationId,
        Enum.Parse<TaskActivityCategory>(item.Category),
        item.Type,
        item.Summary,
        item.Timestamp);
}
