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
            CancellationToken cancellationToken) =>
        {
            if (await GetProjectTaskAsync(projectId, taskId, taskStore, cancellationToken) is null)
            {
                return Results.NotFound();
            }

            var task = await taskStore.UpdateStatusAsync(taskId, request.Status, cancellationToken);
            var workItems = await workItemStore.GetWorkItemsAsync(taskId, cancellationToken);
            return Results.Ok(new EngineeringTaskDetails(task!, workItems));
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
}
