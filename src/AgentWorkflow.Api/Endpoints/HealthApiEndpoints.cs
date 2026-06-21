namespace AgentWorkflow.Api.Endpoints;

public static class HealthApiEndpoints
{
    public static RouteGroupBuilder MapHealthApi(this RouteGroupBuilder api)
    {
        var health = api.MapGroup("/health").WithTags("Health");

        health.MapGet("", () => Results.Ok(new { status = "ok" }));

        return health;
    }
}
