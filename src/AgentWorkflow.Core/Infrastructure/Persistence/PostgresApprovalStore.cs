using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresApprovalStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory) : IApprovalStore
{
    public async Task<IReadOnlyList<ApprovalRecord>> GetApprovalsAsync(
        string projectId,
        string taskId,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return (await context.Approvals
                .AsNoTracking()
                .Where(item => item.ProjectId == projectId && item.TaskId == taskId)
                .OrderBy(item => item.ApprovedAt)
                .ToListAsync(cancellationToken))
            .Select(ToDomain)
            .ToList();
    }

    public async Task<ApprovalRecord> AddApprovalAsync(
        ApprovalRecord approval,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        context.Approvals.Add(ToEntity(approval));
        await context.SaveChangesAsync(cancellationToken);
        return approval;
    }

    public async Task<ApprovalRecord> InvalidateApprovalAsync(
        Guid approvalId,
        string reason,
        DateTimeOffset invalidatedAt,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Approvals.SingleOrDefaultAsync(
            item => item.Id == approvalId,
            cancellationToken) ?? throw new KeyNotFoundException($"Approval '{approvalId}' was not found.");
        if (ParseStatus(entity.Status) == ApprovalStatus.Invalidated)
        {
            return ToDomain(entity);
        }

        entity.Status = ApprovalStatus.Invalidated.ToString();
        entity.InvalidatedAt = invalidatedAt;
        entity.InvalidationReason = reason;
        await context.SaveChangesAsync(cancellationToken);
        return ToDomain(entity);
    }

    private static ApprovalEntity ToEntity(ApprovalRecord item) => new()
    {
        Id = item.Id,
        ProjectId = item.ProjectId,
        TaskId = item.TaskId,
        WorkflowRunId = item.WorkflowRunId,
        Gate = item.Gate.ToString(),
        Status = item.Status.ToString(),
        ArtifactHash = item.Binding.ArtifactHash,
        TargetBranch = item.Binding.TargetBranch,
        CommitSha = item.Binding.CommitSha,
        ApprovedBy = item.ApprovedBy,
        ApprovedAt = item.ApprovedAt,
        InvalidatedAt = item.InvalidatedAt,
        InvalidationReason = item.InvalidationReason
    };

    private static ApprovalRecord ToDomain(ApprovalEntity item) => new(
        item.Id,
        item.ProjectId,
        item.TaskId,
        item.WorkflowRunId,
        Enum.Parse<ApprovalGate>(item.Gate),
        ParseStatus(item.Status),
        new ApprovalBinding(item.ArtifactHash, item.TargetBranch, item.CommitSha),
        item.ApprovedBy,
        item.ApprovedAt,
        item.InvalidatedAt,
        item.InvalidationReason);

    private static ApprovalStatus ParseStatus(string status) =>
        Enum.TryParse<ApprovalStatus>(status, out var parsed)
            ? parsed
            : throw new InvalidOperationException($"Unknown approval status '{status}'.");
}
