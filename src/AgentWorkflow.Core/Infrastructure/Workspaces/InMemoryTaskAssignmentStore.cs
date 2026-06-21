using AgentWorkflow.Core.Application;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryTaskAssignmentStore : ITaskAssignmentStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Dictionary<string, string>> _assignments =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyDictionary<string, string>> GetAssignmentsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            IReadOnlyDictionary<string, string> result =
                _assignments.TryGetValue(workspaceId, out var assignments)
                    ? new Dictionary<string, string>(assignments, StringComparer.OrdinalIgnoreCase)
                    : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return Task.FromResult(result);
        }
    }

    public Task<string> AssignAgentAsync(
        string workspaceId,
        string taskId,
        string agentName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrWhiteSpace(agentName))
        {
            throw new ArgumentException("Agent name is required.", nameof(agentName));
        }

        lock (_sync)
        {
            if (!_assignments.TryGetValue(workspaceId, out var assignments))
            {
                assignments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                _assignments[workspaceId] = assignments;
            }

            var normalized = agentName.Trim();
            assignments[taskId] = normalized;
            return Task.FromResult(normalized);
        }
    }
}
