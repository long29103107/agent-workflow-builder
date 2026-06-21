using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class WorkspaceApiEndpoints
{
    public static RouteGroupBuilder MapWorkspaceApi(this RouteGroupBuilder api)
    {
        var workspaces = api.MapGroup("/workspaces").WithTags("Workspaces");

        workspaces.MapGet("/", async (
            IWorkspaceStore store,
            CancellationToken cancellationToken) =>
        {
            return Results.Ok(await store.GetWorkspacesAsync(cancellationToken));
        });

        workspaces.MapPost("/", async (
            CreateWorkspaceRequest request,
            IWorkspaceStore store,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var workspace = await store.CreateWorkspaceAsync(request, cancellationToken);
                return Results.Created($"/api/workspaces/{workspace.Id}", workspace);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        workspaces.MapGet("/{workspaceId}", async (
            string workspaceId,
            IWorkspaceStore store,
            CancellationToken cancellationToken) =>
        {
            var workspace = await store.GetWorkspaceAsync(workspaceId, cancellationToken);
            return workspace is null ? Results.NotFound() : Results.Ok(workspace);
        });

        workspaces.MapPut("/{workspaceId}", async (
            string workspaceId,
            UpdateWorkspaceRequest request,
            IWorkspaceStore store,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var workspace = await store.UpdateWorkspaceAsync(workspaceId, request, cancellationToken);
                return workspace is null ? Results.NotFound() : Results.Ok(workspace);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        workspaces.MapGet("/{workspaceId}/requests", async (
            string workspaceId,
            IRequestIntakeStore requestStore,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            return Results.Ok(await requestStore.GetRequestsAsync(workspaceId, cancellationToken));
        });

        workspaces.MapPost("/{workspaceId}/requests", async (
            string workspaceId,
            CreateWorkspaceUserRequest request,
            IRequestIntakeStore requestStore,
            IPlannerLogStore plannerStore,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            try
            {
                var createdRequest = await requestStore.CreateRequestAsync(
                    workspaceId,
                    request,
                    cancellationToken);
                var plannerLog = await plannerStore.CreatePendingPlannerLogAsync(
                    workspaceId,
                    createdRequest,
                    cancellationToken);
                return Results.Created(
                    $"/api/workspaces/{workspaceId}/requests/{createdRequest.Id}",
                    new RequestSubmissionResult(createdRequest, plannerLog));
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        workspaces.MapGet("/{workspaceId}/planner/logs", async (
            string workspaceId,
            IPlannerLogStore plannerStore,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            return Results.Ok(await plannerStore.GetPlannerLogsAsync(workspaceId, cancellationToken));
        });

        workspaces.MapPost("/{workspaceId}/planner/logs/{plannerLogId}/approve", async (
            string workspaceId,
            string plannerLogId,
            IPlannerLogStore plannerStore,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            var result = await plannerStore.ApprovePlannerLogAsync(
                workspaceId,
                plannerLogId,
                cancellationToken);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        workspaces.MapGet("/{workspaceId}/tasks", async (
            string workspaceId,
            IWorkspaceTaskSource taskSource,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            return Results.Ok(await taskSource.GetTasksAsync(workspaceId, cancellationToken));
        });

        workspaces.MapGet("/{workspaceId}/scheduler/tasks", async (
            string workspaceId,
            ITaskScheduler scheduler,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            return Results.Ok(scheduler.GetScheduledTasks(workspaceId));
        });

        workspaces.MapPost("/{workspaceId}/scheduler/tasks", async (
            string workspaceId,
            ScheduleTaskRequest request,
            ITaskScheduler scheduler,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            var workspace = await workspaceStore.GetWorkspaceAsync(workspaceId, cancellationToken);
            if (workspace is null)
            {
                return Results.NotFound();
            }

            try
            {
                var scheduledTask = await scheduler.EnqueueAsync(
                    request with
                    {
                        WorkspaceId = workspaceId,
                        RepositoryPath = string.IsNullOrWhiteSpace(request.RepositoryPath)
                            ? workspace.RepositoryPath
                            : request.RepositoryPath,
                        RepositoryUrl = string.IsNullOrWhiteSpace(request.RepositoryUrl)
                            ? workspace.RepositoryUrl
                            : request.RepositoryUrl
                    },
                    cancellationToken);
                return Results.Created(
                    $"/api/workspaces/{workspaceId}/scheduler/tasks/{scheduledTask.Id}",
                    scheduledTask);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        workspaces.MapPost("/{workspaceId}/scheduler/process-next", async (
            string workspaceId,
            ITaskScheduler scheduler,
            IWorkspaceStore workspaceStore,
            CancellationToken cancellationToken) =>
        {
            if (!await WorkspaceExistsAsync(workspaceId, workspaceStore, cancellationToken))
            {
                return Results.NotFound();
            }

            var scheduledTask = await scheduler.ProcessNextAsync(workspaceId, cancellationToken);
            return scheduledTask is null
                ? Results.NotFound(new { error = "No queued tasks are available for this workspace." })
                : Results.Ok(scheduledTask);
        });

        workspaces.MapGet("/{workspaceId}/settings", async (
            string workspaceId,
            IWorkspaceSettingsStore settingsStore,
            CancellationToken cancellationToken) =>
        {
            var settings = await settingsStore.GetSettingsAsync(workspaceId, cancellationToken);
            return settings is null ? Results.NotFound() : Results.Ok(settings);
        });

        workspaces.MapPost("/{workspaceId}/settings", async (
            string workspaceId,
            ToolEndpointSettings settings,
            IWorkspaceSettingsStore settingsStore,
            CancellationToken cancellationToken) =>
        {
            var updated = await settingsStore.UpdateSettingsAsync(
                workspaceId,
                settings,
                cancellationToken);
            return updated is null ? Results.NotFound() : Results.Ok(updated);
        });

        return workspaces;
    }

    private static async Task<bool> WorkspaceExistsAsync(
        string workspaceId,
        IWorkspaceStore store,
        CancellationToken cancellationToken) =>
        await store.GetWorkspaceAsync(workspaceId, cancellationToken) is not null;
}
