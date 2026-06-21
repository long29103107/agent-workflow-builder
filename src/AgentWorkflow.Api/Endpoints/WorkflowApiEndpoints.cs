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
