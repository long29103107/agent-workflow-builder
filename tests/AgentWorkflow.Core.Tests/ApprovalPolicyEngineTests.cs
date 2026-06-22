using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class ApprovalPolicyEngineTests
{
    [Fact]
    public async Task ApproveAsync_IsIdempotentAndInvalidatesApprovalWhenInputsChange()
    {
        var engine = CreateEngine();
        var firstBinding = PlanBinding("plan-v1");
        var secondBinding = PlanBinding("plan-v2");

        var first = await engine.ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(ApprovalGate.InvestigationPlan, firstBinding, "reviewer"),
            CancellationToken.None);
        var duplicate = await engine.ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(ApprovalGate.InvestigationPlan, firstBinding, "reviewer"),
            CancellationToken.None);
        var replacement = await engine.ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(ApprovalGate.InvestigationPlan, secondBinding, "reviewer"),
            CancellationToken.None);

        Assert.Equal(first.Id, duplicate.Id);
        Assert.NotEqual(first.Id, replacement.Id);
        var approvals = await engine.GetApprovalsAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            CancellationToken.None);
        Assert.Equal(ApprovalStatus.Invalidated, approvals.Single(item => item.Id == first.Id).Status);
        Assert.Equal(ApprovalStatus.Approved, approvals.Single(item => item.Id == replacement.Id).Status);
    }

    [Fact]
    public async Task EnsureAuthorizedAsync_RejectsMissingApprovalAndAcceptsExactBinding()
    {
        var engine = CreateEngine();
        var binding = new ApprovalBinding(
            ApprovalInputHasher.Compute("diff"),
            "main",
            null);
        var authorization = new ApprovalAuthorizationRequest(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            ApprovalGate.PullRequest,
            binding);

        await Assert.ThrowsAsync<ApprovalPolicyException>(() =>
            engine.EnsureAuthorizedAsync(authorization, CancellationToken.None));

        var approved = await engine.ApproveAsync(
            authorization.ProjectId,
            authorization.TaskId,
            new ApproveGateRequest(authorization.Gate, binding, "reviewer"),
            CancellationToken.None);
        var authorized = await engine.EnsureAuthorizedAsync(authorization, CancellationToken.None);

        Assert.Equal(approved.Id, authorized!.Id);
    }

    [Fact]
    public async Task ChangedBinding_ProjectsApprovalGrantAndInvalidationIntoTaskHistory()
    {
        var activityStore = new InMemoryTaskActivityStore(new SecretRedactor(), TimeProvider.System);
        var engine = new ApprovalPolicyEngine(
            new InMemoryProjectStore(),
            new InMemoryApprovalStore(),
            activityStore,
            TimeProvider.System);

        await engine.ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(ApprovalGate.InvestigationPlan, PlanBinding("v1"), "reviewer"),
            CancellationToken.None);
        await engine.ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(ApprovalGate.InvestigationPlan, PlanBinding("v2"), "reviewer"),
            CancellationToken.None);

        var activities = await activityStore.GetAfterAsync("task-1", 0, 10, CancellationToken.None);
        Assert.Equal(
            ["ApprovalGranted", "ApprovalInvalidated", "ApprovalGranted"],
            activities.Select(item => item.Type));
        Assert.All(activities, item => Assert.Equal(TaskActivityCategory.Approval, item.Category));
    }

    [Theory]
    [InlineData(ApprovalGate.InvestigationPlan)]
    [InlineData(ApprovalGate.Implementation)]
    [InlineData(ApprovalGate.PullRequest)]
    [InlineData(ApprovalGate.Merge)]
    public async Task ApproveAsync_ValidatesGateSpecificBinding(ApprovalGate gate)
    {
        var engine = CreateEngine();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            engine.ApproveAsync(
                ProjectPolicyDefaults.DefaultProjectId,
                "task-1",
                new ApproveGateRequest(gate, new ApprovalBinding(null, null, null), "reviewer"),
                CancellationToken.None));
    }

    [Theory]
    [MemberData(nameof(ValidGateBindings))]
    public async Task ApproveAsync_AcceptsGateSpecificBinding(
        ApprovalGate gate,
        ApprovalBinding binding)
    {
        var approval = await CreateEngine().ApproveAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            "task-1",
            new ApproveGateRequest(gate, binding, "reviewer"),
            CancellationToken.None);

        Assert.Equal(gate, approval.Gate);
        Assert.Equal(ApprovalStatus.Approved, approval.Status);
    }

    public static TheoryData<ApprovalGate, ApprovalBinding> ValidGateBindings => new()
    {
        { ApprovalGate.InvestigationPlan, PlanBinding("plan") },
        { ApprovalGate.Implementation, PlanBinding("implementation") },
        { ApprovalGate.PullRequest, PlanBinding("pull-request") },
        { ApprovalGate.Merge, new ApprovalBinding(null, "main", "deadbeef") }
    };

    private static ApprovalPolicyEngine CreateEngine() =>
        new(
            new InMemoryProjectStore(),
            new InMemoryApprovalStore(),
            new InMemoryTaskActivityStore(new SecretRedactor(), TimeProvider.System),
            TimeProvider.System);

    private static ApprovalBinding PlanBinding(string content) =>
        new(ApprovalInputHasher.Compute(content), "main", null);
}
