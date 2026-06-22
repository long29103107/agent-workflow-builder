using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class TaskApiEndpoints
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    public static RouteGroupBuilder MapTaskApi(this RouteGroupBuilder api)
    {
        var tasks = api.MapGroup("/tasks").WithTags("Tasks");

        tasks.MapGet("", async (ITaskSource taskSource, CancellationToken cancellationToken) =>
        {
            var result = await taskSource.GetTasksAsync(cancellationToken);
            return Results.Ok(result);
        });

        tasks.MapGet("/{taskId}/history", async (
            string taskId,
            long? afterSequence,
            int? limit,
            ITaskActivityStore activityStore,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var items = await activityStore.GetAfterAsync(
                    taskId,
                    afterSequence ?? 0,
                    limit ?? 100,
                    cancellationToken);
                return Results.Ok(new TaskActivityHistory(
                    items,
                    items.Count == 0 ? afterSequence ?? 0 : items[^1].Sequence));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        tasks.MapGet("/{taskId}/activity", StreamActivityAsync);

        return tasks;
    }

    private static async Task StreamActivityAsync(
        string taskId,
        long? afterSequence,
        int? replayLimit,
        HttpContext context,
        ITaskActivityStore activityStore,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var cursor = ParseLastEventId(context.Request.Headers["Last-Event-ID"])
            ?? afterSequence
            ?? 0;
        var limit = replayLimit ?? 100;
        if (cursor < 0 || limit is < 1 or > 500)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(
                new { error = "Activity cursor must be non-negative and replayLimit must be between 1 and 500." },
                cancellationToken);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status200OK;
        context.Response.ContentType = "text/event-stream";
        context.Response.Headers.CacheControl = "no-cache";
        context.Response.Headers.Connection = "keep-alive";
        context.Response.Headers["X-Accel-Buffering"] = "no";
        await context.Response.StartAsync(cancellationToken);

        var lastWrite = timeProvider.GetUtcNow();
        while (!cancellationToken.IsCancellationRequested)
        {
            var items = await activityStore.GetAfterAsync(
                taskId,
                cursor,
                limit,
                cancellationToken);
            if (items.Count > 0)
            {
                foreach (var item in items)
                {
                    await context.Response.WriteAsync(
                        $"id: {item.Sequence}\n" +
                        $"event: {item.Category.ToString().ToLowerInvariant()}\n" +
                        $"data: {JsonSerializer.Serialize(item, JsonOptions)}\n\n",
                        cancellationToken);
                    cursor = item.Sequence;
                }

                await context.Response.Body.FlushAsync(cancellationToken);
                lastWrite = timeProvider.GetUtcNow();
                if (items.Count == limit)
                {
                    return;
                }

                continue;
            }

            if (timeProvider.GetUtcNow() - lastWrite >= TimeSpan.FromSeconds(15))
            {
                await context.Response.WriteAsync(": heartbeat\n\n", cancellationToken);
                await context.Response.Body.FlushAsync(cancellationToken);
                lastWrite = timeProvider.GetUtcNow();
            }

            await Task.Delay(TimeSpan.FromMilliseconds(250), timeProvider, cancellationToken);
        }
    }

    private static long? ParseLastEventId(string? value) =>
        long.TryParse(value, out var parsed) && parsed >= 0 ? parsed : null;

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
