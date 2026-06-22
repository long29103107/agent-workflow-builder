using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryApprovalStore : IApprovalStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<Guid, ApprovalRecord> _approvals = [];

    public Task<IReadOnlyList<ApprovalRecord>> GetApprovalsAsync(
        string projectId,
        string taskId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<ApprovalRecord>>(
                _approvals.Values
                    .Where(item =>
                        string.Equals(item.ProjectId, projectId, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(item.TaskId, taskId, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(item => item.ApprovedAt)
                    .ToList());
        }
    }

    public Task<ApprovalRecord> AddApprovalAsync(
        ApprovalRecord approval,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_approvals.TryAdd(approval.Id, approval))
            {
                throw new InvalidOperationException($"Approval '{approval.Id}' already exists.");
            }

            return Task.FromResult(approval);
        }
    }

    public Task<ApprovalRecord> InvalidateApprovalAsync(
        Guid approvalId,
        string reason,
        DateTimeOffset invalidatedAt,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            if (!_approvals.TryGetValue(approvalId, out var current))
            {
                throw new KeyNotFoundException($"Approval '{approvalId}' was not found.");
            }

            if (current.Status == ApprovalStatus.Invalidated)
            {
                return Task.FromResult(current);
            }

            var invalidated = current with
            {
                Status = ApprovalStatus.Invalidated,
                InvalidatedAt = invalidatedAt,
                InvalidationReason = reason
            };
            _approvals[approvalId] = invalidated;
            return Task.FromResult(invalidated);
        }
    }
}
