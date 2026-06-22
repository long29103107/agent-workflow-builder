using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryPlannerLogStore(
    IProjectStore projectStore,
    IApprovalPolicyEngine approvalPolicyEngine) : IPlannerLogStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, List<PlannerLog>> _logs = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, List<TaskItem>> _tasks = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _nextTaskNumbers = new(StringComparer.OrdinalIgnoreCase);

    public Task<IReadOnlyList<PlannerLog>> GetPlannerLogsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<PlannerLog>>(
                _logs.TryGetValue(workspaceId, out var logs)
                    ? logs.OrderByDescending(log => log.CreatedAt).ToList()
                    : []);
        }
    }

    public Task<PlannerLog?> GetPlannerLogAsync(
        string workspaceId,
        string plannerLogId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var log = _logs.TryGetValue(workspaceId, out var logs)
                ? logs.FirstOrDefault(item => string.Equals(item.Id, plannerLogId, StringComparison.OrdinalIgnoreCase))
                : null;
            return Task.FromResult(log);
        }
    }

    public Task<PlannerLog> CreatePendingPlannerLogAsync(
        string workspaceId,
        WorkspaceUserRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var log = new PlannerLog(
            Guid.NewGuid().ToString("N"),
            workspaceId,
            request.Id,
            request.Content,
            PlannerLogStatus.PendingApproval,
            CreatePlannerSteps(request.Content),
            now,
            now);

        lock (_sync)
        {
            if (!_logs.TryGetValue(workspaceId, out var logs))
            {
                logs = [];
                _logs[workspaceId] = logs;
            }

            logs.Add(log);
        }

        return Task.FromResult(log);
    }

    public Task<PlannerLog?> UpdatePlannerLogAsync(
        string workspaceId,
        string plannerLogId,
        UpdatePlannerLogRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateSteps(request.Steps);

        lock (_sync)
        {
            if (!_logs.TryGetValue(workspaceId, out var logs))
            {
                return Task.FromResult<PlannerLog?>(null);
            }

            var index = logs.FindIndex(log =>
                string.Equals(log.Id, plannerLogId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return Task.FromResult<PlannerLog?>(null);
            }

            var current = logs[index];
            if (current.Status != PlannerLogStatus.PendingApproval)
            {
                throw new InvalidOperationException("Only pending planner logs can be edited.");
            }

            var updated = current with
            {
                Steps = request.Steps.Select(step => new PlannerStep(
                    step.Title.Trim(),
                    step.Detail.Trim(),
                    step.Owner.Trim())).ToList(),
                UpdatedAt = DateTimeOffset.UtcNow
            };
            logs[index] = updated;
            return Task.FromResult<PlannerLog?>(updated);
        }
    }

    public async Task<PlannerApprovalResult?> ApprovePlannerLogAsync(
        string workspaceId,
        string plannerLogId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var project = await projectStore.GetProjectAsync(workspaceId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        PlannerLog snapshot;
        lock (_sync)
        {
            if (!_logs.TryGetValue(workspaceId, out var logs))
            {
                return null;
            }

            var index = logs.FindIndex(log =>
                string.Equals(log.Id, plannerLogId, StringComparison.OrdinalIgnoreCase));
            if (index < 0)
            {
                return null;
            }

            snapshot = logs[index];
        }

        var binding = new ApprovalBinding(
            ApprovalInputHasher.Compute(JsonSerializer.Serialize(
                new { snapshot.RequestId, snapshot.Steps },
                PersistenceJson.Options)),
            project.BranchPolicy.BaseBranch,
            CommitSha: null);
        var approval = await approvalPolicyEngine.ApproveAsync(
            workspaceId,
            snapshot.Id,
            new ApproveGateRequest(
                ApprovalGate.InvestigationPlan,
                binding,
                ApprovedBy: "workspace-user"),
            cancellationToken);
        await approvalPolicyEngine.EnsureAuthorizedAsync(
            new ApprovalAuthorizationRequest(
                workspaceId,
                snapshot.Id,
                ApprovalGate.InvestigationPlan,
                binding),
            cancellationToken);

        lock (_sync)
        {
            var logs = _logs[workspaceId];
            var index = logs.FindIndex(log =>
                string.Equals(log.Id, plannerLogId, StringComparison.OrdinalIgnoreCase));
            var current = logs[index];
            if (current.UpdatedAt != snapshot.UpdatedAt)
            {
                throw new InvalidOperationException(
                    "The planner log changed while approval was being recorded. Retry approval for the latest plan.");
            }

            if (!_tasks.TryGetValue(workspaceId, out var tasks))
            {
                tasks = [];
                _tasks[workspaceId] = tasks;
            }

            var generated = tasks
                .Where(task => task.Id.StartsWith($"planner-{current.Id}-", StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (current.Status == PlannerLogStatus.PendingApproval)
            {
                current = current with
                {
                    Status = PlannerLogStatus.Approved,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                logs[index] = current;
                var nextNumber = _nextTaskNumbers.GetValueOrDefault(workspaceId, 1);
                generated = CreateTasks(current, project.Code, nextNumber).ToList();
                _nextTaskNumbers[workspaceId] = nextNumber + generated.Count;
                tasks.InsertRange(0, generated);
            }

            return new PlannerApprovalResult(current, generated, approval);
        }
    }

    public Task<IReadOnlyList<TaskItem>> GetGeneratedTasksAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<TaskItem>>(
                _tasks.TryGetValue(workspaceId, out var tasks) ? tasks.ToList() : []);
        }
    }

    public Task<TaskItem?> GetGeneratedTaskAsync(
        string workspaceId,
        string taskId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            var task = _tasks.TryGetValue(workspaceId, out var tasks)
                ? tasks.FirstOrDefault(item =>
                    string.Equals(item.Id, taskId, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(item.Key, taskId, StringComparison.OrdinalIgnoreCase))
                : null;
            return Task.FromResult(task);
        }
    }

    private static IReadOnlyList<PlannerStep> CreatePlannerSteps(string request) =>
    [
        new("Capture request", request, "Request intake"),
        new("Ground in work item", "Create workspace-scoped work items for execution.", "Agent planner"),
        new("Plan execution", "Break the request into repository investigation, implementation, verification, and review slices.", "Lead agent"),
        new("Prepare processing", "Queue approved tasks and process the next workspace priority item.", "Scheduler")
    ];

    private static IEnumerable<TaskItem> CreateTasks(PlannerLog log, string projectCode, int firstNumber) =>
        log.Steps.Select((step, index) => new TaskItem(
            $"planner-{log.Id}-{index + 1}",
            "agent-planner",
            $"{projectCode}-{firstNumber + index}",
            step.Title,
            step.Detail,
            "Backlog",
            index == 0 ? "High" : "Medium",
            [step.Owner, "planner"],
            step.Owner));

    private static void ValidateSteps(IReadOnlyList<PlannerStep> steps)
    {
        if (steps is null || steps.Count == 0)
        {
            throw new ArgumentException("At least one planner step is required.", nameof(steps));
        }

        if (steps.Any(step =>
            string.IsNullOrWhiteSpace(step.Title) ||
            string.IsNullOrWhiteSpace(step.Detail) ||
            string.IsNullOrWhiteSpace(step.Owner)))
        {
            throw new ArgumentException(
                "Every planner step requires a title, detail, and owner.",
                nameof(steps));
        }
    }
}
