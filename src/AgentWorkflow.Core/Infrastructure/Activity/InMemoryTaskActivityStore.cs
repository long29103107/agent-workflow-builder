using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryTaskActivityStore(
    ISecretRedactor redactor,
    TimeProvider timeProvider) : ITaskActivityStore
{
    private readonly Lock _sync = new();
    private readonly List<TaskActivity> _items = [];
    private long _sequence;

    public Task<TaskActivity> AppendAsync(
        string taskId,
        Guid? workflowRunId,
        Guid correlationId,
        TaskActivityCategory category,
        string type,
        string summary,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (correlationId == Guid.Empty)
        {
            throw new ArgumentException("A correlation ID is required.", nameof(correlationId));
        }

        var item = new TaskActivity(
            Interlocked.Increment(ref _sequence),
            Guid.NewGuid(),
            Require(taskId, nameof(taskId)),
            workflowRunId,
            correlationId,
            category,
            Require(type, nameof(type)),
            redactor.Redact(Require(summary, nameof(summary))),
            timeProvider.GetUtcNow());
        lock (_sync)
        {
            _items.Add(item);
        }

        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<TaskActivity>> GetAfterAsync(
        string taskId,
        long afterSequence,
        int limit,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateCursor(afterSequence, limit);
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<TaskActivity>>(
                _items.Where(item =>
                        item.Sequence > afterSequence &&
                        string.Equals(item.TaskId, taskId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.Sequence)
                    .Take(limit)
                    .ToList());
        }
    }

    internal static void ValidateCursor(long afterSequence, int limit)
    {
        if (afterSequence < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(afterSequence));
        }

        if (limit is < 1 or > 500)
        {
            throw new ArgumentOutOfRangeException(nameof(limit), "Activity page size must be between 1 and 500.");
        }
    }

    internal static string Require(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A non-empty value is required.", parameterName);
}
