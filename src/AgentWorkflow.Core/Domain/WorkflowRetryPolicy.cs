namespace AgentWorkflow.Core.Domain;

public static class WorkflowRetryPolicy
{
    public static async Task<T> ExecuteAsync<T>(
        WorkflowOperationKind operation,
        Func<CancellationToken, Task<T>> action,
        int maxAttempts,
        CancellationToken cancellationToken)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        if (operation != WorkflowOperationKind.TransientRead && maxAttempts > 1)
        {
            throw new InvalidOperationException(
                $"Blind retry is not allowed for {operation}. Resume it with a persisted idempotency key instead.");
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await action(cancellationToken);
            }
            catch (TransientWorkflowException) when (attempt < maxAttempts)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
        }
    }

    public static void EnsureExternalWriteCommand(ExternalWriteCommand command)
    {
        if (command.Operation == WorkflowOperationKind.TransientRead)
        {
            throw new InvalidOperationException("Transient reads are not external write commands.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(command.IdempotencyKey);
    }
}

public sealed class TransientWorkflowException(string message, Exception? innerException = null)
    : Exception(message, innerException);
