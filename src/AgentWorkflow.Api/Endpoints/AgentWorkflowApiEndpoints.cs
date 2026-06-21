namespace AgentWorkflow.Api.Endpoints;

public static class AgentWorkflowApiEndpoints
{
    public static RouteGroupBuilder MapAgentWorkflowApi(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api");

        api.MapHealthApi();
        api.MapTaskApi();
        api.MapSchedulerApi();
        api.MapWorkflowApi();
        api.MapMemoryApi();
        api.MapRepositoryApi();
        api.MapSettingsApi();
        api.MapProjectApi();
        api.MapProjectTaskApi();
        api.MapWorkspaceApi();

        return api;
    }
}
