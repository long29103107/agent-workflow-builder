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
        var run = new WorkflowRun(Guid.NewGuid(), taskId, "Running", DateTimeOffset.UtcNow, null, null);
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
        var current = _runs[runId];
        var completed = current with { Status = "Completed", CompletedAt = DateTimeOffset.UtcNow, Result = result };
        _runs[runId] = completed;
        AddEvent(runId, "Workflow Engine", "RunCompleted", "Investigation completed and execution plan generated.");
        return completed;
    }

    public WorkflowRun FailRun(Guid runId, string reason)
    {
        var current = _runs[runId];
        var failed = current with { Status = "Failed", CompletedAt = DateTimeOffset.UtcNow };
        _runs[runId] = failed;
        AddEvent(runId, "Workflow Engine", "RunFailed", reason);
        return failed;
    }
}
