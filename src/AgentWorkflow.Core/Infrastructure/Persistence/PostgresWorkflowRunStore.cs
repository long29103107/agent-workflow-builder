using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresWorkflowRunStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory) : IWorkflowRunStore
{
    public WorkflowRun CreateRun(string taskId)
    {
        var run = new WorkflowRun(
            Guid.NewGuid(),
            taskId,
            "Running",
            DateTimeOffset.UtcNow,
            CompletedAt: null,
            Result: null);
        using var context = contextFactory.CreateDbContext();
        context.WorkflowRuns.Add(ToEntity(run));
        context.SaveChanges();
        AddEvent(run.Id, "Workflow Engine", "RunStarted", $"Investigation started for task {taskId}.");
        return run;
    }

    public WorkflowRun? GetRun(Guid runId)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.AsNoTracking().SingleOrDefault(run => run.Id == runId);
        return entity is null ? null : ToDomain(entity);
    }

    public IReadOnlyList<WorkflowEvent> GetEvents(Guid runId)
    {
        using var context = contextFactory.CreateDbContext();
        return context.WorkflowEvents
            .AsNoTracking()
            .Where(item => item.RunId == runId)
            .OrderBy(item => item.Timestamp)
            .Select(item => new WorkflowEvent(
                item.Id,
                item.RunId,
                item.Timestamp,
                item.Agent,
                item.Type,
                item.Message))
            .ToList();
    }

    public void AddEvent(Guid runId, string agent, string type, string message)
    {
        using var context = contextFactory.CreateDbContext();
        context.WorkflowEvents.Add(new WorkflowEventEntity
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            Timestamp = DateTimeOffset.UtcNow,
            Agent = agent,
            Type = type,
            Message = message
        });
        context.SaveChanges();
    }

    public WorkflowRun CompleteRun(Guid runId, InvestigationResult result)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        entity.Status = "Completed";
        entity.CompletedAt = DateTimeOffset.UtcNow;
        entity.ResultJson = JsonSerializer.Serialize(result, PersistenceJson.Options);
        context.SaveChanges();
        AddEvent(runId, "Workflow Engine", "RunCompleted", "Investigation completed and execution plan generated.");
        return ToDomain(entity);
    }

    public WorkflowRun FailRun(Guid runId, string reason)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        entity.Status = "Failed";
        entity.CompletedAt = DateTimeOffset.UtcNow;
        context.SaveChanges();
        AddEvent(runId, "Workflow Engine", "RunFailed", reason);
        return ToDomain(entity);
    }

    private static WorkflowRunEntity ToEntity(WorkflowRun run) =>
        new()
        {
            Id = run.Id,
            TaskId = run.TaskId,
            Status = run.Status,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            ResultJson = run.Result is null
                ? null
                : JsonSerializer.Serialize(run.Result, PersistenceJson.Options)
        };

    private static WorkflowRun ToDomain(WorkflowRunEntity entity) =>
        new(
            entity.Id,
            entity.TaskId,
            entity.Status,
            entity.StartedAt,
            entity.CompletedAt,
            entity.ResultJson is null
                ? null
                : JsonSerializer.Deserialize<InvestigationResult>(
                    entity.ResultJson,
                    PersistenceJson.Options));
}
