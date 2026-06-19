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

    private static InMemoryTaskScheduler CreateScheduler(RecordingWorkflowEngine workflow) =>
        new(new FakeTaskSource(), workflow);

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

    private sealed class RecordingWorkflowEngine(int delayMilliseconds = 0) : IWorkflowEngine
    {
        private readonly ConcurrentQueue<string> _processedTaskIds = new();

        public IReadOnlyList<string> ProcessedTaskIds => _processedTaskIds.ToList();

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
