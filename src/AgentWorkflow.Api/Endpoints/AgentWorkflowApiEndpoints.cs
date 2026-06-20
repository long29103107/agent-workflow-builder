using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class AgentWorkflowApiEndpoints
{
    public static RouteGroupBuilder MapAgentWorkflowApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");
        api.MapWorkspaceApi();

        api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

        api.MapGet("/tasks", async (ITaskSource tasks, CancellationToken cancellationToken) =>
        {
            var result = await tasks.GetTasksAsync(cancellationToken);
            return Results.Ok(result);
        });

        var scheduler = api.MapGroup("/scheduler");

        scheduler.MapGet("/tasks", (ITaskScheduler taskScheduler) =>
        {
            return Results.Ok(taskScheduler.GetScheduledTasks());
        });

        scheduler.MapGet("/tasks/{scheduledTaskId:guid}", (
            Guid scheduledTaskId,
            ITaskScheduler taskScheduler) =>
        {
            var scheduledTask = taskScheduler.GetScheduledTask(scheduledTaskId);
            return scheduledTask is null ? Results.NotFound() : Results.Ok(scheduledTask);
        });

        scheduler.MapPost("/tasks", async (
            ScheduleTaskRequest request,
            ITaskScheduler taskScheduler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var scheduledTask = await taskScheduler.EnqueueAsync(request, cancellationToken);
                return Results.Created($"/api/scheduler/tasks/{scheduledTask.Id}", scheduledTask);
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

        scheduler.MapPost("/process-next", async (
            ITaskScheduler taskScheduler,
            CancellationToken cancellationToken) =>
        {
            var scheduledTask = await taskScheduler.ProcessNextAsync(cancellationToken);
            return scheduledTask is null
                ? Results.NotFound(new { error = "No queued tasks are available." })
                : Results.Ok(scheduledTask);
        });

        api.MapPost("/workflows/investigate", async (
            InvestigationRequest request,
            IWorkflowEngine workflowEngine,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.TaskId))
            {
                return Results.BadRequest(new { error = "taskId is required." });
            }

            var run = await workflowEngine.StartInvestigationAsync(request, cancellationToken);
            return Results.Created($"/api/workflows/{run.Id}", run);
        });

        api.MapGet("/workflows/{runId:guid}", (Guid runId, IWorkflowRunStore store) =>
        {
            var run = store.GetRun(runId);
            return run is null ? Results.NotFound() : Results.Ok(run);
        });

        api.MapGet("/workflows/{runId:guid}/events", (Guid runId, IWorkflowRunStore store) =>
        {
            if (store.GetRun(runId) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(store.GetEvents(runId));
        });

        api.MapGet("/memory/search", async (
            string query,
            IMemoryService memory,
            CancellationToken cancellationToken) =>
        {
            var results = await memory.SearchVectorMemoryAsync(query, cancellationToken);
            return Results.Ok(results);
        });

        api.MapPost("/memory", async (
            MemoryItem item,
            IMemoryService memory,
            CancellationToken cancellationToken) =>
        {
            var stored = await memory.StoreMemoryAsync(item, cancellationToken);
            return Results.Created($"/api/memory/search?query={Uri.EscapeDataString(stored.Title)}", stored);
        });

        api.MapGet("/repos/context", async (
            string? path,
            string? url,
            IRepositoryConnectionService repositoryConnection,
            IRepositoryReader repositoryReader,
            CancellationToken cancellationToken) =>
        {
            var connection = repositoryConnection.ResolveConnection(path, url);
            var context = await repositoryReader.GetContextAsync(connection, cancellationToken);
            return Results.Ok(context);
        });

        api.MapGet("/repos/connection", (IRepositoryConnectionService repositoryConnection) =>
        {
            return Results.Ok(repositoryConnection.GetConnection());
        });

        api.MapPost("/repos/connection", (
            RepositoryConnection connection,
            IRepositoryConnectionService repositoryConnection) =>
        {
            var updated = repositoryConnection.UpdateConnection(connection);
            return Results.Ok(updated);
        });

        api.MapGet("/settings", (ISettingsStore settingsStore) =>
        {
            return Results.Ok(settingsStore.GetSettings());
        });

        api.MapPost("/settings", (ToolEndpointSettings settings, ISettingsStore settingsStore) =>
        {
            var updated = settingsStore.UpdateSettings(settings);
            return Results.Ok(updated);
        });

        return api;
    }
}
