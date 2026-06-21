using AgentWorkflow.Core.Application;

namespace AgentWorkflow.Api.Endpoints;

public static class TaskApiEndpoints
{
    public static RouteGroupBuilder MapTaskApi(this RouteGroupBuilder api)
    {
        var tasks = api.MapGroup("/tasks").WithTags("Tasks");

        tasks.MapGet("", async (ITaskSource taskSource, CancellationToken cancellationToken) =>
        {
            var result = await taskSource.GetTasksAsync(cancellationToken);
            return Results.Ok(result);
        });

        return tasks;
    }
}
