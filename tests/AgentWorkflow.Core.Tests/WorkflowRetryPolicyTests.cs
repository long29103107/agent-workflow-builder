using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Tests;

public sealed class WorkflowRetryPolicyTests
{
    [Fact]
    public async Task ExecuteAsync_RetriesTransientReads()
    {
        var attempts = 0;

        var result = await WorkflowRetryPolicy.ExecuteAsync(
            WorkflowOperationKind.TransientRead,
            _ =>
            {
                attempts++;
                return attempts < 3
                    ? Task.FromException<string>(new TransientWorkflowException("Temporary read failure."))
                    : Task.FromResult("ok");
            },
            maxAttempts: 3,
            CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(3, attempts);
    }

    [Theory]
    [InlineData(WorkflowOperationKind.Commit)]
    [InlineData(WorkflowOperationKind.Push)]
    [InlineData(WorkflowOperationKind.CreatePullRequest)]
    [InlineData(WorkflowOperationKind.Merge)]
    public async Task ExecuteAsync_RejectsBlindExternalWriteRetry(WorkflowOperationKind operation)
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            WorkflowRetryPolicy.ExecuteAsync(
                operation,
                _ => Task.FromResult("not-run"),
                maxAttempts: 2,
                CancellationToken.None));
    }

    [Fact]
    public void EnsureExternalWriteCommand_RequiresIdempotencyKey()
    {
        Assert.Throws<ArgumentException>(() =>
            WorkflowRetryPolicy.EnsureExternalWriteCommand(
                new ExternalWriteCommand(WorkflowOperationKind.Push, "")));
    }
}
