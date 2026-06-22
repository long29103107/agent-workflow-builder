using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class WorkflowStateMachineTests
{
    [Fact]
    public void TransitionRun_RejectsOutOfOrderTransition()
    {
        var store = new InMemoryWorkflowRunStore();
        var run = store.CreateRun("task-1");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            store.TransitionRun(run.Id, WorkflowStage.Investigating));

        Assert.Contains("Created", exception.Message);
        Assert.Contains("Investigating", exception.Message);
        Assert.Equal(WorkflowStage.Created, store.GetRun(run.Id)!.Stage);
    }

    [Fact]
    public async Task WorkflowEngine_PersistsLeadAgentStageSequenceAndResult()
    {
        var store = new InMemoryWorkflowRunStore();
        var evidenceStore = CreateEvidenceStore();
        var engine = new WorkflowEngine(store, new RecordingLeadAgent(), evidenceStore);

        var run = await engine.StartInvestigationAsync(
            new InvestigationRequest("task-1", ".", null, null),
            CancellationToken.None);

        Assert.Equal("Completed", run.Status);
        Assert.Equal(WorkflowStage.Completed, run.Stage);
        Assert.Equal(1, run.Attempt);
        Assert.NotNull(run.Result);
        Assert.Null(run.FailureDetails);

        var stageEvents = store.GetEvents(run.Id)
            .Where(item => item.Type == "StageChanged")
            .Select(item => item.Message)
            .ToList();
        Assert.Equal(
        [
            "Workflow advanced to LoadingTaskContext.",
            "Workflow advanced to ResolvingRepository.",
            "Workflow advanced to LoadingMemory.",
            "Workflow advanced to Investigating.",
            "Workflow advanced to Aggregating.",
            "Workflow advanced to Completed."
        ], stageEvents);

        var evidence = evidenceStore.GetEvidence(run.Id);
        Assert.Equal(AgentExecutionStatus.Completed, Assert.Single(evidence.AgentExecutions).Status);
        Assert.Contains(evidence.EvidenceItems, item => item.Kind == EvidenceKind.Rationale);
        Assert.Contains(evidence.EvidenceItems, item =>
            item.Kind == EvidenceKind.SourceReference && item.SourceReference == "src/Program.cs");
        Assert.Contains(evidence.EvidenceItems, item => item.Kind == EvidenceKind.ToolResult);
        Assert.Equal("investigation-plan.json", Assert.Single(evidence.Artifacts).Name);
    }

    [Fact]
    public async Task WorkflowEngine_PersistsFailureDetails()
    {
        var store = new InMemoryWorkflowRunStore();
        var evidenceStore = CreateEvidenceStore();
        var engine = new WorkflowEngine(store, new FailingLeadAgent(), evidenceStore);

        var run = await engine.StartInvestigationAsync(
            new InvestigationRequest("task-1", ".", null, null),
            CancellationToken.None);

        Assert.Equal("Failed", run.Status);
        Assert.Equal(WorkflowStage.Failed, run.Stage);
        Assert.Equal("Investigation failed.", run.FailureDetails);
        Assert.NotNull(run.CompletedAt);
        Assert.Equal(AgentExecutionStatus.Failed, Assert.Single(evidenceStore.GetEvidence(run.Id).AgentExecutions).Status);
    }

    private static InvestigationResult Result() =>
        new(
            "Summary",
            new ExecutionPlan("Plan", [], [], []),
            [],
            new RepositoryContext(
                ".",
                "repository",
                new RepositoryConnection("Local", null, ".", "", "repository", "main", "Connected", ""),
                ["src/Program.cs"],
                [],
                "Repository context"),
            [],
            []);

    private static InMemoryWorkflowEvidenceStore CreateEvidenceStore() =>
        new(new SecretRedactor(), TimeProvider.System);

    private sealed class RecordingLeadAgent : ILeadAgent
    {
        public Task<InvestigationResult> InvestigateAsync(
            InvestigationRequest request,
            Action<WorkflowStage, string, string> advanceStage,
            CancellationToken cancellationToken)
        {
            advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
            advanceStage(WorkflowStage.ResolvingRepository, "LeadAgent", "Resolving repository.");
            advanceStage(WorkflowStage.LoadingMemory, "LeadAgent", "Loading memory.");
            advanceStage(WorkflowStage.Investigating, "LeadAgent", "Investigating.");
            advanceStage(WorkflowStage.Aggregating, "LeadAgent", "Aggregating.");
            return Task.FromResult(Result());
        }
    }

    private sealed class FailingLeadAgent : ILeadAgent
    {
        public Task<InvestigationResult> InvestigateAsync(
            InvestigationRequest request,
            Action<WorkflowStage, string, string> advanceStage,
            CancellationToken cancellationToken)
        {
            advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
            throw new InvalidOperationException("Investigation failed.");
        }
    }
}
