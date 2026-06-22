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
            Result: null,
            Stage: WorkflowStage.Created,
            Attempt: 1,
            FailureDetails: null);
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

    public WorkflowRun BeginRecoveryAttempt(Guid runId)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        var stage = ParseStage(entity.Stage);
        if (stage is WorkflowStage.Completed or WorkflowStage.Failed)
        {
            throw new InvalidOperationException("Terminal workflow runs cannot be recovered.");
        }
        if (stage == WorkflowStage.Created)
        {
            return ToDomain(entity);
        }

        entity.Attempt++;
        entity.FailureDetails = null;
        context.SaveChanges();
        AddEvent(runId, "Workflow Engine", "RunRecovered", $"Workflow resumed at {stage} for attempt {entity.Attempt}.");
        return ToDomain(entity);
    }

    public WorkflowRun CompleteRun(Guid runId, InvestigationResult result, string idempotencyKey)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        if (!TryAddCommand(context, runId, WorkflowStage.Completed, idempotencyKey))
        {
            return ToDomain(entity);
        }
        WorkflowStateMachine.EnsureTransition(ParseStage(entity.Stage), WorkflowStage.Completed);
        entity.Status = "Completed";
        entity.Stage = WorkflowStage.Completed.ToString();
        entity.CompletedAt = DateTimeOffset.UtcNow;
        entity.ResultJson = JsonSerializer.Serialize(result, PersistenceJson.Options);
        entity.FailureDetails = null;
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateException) when (IsCommandApplied(runId, idempotencyKey))
        {
            return GetRun(runId)!;
        }
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Completed.");
        AddEvent(runId, "Workflow Engine", "RunCompleted", "Investigation completed and execution plan generated.");
        return ToDomain(entity);
    }

    public WorkflowRun FailRun(Guid runId, string reason, string idempotencyKey)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        if (!TryAddCommand(context, runId, WorkflowStage.Failed, idempotencyKey))
        {
            return ToDomain(entity);
        }
        WorkflowStateMachine.EnsureTransition(ParseStage(entity.Stage), WorkflowStage.Failed);
        entity.Status = "Failed";
        entity.Stage = WorkflowStage.Failed.ToString();
        entity.CompletedAt = DateTimeOffset.UtcNow;
        entity.FailureDetails = reason;
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateException) when (IsCommandApplied(runId, idempotencyKey))
        {
            return GetRun(runId)!;
        }
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Failed.");
        AddEvent(runId, "Workflow Engine", "RunFailed", reason);
        return ToDomain(entity);
    }

    public WorkflowRun TransitionRun(Guid runId, WorkflowStageCommand command)
    {
        using var context = contextFactory.CreateDbContext();
        var entity = context.WorkflowRuns.Single(run => run.Id == runId);
        if (!TryAddCommand(context, runId, command.Stage, command.IdempotencyKey))
        {
            return ToDomain(entity);
        }
        WorkflowStateMachine.EnsureTransition(ParseStage(entity.Stage), command.Stage);
        entity.Stage = command.Stage.ToString();
        try
        {
            context.SaveChanges();
        }
        catch (DbUpdateException) when (IsCommandApplied(runId, command.IdempotencyKey))
        {
            return GetRun(runId)!;
        }
        AddEvent(runId, "LeadAgent", "StageChanged", $"Workflow advanced to {command.Stage}.");
        return ToDomain(entity);
    }

    private static bool TryAddCommand(
        AgentWorkflowDbContext context,
        Guid runId,
        WorkflowStage stage,
        string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (context.WorkflowCommands.Any(item =>
            item.RunId == runId && item.IdempotencyKey == idempotencyKey))
        {
            return false;
        }

        context.WorkflowCommands.Add(new WorkflowCommandEntity
        {
            Id = Guid.NewGuid(),
            RunId = runId,
            IdempotencyKey = idempotencyKey,
            Stage = stage.ToString(),
            AppliedAt = DateTimeOffset.UtcNow
        });
        return true;
    }

    private bool IsCommandApplied(Guid runId, string idempotencyKey)
    {
        using var context = contextFactory.CreateDbContext();
        return context.WorkflowCommands.AsNoTracking().Any(item =>
            item.RunId == runId && item.IdempotencyKey == idempotencyKey);
    }

    private static WorkflowRunEntity ToEntity(WorkflowRun run) =>
        new()
        {
            Id = run.Id,
            TaskId = run.TaskId,
            Status = run.Status,
            Stage = run.Stage.ToString(),
            Attempt = run.Attempt,
            StartedAt = run.StartedAt,
            CompletedAt = run.CompletedAt,
            FailureDetails = run.FailureDetails,
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
                    PersistenceJson.Options),
            ParseStage(entity.Stage),
            entity.Attempt,
            entity.FailureDetails);

    private static WorkflowStage ParseStage(string stage) =>
        Enum.TryParse<WorkflowStage>(stage, ignoreCase: true, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown persisted workflow stage '{stage}'.");
}
