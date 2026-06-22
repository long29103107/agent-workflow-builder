using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class ProjectTaskApiEndpoints
{
    public static RouteGroupBuilder MapProjectTaskApi(this RouteGroupBuilder api)
    {
        var tasks = api.MapGroup("/projects/{projectId}/tasks")
            .WithTags("Engineering Tasks");

        tasks.MapGet("", async (
            string projectId,
            IProjectStore projectStore,
            IEngineeringTaskStore taskStore,
            CancellationToken cancellationToken) =>
        {
            if (!await ProjectExistsAsync(projectId, projectStore, cancellationToken))
            {
                return Results.NotFound();
            }

            return Results.Ok(await taskStore.GetTasksAsync(projectId, cancellationToken));
        });

        tasks.MapPost("", async (
            string projectId,
            CreateProjectTaskRequest request,
            IProjectStore projectStore,
            IEngineeringTaskStore taskStore,
            IWorkItemStore workItemStore,
            CancellationToken cancellationToken) =>
        {
            if (!await ProjectExistsAsync(projectId, projectStore, cancellationToken))
            {
                return Results.NotFound();
            }

            try
            {
                var task = await taskStore.CreateTaskAsync(
                    new CreateEngineeringTaskRequest(
                        projectId,
                        request.Title,
                        request.Description,
                        request.Priority,
                        request.WorkItems),
                    cancellationToken);
                var workItems = await workItemStore.GetWorkItemsAsync(task.Id, cancellationToken);
                return Results.Created(
                    $"/api/projects/{projectId}/tasks/{task.Id}",
                    new EngineeringTaskDetails(task, workItems));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        tasks.MapGet("/{taskId}", async (
            string projectId,
            string taskId,
            IEngineeringTaskStore taskStore,
            IWorkItemStore workItemStore,
            CancellationToken cancellationToken) =>
        {
            var task = await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken);
            if (task is null)
            {
                return Results.NotFound();
            }

            var workItems = await workItemStore.GetWorkItemsAsync(task.Id, cancellationToken);
            return Results.Ok(new EngineeringTaskDetails(task, workItems));
        });

        tasks.MapPatch("/{taskId}/status", async (
            string projectId,
            string taskId,
            UpdateEngineeringTaskStatusRequest request,
            IEngineeringTaskStore taskStore,
            IWorkItemStore workItemStore,
            IApprovalPolicyEngine approvalPolicyEngine,
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            try
            {
                if (RequiredGate(request.Status) is { } gate)
                {
                    if (request.ApprovalBinding is null)
                    {
                        throw new ApprovalPolicyException(
                            $"{gate} approval binding is required for status '{request.Status}'.");
                    }

                    await approvalPolicyEngine.EnsureAuthorizedAsync(
                        new ApprovalAuthorizationRequest(
                            projectId,
                            taskId,
                            gate,
                            request.ApprovalBinding),
                        cancellationToken);
                }

                var task = await taskStore.UpdateStatusAsync(taskId, request.Status, cancellationToken);
                var workItems = await workItemStore.GetWorkItemsAsync(taskId, cancellationToken);
                return Results.Ok(new EngineeringTaskDetails(task!, workItems));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (ApprovalPolicyException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        tasks.MapGet("/{taskId}/approvals", async (
            string projectId,
            string taskId,
            IEngineeringTaskStore taskStore,
            IApprovalPolicyEngine approvalPolicyEngine,
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(await approvalPolicyEngine.GetApprovalsAsync(
                projectId,
                taskId,
                cancellationToken));
        });

        tasks.MapPost("/{taskId}/approvals", async (
            string projectId,
            string taskId,
            ApproveGateRequest request,
            IEngineeringTaskStore taskStore,
            IApprovalPolicyEngine approvalPolicyEngine,
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            try
            {
                var approval = await approvalPolicyEngine.ApproveAsync(
                    projectId,
                    taskId,
                    request,
                    cancellationToken);
                return Results.Created(
                    $"/api/projects/{projectId}/tasks/{taskId}/approvals/{approval.Id}",
                    approval);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        tasks.MapGet("/{taskId}/work-items", async (
            string projectId,
            string taskId,
            IEngineeringTaskStore taskStore,
            IWorkItemStore workItemStore,
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(await workItemStore.GetWorkItemsAsync(taskId, cancellationToken));
        });

        tasks.MapPost("/{taskId}/work-items", async (
            string projectId,
            string taskId,
            CreateWorkItemRequest request,
            IEngineeringTaskStore taskStore,
            IWorkItemStore workItemStore,
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            try
            {
                var workItem = await workItemStore.AddWorkItemAsync(
                    taskId,
                    request,
                    cancellationToken);
                return Results.Created(
                    $"/api/projects/{projectId}/tasks/{taskId}/work-items/{workItem.Id}",
                    workItem);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        return tasks;
    }

    private static async Task<bool> ProjectExistsAsync(
        string projectId,
        IProjectStore projectStore,
        CancellationToken cancellationToken) =>
        await projectStore.GetProjectAsync(projectId, cancellationToken) is not null;

    private static async Task<EngineeringTask?> GetProjectTaskAsync(
        string projectId,
        string taskId,
        IEngineeringTaskStore taskStore,
        CancellationToken cancellationToken)
    {
        var task = await taskStore.GetTaskAsync(taskId, cancellationToken);
        return task is not null && string.Equals(
            task.ProjectId,
            projectId,
            StringComparison.OrdinalIgnoreCase)
            ? task
            : null;
    }

    private static ApprovalGate? RequiredGate(EngineeringTaskStatus status) => status switch
    {
        EngineeringTaskStatus.ReadyForImplementation => ApprovalGate.InvestigationPlan,
        EngineeringTaskStatus.ReadyForPullRequest => ApprovalGate.Implementation,
        EngineeringTaskStatus.PullRequestOpen => ApprovalGate.PullRequest,
        EngineeringTaskStatus.Completed => ApprovalGate.Merge,
        _ => null
    };
}
