using System.Collections.Concurrent;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryWorkflowRunStore : IWorkflowRunStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowRun> _runs = new();
    private readonly ConcurrentDictionary<Guid, List<WorkflowEvent>> _events = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, byte>> _commands = new();

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
        _runs[run.Id] = run;
        _events[run.Id] = [];
        _commands[run.Id] = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
        AddEvent(run.Id, "Workflow Engine", "RunStarted", $"Investigation started for task {taskId}.");
        return run;
    }

    public WorkflowRun? GetRun(Guid runId) => _runs.TryGetValue(runId, out var run) ? run : null;

    public IReadOnlyList<WorkflowEvent> GetEvents(Guid runId) =>
        _events.TryGetValue(runId, out var events) ? events.OrderBy(e => e.Timestamp).ToList() : [];

    public void AddEvent(Guid runId, string agent, string type, string message)
    {
        var workflowEvent = new WorkflowEvent(Guid.NewGuid(), runId, DateTimeOffset.UtcNow, agent, type, message);
        var events = _events.GetOrAdd(runId, _ => []);

        lock (events)
        {
            events.Add(workflowEvent);
        }
    }

    public WorkflowRun BeginRecoveryAttempt(Guid runId)
    {
        var recovered = UpdateRun(runId, current =>
        {
            if (current.Stage is WorkflowStage.Completed or WorkflowStage.Failed)
            {
                throw new InvalidOperationException("Terminal workflow runs cannot be recovered.");
            }

            return current.Stage == WorkflowStage.Created
                ? current
                : current with { Attempt = current.Attempt + 1, FailureDetails = null };
        });
        if (recovered.Stage != WorkflowStage.Created)
        {
            AddEvent(runId, "Workflow Engine", "RunRecovered", $"Workflow resumed at {recovered.Stage} for attempt {recovered.Attempt}.");
        }
        return recovered;
    }

    public WorkflowRun CompleteRun(Guid runId, InvestigationResult result, string idempotencyKey)
    {
        if (!TryApplyCommand(runId, idempotencyKey))
        {
            return GetRequiredRun(runId);
        }
        WorkflowRun completed;
        try
        {
            completed = UpdateRun(runId, current =>
            {
                WorkflowStateMachine.EnsureTransition(current.Stage, WorkflowStage.Completed);
                return current with
                {
                    Status = "Completed",
                    Stage = WorkflowStage.Completed,
                    CompletedAt = DateTimeOffset.UtcNow,
                    Result = result,
                    FailureDetails = null
                };
            });
        }
        catch
        {
            RemoveCommand(runId, idempotencyKey);
            throw;
        }
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Completed.");
        AddEvent(runId, "Workflow Engine", "RunCompleted", "Investigation completed and execution plan generated.");
        return completed;
    }

    public WorkflowRun FailRun(Guid runId, string reason, string idempotencyKey)
    {
        if (!TryApplyCommand(runId, idempotencyKey))
        {
            return GetRequiredRun(runId);
        }
        WorkflowRun failed;
        try
        {
            failed = UpdateRun(runId, current =>
            {
                WorkflowStateMachine.EnsureTransition(current.Stage, WorkflowStage.Failed);
                return current with
                {
                    Status = "Failed",
                    Stage = WorkflowStage.Failed,
                    CompletedAt = DateTimeOffset.UtcNow,
                    FailureDetails = reason
                };
            });
        }
        catch
        {
            RemoveCommand(runId, idempotencyKey);
            throw;
        }
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Failed.");
        AddEvent(runId, "Workflow Engine", "RunFailed", reason);
        return failed;
    }

    public WorkflowRun TransitionRun(Guid runId, WorkflowStageCommand command)
    {
        if (!TryApplyCommand(runId, command.IdempotencyKey))
        {
            return GetRequiredRun(runId);
        }
        WorkflowRun transitioned;
        try
        {
            transitioned = UpdateRun(runId, current =>
            {
                WorkflowStateMachine.EnsureTransition(current.Stage, command.Stage);
                return current with { Stage = command.Stage };
            });
        }
        catch
        {
            RemoveCommand(runId, command.IdempotencyKey);
            throw;
        }
        AddEvent(runId, "LeadAgent", "StageChanged", $"Workflow advanced to {command.Stage}.");
        return transitioned;
    }

    private bool TryApplyCommand(Guid runId, string idempotencyKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idempotencyKey);
        if (!_runs.ContainsKey(runId))
        {
            throw new KeyNotFoundException($"Workflow run '{runId}' was not found.");
        }
        return _commands.GetOrAdd(runId, _ => new ConcurrentDictionary<string, byte>(StringComparer.Ordinal))
            .TryAdd(idempotencyKey, 0);
    }

    private WorkflowRun GetRequiredRun(Guid runId) =>
        GetRun(runId) ?? throw new KeyNotFoundException($"Workflow run '{runId}' was not found.");

    private void RemoveCommand(Guid runId, string idempotencyKey)
    {
        if (_commands.TryGetValue(runId, out var commands))
        {
            commands.TryRemove(idempotencyKey, out _);
        }
    }

    private WorkflowRun UpdateRun(Guid runId, Func<WorkflowRun, WorkflowRun> update)
    {
        while (true)
        {
            if (!_runs.TryGetValue(runId, out var current))
            {
                throw new KeyNotFoundException($"Workflow run '{runId}' was not found.");
            }

            var updated = update(current);
            if (_runs.TryUpdate(runId, updated, current))
            {
                return updated;
            }
        }
    }
}
