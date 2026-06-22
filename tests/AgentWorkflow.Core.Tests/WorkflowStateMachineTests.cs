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
            store.TransitionRun(
                run.Id,
                new WorkflowStageCommand(WorkflowStage.Investigating, "task-1:investigating")));

        Assert.Contains("Created", exception.Message);
        Assert.Contains("Investigating", exception.Message);
        Assert.Equal(WorkflowStage.Created, store.GetRun(run.Id)!.Stage);
    }

    [Fact]
    public void TransitionRun_ReplaysSameCommandWithoutDuplicatingTransition()
    {
        var store = new InMemoryWorkflowRunStore();
        var run = store.CreateRun("task-1");
        var command = new WorkflowStageCommand(
            WorkflowStage.LoadingTaskContext,
            "task-1:loading-context");

        var first = store.TransitionRun(run.Id, command);
        var replayed = store.TransitionRun(run.Id, command);

        Assert.Equal(first, replayed);
        Assert.Single(store.GetEvents(run.Id), item => item.Type == "StageChanged");
    }

    [Fact]
    public async Task WorkflowEngine_ResumesInterruptedRunFromPersistedStage()
    {
        var store = new InMemoryWorkflowRunStore();
        using var interruption = new CancellationTokenSource();
        var leadAgent = new InterruptOnceLeadAgent(interruption.Cancel);
        var engine = new WorkflowEngine(
            store,
            leadAgent,
            CreateEvidenceStore(),
            CreateActivityStore());
        var request = new InvestigationRequest("task-1", ".", null, null);
        var queued = engine.QueueInvestigation(request);

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            engine.ExecuteInvestigationAsync(queued.Id, request, interruption.Token));
        Assert.Equal(WorkflowStage.LoadingMemory, store.GetRun(queued.Id)!.Stage);

        var recovered = await engine.ExecuteInvestigationAsync(
            queued.Id,
            request,
            CancellationToken.None);

        Assert.Equal(WorkflowStage.Completed, recovered.Stage);
        Assert.Equal(2, recovered.Attempt);
        Assert.Contains(store.GetEvents(queued.Id), item => item.Type == "RunRecovered");
    }

    [Fact]
    public async Task WorkflowEngine_PersistsLeadAgentStageSequenceAndResult()
    {
        var store = new InMemoryWorkflowRunStore();
        var evidenceStore = CreateEvidenceStore();
        var activityStore = CreateActivityStore();
        var engine = new WorkflowEngine(
            store,
            new RecordingLeadAgent(),
            evidenceStore,
            activityStore);

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
        var activities = await activityStore.GetAfterAsync("task-1", 0, 100, CancellationToken.None);
        Assert.Contains(activities, item => item.Category == TaskActivityCategory.Workflow);
        Assert.Contains(activities, item => item.Category == TaskActivityCategory.Agent);
        Assert.Contains(activities, item => item.Category == TaskActivityCategory.Evidence);
        Assert.Contains(activities, item => item.Category == TaskActivityCategory.Artifact);
        Assert.Equal(
            activities.OrderBy(item => item.Sequence).Select(item => item.Sequence),
            activities.Select(item => item.Sequence));
    }

    [Fact]
    public async Task WorkflowEngine_PersistsFailureDetails()
    {
        var store = new InMemoryWorkflowRunStore();
        var evidenceStore = CreateEvidenceStore();
        var engine = new WorkflowEngine(
            store,
            new FailingLeadAgent(),
            evidenceStore,
            CreateActivityStore());

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

    private static InMemoryTaskActivityStore CreateActivityStore() =>
        new(new SecretRedactor(), TimeProvider.System);

    private sealed class RecordingLeadAgent : ILeadAgent
    {
        public Task<InvestigationResult> InvestigateAsync(
            InvestigationRequest request,
            Func<WorkflowStage, string, string, Task> advanceStage,
            CancellationToken cancellationToken)
        {
            return InvestigateCoreAsync();

            async Task<InvestigationResult> InvestigateCoreAsync()
            {
                await advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
                await advanceStage(WorkflowStage.ResolvingRepository, "LeadAgent", "Resolving repository.");
                await advanceStage(WorkflowStage.LoadingMemory, "LeadAgent", "Loading memory.");
                await advanceStage(WorkflowStage.Investigating, "LeadAgent", "Investigating.");
                await advanceStage(WorkflowStage.Aggregating, "LeadAgent", "Aggregating.");
                return Result();
            }
        }
    }

    private sealed class FailingLeadAgent : ILeadAgent
    {
        public Task<InvestigationResult> InvestigateAsync(
            InvestigationRequest request,
            Func<WorkflowStage, string, string, Task> advanceStage,
            CancellationToken cancellationToken)
        {
            return FailAsync();

            async Task<InvestigationResult> FailAsync()
            {
                await advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
                throw new InvalidOperationException("Investigation failed.");
            }
        }
    }

    private sealed class InterruptOnceLeadAgent(Action interrupt) : ILeadAgent
    {
        private int _calls;

        public async Task<InvestigationResult> InvestigateAsync(
            InvestigationRequest request,
            Func<WorkflowStage, string, string, Task> advanceStage,
            CancellationToken cancellationToken)
        {
            await advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
            await advanceStage(WorkflowStage.ResolvingRepository, "LeadAgent", "Resolving repository.");
            await advanceStage(WorkflowStage.LoadingMemory, "LeadAgent", "Loading memory.");
            if (Interlocked.Increment(ref _calls) == 1)
            {
                interrupt();
                throw new OperationCanceledException(cancellationToken);
            }
            await advanceStage(WorkflowStage.Investigating, "LeadAgent", "Investigating.");
            await advanceStage(WorkflowStage.Aggregating, "LeadAgent", "Aggregating.");
            return Result();
        }
    }
}
