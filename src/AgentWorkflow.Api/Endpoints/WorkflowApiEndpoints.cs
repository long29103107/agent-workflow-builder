using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class WorkflowApiEndpoints
{
    public static RouteGroupBuilder MapWorkflowApi(this RouteGroupBuilder api)
    {
        var workflows = api.MapGroup("/workflows").WithTags("Workflows");

        workflows.MapPost("/investigate", async (
            InvestigationRequest request,
            ITaskScheduler taskScheduler,
            CancellationToken cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(request.TaskId))
            {
                return Results.BadRequest(new { error = "taskId is required." });
            }

            try
            {
                var scheduledTask = await taskScheduler.EnqueueAsync(
                    new ScheduleTaskRequest(
                        request.TaskId,
                        Priority: null,
                        request.RepositoryPath,
                        request.RepositoryUrl,
                        request.WorkspaceId,
                        AssignedAgent: null,
                        request.RequestedAgents),
                    cancellationToken);
                return Results.Accepted(
                    $"/api/scheduler/tasks/{scheduledTask.Id}",
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

        workflows.MapGet("/{runId:guid}", (Guid runId, IWorkflowRunStore store) =>
        {
            var run = store.GetRun(runId);
            return run is null ? Results.NotFound() : Results.Ok(run);
        });

        workflows.MapGet("/{runId:guid}/events", (Guid runId, IWorkflowRunStore store) =>
        {
            if (store.GetRun(runId) is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(store.GetEvents(runId));
        });

        return workflows;
    }
}
