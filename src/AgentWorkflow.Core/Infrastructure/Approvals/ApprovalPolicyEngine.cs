using System.Text.RegularExpressions;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed partial class ApprovalPolicyEngine(
    IProjectStore projectStore,
    IApprovalStore approvalStore,
    ITaskActivityStore activityStore,
    TimeProvider timeProvider) : IApprovalPolicyEngine
{
    public async Task<ApprovalRecord> ApproveAsync(
        string projectId,
        string taskId,
        ApproveGateRequest request,
        CancellationToken cancellationToken)
    {
        var project = await GetProjectAsync(projectId, cancellationToken);
        var binding = NormalizeAndValidate(request.Gate, request.Binding, project);
        var approvals = await approvalStore.GetApprovalsAsync(projectId, taskId, cancellationToken);
        var existing = approvals.FirstOrDefault(item =>
            item.Gate == request.Gate &&
            item.Status == ApprovalStatus.Approved &&
            item.Binding == binding);
        if (existing is not null)
        {
            return existing;
        }

        await InvalidateStaleAsync(approvals, request.Gate, binding, cancellationToken);
        var approval = new ApprovalRecord(
            Guid.NewGuid(),
            projectId,
            taskId,
            request.WorkflowRunId,
            request.Gate,
            ApprovalStatus.Approved,
            binding,
            Require(request.ApprovedBy, nameof(request.ApprovedBy)),
            timeProvider.GetUtcNow(),
            null,
            null);
        var added = await approvalStore.AddApprovalAsync(approval, cancellationToken);
        await activityStore.AppendAsync(
            taskId,
            added.WorkflowRunId,
            added.WorkflowRunId ?? added.Id,
            TaskActivityCategory.Approval,
            "ApprovalGranted",
            $"{added.Gate} approval granted for the current binding.",
            cancellationToken);
        return added;
    }

    public async Task<ApprovalRecord?> EnsureAuthorizedAsync(
        ApprovalAuthorizationRequest request,
        CancellationToken cancellationToken)
    {
        var project = await GetProjectAsync(request.ProjectId, cancellationToken);
        var binding = NormalizeAndValidate(request.Gate, request.Binding, project);
        if (!IsRequired(project.ApprovalPolicy, request.Gate))
        {
            return null;
        }

        var approvals = await approvalStore.GetApprovalsAsync(
            request.ProjectId,
            request.TaskId,
            cancellationToken);
        await InvalidateStaleAsync(approvals, request.Gate, binding, cancellationToken);
        var approval = approvals.FirstOrDefault(item =>
            item.Gate == request.Gate &&
            item.Status == ApprovalStatus.Approved &&
            item.Binding == binding);
        return approval ?? throw new ApprovalPolicyException(
            $"{request.Gate} approval is required for the current artifact, branch, or commit.");
    }

    public Task<IReadOnlyList<ApprovalRecord>> GetApprovalsAsync(
        string projectId,
        string taskId,
        CancellationToken cancellationToken) =>
        approvalStore.GetApprovalsAsync(projectId, taskId, cancellationToken);

    private async Task<Project> GetProjectAsync(string projectId, CancellationToken cancellationToken) =>
        await projectStore.GetProjectAsync(projectId, cancellationToken)
            ?? throw new KeyNotFoundException($"Project '{projectId}' was not found.");

    private async Task InvalidateStaleAsync(
        IReadOnlyList<ApprovalRecord> approvals,
        ApprovalGate gate,
        ApprovalBinding binding,
        CancellationToken cancellationToken)
    {
        foreach (var stale in approvals.Where(item =>
                     item.Gate == gate &&
                     item.Status == ApprovalStatus.Approved &&
                     item.Binding != binding))
        {
            var invalidated = await approvalStore.InvalidateApprovalAsync(
                stale.Id,
                "The approved artifact hash, target branch, or commit SHA changed.",
                timeProvider.GetUtcNow(),
                cancellationToken);
            await activityStore.AppendAsync(
                invalidated.TaskId,
                invalidated.WorkflowRunId,
                invalidated.WorkflowRunId ?? invalidated.Id,
                TaskActivityCategory.Approval,
                "ApprovalInvalidated",
                $"{invalidated.Gate} approval invalidated because its binding changed.",
                cancellationToken);
        }
    }

    private static ApprovalBinding NormalizeAndValidate(
        ApprovalGate gate,
        ApprovalBinding binding,
        Project project)
    {
        var normalized = new ApprovalBinding(
            NormalizeHash(binding.ArtifactHash),
            binding.TargetBranch?.Trim(),
            NormalizeHash(binding.CommitSha));

        if (gate is ApprovalGate.InvestigationPlan or ApprovalGate.Implementation or ApprovalGate.PullRequest)
        {
            if (normalized.ArtifactHash is null || normalized.ArtifactHash.Length != 64 ||
                !HexPattern().IsMatch(normalized.ArtifactHash))
            {
                throw new ArgumentException("A valid SHA-256 artifact hash is required for this approval gate.");
            }
        }

        if (gate is ApprovalGate.PullRequest or ApprovalGate.Merge)
        {
            ValidateBranch(normalized.TargetBranch, project);
        }

        if (gate == ApprovalGate.Merge)
        {
            if (normalized.CommitSha is null || normalized.CommitSha.Length is < 7 or > 64 ||
                !HexPattern().IsMatch(normalized.CommitSha))
            {
                throw new ArgumentException("A valid commit SHA is required for the merge approval gate.");
            }
        }

        return normalized;
    }

    private static void ValidateBranch(string? branch, Project project)
    {
        if (string.IsNullOrWhiteSpace(branch) ||
            branch.Any(char.IsWhiteSpace) ||
            branch.Contains("..", StringComparison.Ordinal) ||
            branch.StartsWith("-", StringComparison.Ordinal) ||
            !string.Equals(branch, project.BranchPolicy.BaseBranch, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                $"Target branch must match the project's protected base branch '{project.BranchPolicy.BaseBranch}'.");
        }
    }

    private static bool IsRequired(ProjectApprovalPolicy policy, ApprovalGate gate) => gate switch
    {
        ApprovalGate.InvestigationPlan => policy.RequireInvestigationPlanApproval,
        ApprovalGate.Implementation => policy.RequireImplementationApproval,
        ApprovalGate.PullRequest => policy.RequirePullRequestApproval,
        ApprovalGate.Merge => policy.RequireMergeApproval,
        _ => throw new ArgumentOutOfRangeException(nameof(gate), gate, null)
    };

    private static string? NormalizeHash(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();

    private static string Require(string value, string parameterName) =>
        !string.IsNullOrWhiteSpace(value)
            ? value.Trim()
            : throw new ArgumentException("A non-empty approver is required.", parameterName);

    [GeneratedRegex("^[0-9a-f]+$")]
    private static partial Regex HexPattern();
}
