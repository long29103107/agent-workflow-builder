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
        var engine = new WorkflowEngine(store, new RecordingLeadAgent());

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
    }

    [Fact]
    public async Task WorkflowEngine_PersistsFailureDetails()
    {
        var store = new InMemoryWorkflowRunStore();
        var engine = new WorkflowEngine(store, new FailingLeadAgent());

        var run = await engine.StartInvestigationAsync(
            new InvestigationRequest("task-1", ".", null, null),
            CancellationToken.None);

        Assert.Equal("Failed", run.Status);
        Assert.Equal(WorkflowStage.Failed, run.Stage);
        Assert.Equal("Investigation failed.", run.FailureDetails);
        Assert.NotNull(run.CompletedAt);
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
                [],
                [],
                "Repository context"),
            [],
            []);

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
