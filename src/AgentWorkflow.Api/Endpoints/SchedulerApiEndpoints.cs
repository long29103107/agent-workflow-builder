using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class SchedulerApiEndpoints
{
    public static RouteGroupBuilder MapSchedulerApi(this RouteGroupBuilder api)
    {
        var scheduler = api.MapGroup("/scheduler").WithTags("Scheduler");

        scheduler.MapGet("/tasks", (ITaskScheduler taskScheduler) =>
            Results.Ok(taskScheduler.GetScheduledTasks()));

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

        return scheduler;
    }
}
