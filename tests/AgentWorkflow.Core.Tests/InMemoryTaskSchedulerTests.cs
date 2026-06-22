using System.Collections.Concurrent;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class InMemoryTaskSchedulerTests
{
    [Fact]
    public async Task ProcessNextAsync_ProcessesHighestPriorityFirst()
    {
        var workflow = new RecordingWorkflowEngine();
        var scheduler = CreateScheduler(workflow);

        await scheduler.EnqueueAsync(Request("task-low"), CancellationToken.None);
        await scheduler.EnqueueAsync(Request("task-critical"), CancellationToken.None);
        await scheduler.EnqueueAsync(Request("task-high"), CancellationToken.None);

        await scheduler.ProcessNextAsync(CancellationToken.None);
        await scheduler.ProcessNextAsync(CancellationToken.None);
        await scheduler.ProcessNextAsync(CancellationToken.None);

        Assert.Equal(["task-critical", "task-high", "task-low"], workflow.ProcessedTaskIds);
    }

    [Fact]
    public async Task ProcessNextAsync_UsesFifoWithinSamePriority()
    {
        var workflow = new RecordingWorkflowEngine();
        var scheduler = CreateScheduler(workflow);

        await scheduler.EnqueueAsync(
            Request("task-low", ScheduledTaskPriority.High),
            CancellationToken.None);
        await scheduler.EnqueueAsync(
            Request("task-high", ScheduledTaskPriority.High),
            CancellationToken.None);

        await scheduler.ProcessNextAsync(CancellationToken.None);
        await scheduler.ProcessNextAsync(CancellationToken.None);

        Assert.Equal(["task-low", "task-high"], workflow.ProcessedTaskIds);
    }

    [Fact]
    public async Task EnqueueAsync_RejectsUnknownAndActiveDuplicateTasks()
    {
        var scheduler = CreateScheduler(new RecordingWorkflowEngine());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            scheduler.EnqueueAsync(Request("missing"), CancellationToken.None));

        await scheduler.EnqueueAsync(Request("task-high"), CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            scheduler.EnqueueAsync(Request("task-high"), CancellationToken.None));
    }

    [Fact]
    public async Task ProcessNextAsync_ClaimsDifferentItemsAcrossConcurrentCalls()
    {
        var workflow = new RecordingWorkflowEngine(delayMilliseconds: 20);
        var scheduler = CreateScheduler(workflow);

        await scheduler.EnqueueAsync(Request("task-low"), CancellationToken.None);
        await scheduler.EnqueueAsync(Request("task-high"), CancellationToken.None);

        var results = await Task.WhenAll(
            scheduler.ProcessNextAsync(CancellationToken.None),
            scheduler.ProcessNextAsync(CancellationToken.None));

        Assert.Equal(2, results.Select(result => result!.Id).Distinct().Count());
        Assert.All(results, result => Assert.Equal(ScheduledTaskStatus.Completed, result!.Status));
    }

    [Fact]
    public async Task ProcessAsync_ProcessesTheSelectedWorkspaceItem()
    {
        var workflow = new RecordingWorkflowEngine();
        var scheduler = CreateScheduler(workflow);
        var workspaceId = "workspace-a";
        var low = await scheduler.EnqueueAsync(
            Request("task-low") with { WorkspaceId = workspaceId },
            CancellationToken.None);
        var high = await scheduler.EnqueueAsync(
            Request("task-high") with { WorkspaceId = workspaceId },
            CancellationToken.None);

        var processed = await scheduler.ProcessAsync(low.Id, workspaceId, CancellationToken.None);

        Assert.Equal(low.Id, processed!.Id);
        Assert.Equal(["task-low"], workflow.ProcessedTaskIds);
        Assert.Equal(ScheduledTaskStatus.Queued, scheduler.GetScheduledTask(high.Id)!.Status);
    }

    [Fact]
    public async Task Processing_RecordsLeaseAndCancellationRequeuesTheItem()
    {
        var workflow = new RecordingWorkflowEngine(delayMilliseconds: 500);
        var scheduler = CreateScheduler(workflow);
        var queued = await scheduler.EnqueueAsync(Request("task-high"), CancellationToken.None);
        using var cancellation = new CancellationTokenSource();

        var processing = scheduler.ProcessNextAsync(cancellation.Token);
        await WaitUntilAsync(() => workflow.ProcessedTaskIds.Count == 1);

        var leased = scheduler.GetScheduledTask(queued.Id);
        Assert.Equal(ScheduledTaskStatus.Processing, leased?.Status);
        Assert.NotNull(leased?.LastHeartbeatAt);
        Assert.NotNull(leased?.LeaseExpiresAt);

        await cancellation.CancelAsync();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => processing);

        var requeued = scheduler.GetScheduledTask(queued.Id);
        Assert.Equal(ScheduledTaskStatus.Queued, requeued?.Status);
        Assert.Null(requeued?.StartedAt);
        Assert.Null(requeued?.LastHeartbeatAt);
        Assert.Null(requeued?.LeaseExpiresAt);
    }

    private static async Task WaitUntilAsync(Func<bool> condition)
    {
        for (var attempt = 0; attempt < 100 && !condition(); attempt++)
        {
            await Task.Delay(10);
        }

        Assert.True(condition());
    }

    private static InMemoryTaskScheduler CreateScheduler(RecordingWorkflowEngine workflow) =>
        new(new FakeTaskSource(), new FakeWorkspaceTaskSource(), workflow, TimeProvider.System);

    private static ScheduleTaskRequest Request(
        string taskId,
        ScheduledTaskPriority? priority = null) =>
        new(taskId, priority, RepositoryPath: ".", RepositoryUrl: null);

    private sealed class FakeTaskSource : ITaskSource
    {
        private static readonly IReadOnlyList<TaskItem> Tasks =
        [
            new("task-low", "Test", "LOW-1", "Low task", "", "Ready", "Low", []),
            new("task-high", "Test", "HIGH-1", "High task", "", "Ready", "High", []),
            new("task-critical", "Test", "CRIT-1", "Critical task", "", "Ready", "Critical", [])
        ];

        public Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Tasks);

        public Task<TaskItem?> GetTaskAsync(string taskId, CancellationToken cancellationToken) =>
            Task.FromResult(Tasks.FirstOrDefault(task => task.Id == taskId));
    }

    private sealed class FakeWorkspaceTaskSource : IWorkspaceTaskSource
    {
        private readonly FakeTaskSource _taskSource = new();

        public Task<IReadOnlyList<TaskItem>> GetTasksAsync(
            string workspaceId,
            CancellationToken cancellationToken) =>
            _taskSource.GetTasksAsync(cancellationToken);

        public Task<TaskItem?> GetTaskAsync(
            string workspaceId,
            string taskId,
            CancellationToken cancellationToken) =>
            _taskSource.GetTaskAsync(taskId, cancellationToken);
    }

    private sealed class RecordingWorkflowEngine(int delayMilliseconds = 0) : IWorkflowEngine
    {
        private readonly ConcurrentQueue<string> _processedTaskIds = new();

        public IReadOnlyList<string> ProcessedTaskIds => _processedTaskIds.ToList();

        public WorkflowRun QueueInvestigation(InvestigationRequest request) =>
            new(
                Guid.NewGuid(),
                request.TaskId,
                "Running",
                DateTimeOffset.UtcNow,
                null,
                Result: null);

        public async Task<WorkflowRun> ExecuteInvestigationAsync(
            Guid runId,
            InvestigationRequest request,
            CancellationToken cancellationToken)
        {
            var completed = await StartInvestigationAsync(request, cancellationToken);
            return completed with { Id = runId };
        }

        public async Task<WorkflowRun> StartInvestigationAsync(
            InvestigationRequest request,
            CancellationToken cancellationToken)
        {
            _processedTaskIds.Enqueue(request.TaskId);

            if (delayMilliseconds > 0)
            {
                await Task.Delay(delayMilliseconds, cancellationToken);
            }

            return new WorkflowRun(
                Guid.NewGuid(),
                request.TaskId,
                "Completed",
                DateTimeOffset.UtcNow,
                DateTimeOffset.UtcNow,
                Result: null);
        }
    }
}
