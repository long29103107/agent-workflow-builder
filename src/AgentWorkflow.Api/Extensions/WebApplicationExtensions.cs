using Scalar.AspNetCore;

namespace AgentWorkflow.Api.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MapAgentWorkflowApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi("/swagger/{documentName}/swagger.json");
        app.MapScalarApiReference(options =>
        {
            options.Title = "Agent Workflow API";
            options.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json";
        });

        return app;
    }
}
