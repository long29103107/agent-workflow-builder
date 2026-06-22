using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRunStore _store;
    private readonly ILeadAgent _leadAgent;
    private readonly IWorkflowEvidenceStore _evidenceStore;

    public WorkflowEngine(
        IWorkflowRunStore store,
        ILeadAgent leadAgent,
        IWorkflowEvidenceStore evidenceStore)
    {
        _store = store;
        _leadAgent = leadAgent;
        _evidenceStore = evidenceStore;
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
        var execution = _evidenceStore.StartExecution(run.Id, "LeadAgent");

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
                    _evidenceStore.AppendEvidence(
                        run.Id,
                        execution.Id,
                        EvidenceKind.Action,
                        message,
                        action: stage.ToString());
                },
                cancellationToken);

            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.Rationale,
                result.Summary);
            foreach (var sourceReference in result.RepositoryContext.ImportantFiles)
            {
                _evidenceStore.AppendEvidence(
                    run.Id,
                    execution.Id,
                    EvidenceKind.SourceReference,
                    "Repository source used by the investigation.",
                    sourceReference: sourceReference);
            }

            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.ToolResult,
                "Repository context and memory lookup completed.",
                toolName: "RepositoryAndMemoryContext",
                toolResult: $"{result.RepositoryContext.ImportantFiles.Count} source files, " +
                    $"{result.MemoryItems.Count} memory items, {result.GraphEntities.Count} graph entities.");
            _evidenceStore.AppendArtifact(
                run.Id,
                execution.Id,
                "investigation-plan.json",
                "ExecutionPlan",
                JsonSerializer.Serialize(result.Plan, PersistenceJson.Options),
                "application/json");
            _evidenceStore.CompleteExecution(execution.Id, AgentExecutionStatus.Completed);

            return _store.CompleteRun(run.Id, result);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.Action,
                "Workflow execution was cancelled and may be retried.",
                action: "Cancelled");
            _evidenceStore.CompleteExecution(execution.Id, AgentExecutionStatus.Cancelled);
            throw;
        }
        catch (Exception ex)
        {
            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.Action,
                "Workflow execution failed.",
                action: ex.Message);
            _evidenceStore.CompleteExecution(execution.Id, AgentExecutionStatus.Failed);
            return _store.FailRun(run.Id, ex.Message);
        }
    }
}
