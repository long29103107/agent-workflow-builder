using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class TaskActivityStoreTests
{
    [Fact]
    public async Task AppendAndReplay_UsesExclusiveMonotonicCursorAndRedactsSummaries()
    {
        var store = new InMemoryTaskActivityStore(new SecretRedactor(), TimeProvider.System);
        var correlationId = Guid.NewGuid();
        var first = await store.AppendAsync(
            "task-1",
            null,
            correlationId,
            TaskActivityCategory.Workflow,
            "Started",
            "api_key=top-secret",
            CancellationToken.None);
        var otherTask = await store.AppendAsync(
            "task-2",
            null,
            correlationId,
            TaskActivityCategory.Workflow,
            "Started",
            "Other task",
            CancellationToken.None);
        var second = await store.AppendAsync(
            "task-1",
            null,
            correlationId,
            TaskActivityCategory.Agent,
            "Completed",
            "Agent completed",
            CancellationToken.None);

        var replay = await store.GetAfterAsync("task-1", first.Sequence, 10, CancellationToken.None);

        Assert.True(first.Sequence < otherTask.Sequence && otherTask.Sequence < second.Sequence);
        Assert.DoesNotContain("top-secret", first.Summary);
        Assert.Equal([second.Sequence], replay.Select(item => item.Sequence));
        Assert.All(replay, item => Assert.Equal(correlationId, item.CorrelationId));
    }

    [Theory]
    [InlineData(-1, 10)]
    [InlineData(0, 0)]
    [InlineData(0, 501)]
    public async Task GetAfterAsync_RejectsInvalidCursorOrReplayLimit(long cursor, int limit)
    {
        var store = new InMemoryTaskActivityStore(new SecretRedactor(), TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            store.GetAfterAsync("task-1", cursor, limit, CancellationToken.None));
    }
}
