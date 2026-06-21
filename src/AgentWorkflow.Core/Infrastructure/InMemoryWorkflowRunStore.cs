using System.Collections.Concurrent;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryWorkflowRunStore : IWorkflowRunStore
{
    private readonly ConcurrentDictionary<Guid, WorkflowRun> _runs = new();
    private readonly ConcurrentDictionary<Guid, List<WorkflowEvent>> _events = new();

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

    public WorkflowRun CompleteRun(Guid runId, InvestigationResult result)
    {
        var completed = UpdateRun(runId, current =>
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
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Completed.");
        AddEvent(runId, "Workflow Engine", "RunCompleted", "Investigation completed and execution plan generated.");
        return completed;
    }

    public WorkflowRun FailRun(Guid runId, string reason)
    {
        var failed = UpdateRun(runId, current =>
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
        AddEvent(runId, "Workflow Engine", "StageChanged", "Workflow advanced to Failed.");
        AddEvent(runId, "Workflow Engine", "RunFailed", reason);
        return failed;
    }

    public WorkflowRun TransitionRun(Guid runId, WorkflowStage nextStage)
    {
        var transitioned = UpdateRun(runId, current =>
        {
            WorkflowStateMachine.EnsureTransition(current.Stage, nextStage);
            return current with { Stage = nextStage };
        });
        AddEvent(runId, "LeadAgent", "StageChanged", $"Workflow advanced to {nextStage}.");
        return transitioned;
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
