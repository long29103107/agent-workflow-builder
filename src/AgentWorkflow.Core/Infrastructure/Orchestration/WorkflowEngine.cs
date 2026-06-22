using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRunStore _store;
    private readonly ILeadAgent _leadAgent;
    private readonly IWorkflowEvidenceStore _evidenceStore;
    private readonly ITaskActivityStore _activityStore;

    public WorkflowEngine(
        IWorkflowRunStore store,
        ILeadAgent leadAgent,
        IWorkflowEvidenceStore evidenceStore,
        ITaskActivityStore activityStore)
    {
        _store = store;
        _leadAgent = leadAgent;
        _evidenceStore = evidenceStore;
        _activityStore = activityStore;
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
        run = _store.BeginRecoveryAttempt(runId);
        var execution = _evidenceStore.StartExecution(run.Id, "LeadAgent");

        try
        {
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Workflow,
                "RunExecutionStarted",
                "Workflow execution started.",
                cancellationToken);
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Agent,
                "AgentExecutionStarted",
                "Lead Agent execution started.",
                cancellationToken);
            var result = await _leadAgent.InvestigateAsync(
                request,
                async (stage, agent, message) =>
                {
                    var current = _store.GetRun(run.Id)
                        ?? throw new InvalidOperationException($"Workflow run '{run.Id}' was not found.");
                    if (!WorkflowStateMachine.HasReached(current.Stage, stage))
                    {
                        _store.TransitionRun(
                            run.Id,
                            new WorkflowStageCommand(stage, $"{run.Id}:stage:{stage}"));
                        await AppendActivityAsync(
                            run,
                            TaskActivityCategory.Workflow,
                            "StageChanged",
                            $"Workflow advanced to {stage}.",
                            cancellationToken);
                    }
                    _store.AddEvent(run.Id, agent, "Activity", message);
                    _evidenceStore.AppendEvidence(
                        run.Id,
                        execution.Id,
                        EvidenceKind.Action,
                        message,
                        action: stage.ToString());
                    await AppendActivityAsync(
                        run,
                        TaskActivityCategory.Agent,
                        "AgentActivity",
                        $"{agent}: {message}",
                        cancellationToken);
                },
                cancellationToken);

            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.Rationale,
                result.Summary);
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Evidence,
                "RationaleRecorded",
                "Investigation rationale summary recorded.",
                cancellationToken);
            foreach (var sourceReference in result.RepositoryContext.ImportantFiles)
            {
                _evidenceStore.AppendEvidence(
                    run.Id,
                    execution.Id,
                    EvidenceKind.SourceReference,
                    "Repository source used by the investigation.",
                    sourceReference: sourceReference);
                await AppendActivityAsync(
                    run,
                    TaskActivityCategory.Evidence,
                    "SourceReferenceRecorded",
                    $"Repository source recorded: {sourceReference}",
                    cancellationToken);
            }

            _evidenceStore.AppendEvidence(
                run.Id,
                execution.Id,
                EvidenceKind.ToolResult,
                "Repository context and memory lookup completed.",
                toolName: "RepositoryAndMemoryContext",
                toolResult: $"{result.RepositoryContext.ImportantFiles.Count} source files, " +
                    $"{result.MemoryItems.Count} memory items, {result.GraphEntities.Count} graph entities.");
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Evidence,
                "ToolResultRecorded",
                "Repository and memory context result recorded.",
                cancellationToken);
            _evidenceStore.AppendArtifact(
                run.Id,
                execution.Id,
                "investigation-plan.json",
                "ExecutionPlan",
                JsonSerializer.Serialize(result.Plan, PersistenceJson.Options),
                "application/json");
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Artifact,
                "ArtifactRecorded",
                "Execution plan artifact recorded.",
                cancellationToken);
            _evidenceStore.CompleteExecution(execution.Id, AgentExecutionStatus.Completed);
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Agent,
                "AgentExecutionCompleted",
                "Lead Agent execution completed.",
                cancellationToken);

            var completed = _store.CompleteRun(run.Id, result, $"{run.Id}:complete");
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Workflow,
                "RunCompleted",
                "Workflow completed.",
                cancellationToken);
            return completed;
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
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Agent,
                "AgentExecutionCancelled",
                "Lead Agent execution was cancelled.",
                CancellationToken.None);
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
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Agent,
                "AgentExecutionFailed",
                "Lead Agent execution failed.",
                CancellationToken.None);
            var failed = _store.FailRun(run.Id, ex.Message, $"{run.Id}:attempt:{run.Attempt}:fail");
            await AppendActivityAsync(
                run,
                TaskActivityCategory.Workflow,
                "RunFailed",
                ex.Message,
                CancellationToken.None);
            return failed;
        }
    }

    private Task<TaskActivity> AppendActivityAsync(
        WorkflowRun run,
        TaskActivityCategory category,
        string type,
        string summary,
        CancellationToken cancellationToken) =>
        _activityStore.AppendAsync(
            run.TaskId,
            run.Id,
            run.Id,
            category,
            type,
            summary,
            cancellationToken);
}
