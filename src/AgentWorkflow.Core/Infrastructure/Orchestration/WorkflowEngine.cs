using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRunStore _store;
    private readonly ILeadAgent _leadAgent;

    public WorkflowEngine(IWorkflowRunStore store, ILeadAgent leadAgent)
    {
        _store = store;
        _leadAgent = leadAgent;
    }

    public async Task<WorkflowRun> StartInvestigationAsync(InvestigationRequest request, CancellationToken cancellationToken)
    {
        var run = QueueInvestigation(request);
        return await ExecuteInvestigationAsync(run.Id, request, cancellationToken);
    }

    public WorkflowRun QueueInvestigation(InvestigationRequest request) =>
        _store.CreateRun(request.TaskId);

    public async Task<WorkflowRun> ExecuteInvestigationAsync(
        Guid runId,
        InvestigationRequest request,
        CancellationToken cancellationToken)
    {
        var run = _store.GetRun(runId)
            ?? throw new InvalidOperationException($"Workflow run '{runId}' was not found.");

        try
        {
            var result = await _leadAgent.InvestigateAsync(
                request,
                (stage, agent, message) =>
                {
                    var current = _store.GetRun(run.Id)
                        ?? throw new InvalidOperationException($"Workflow run '{run.Id}' was not found.");
                    if (current.Stage != stage)
                    {
                        _store.TransitionRun(run.Id, stage);
                    }
                    _store.AddEvent(run.Id, agent, "Activity", message);
                },
                cancellationToken);

            return _store.CompleteRun(run.Id, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return _store.FailRun(run.Id, ex.Message);
        }
    }
}
