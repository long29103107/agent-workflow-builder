using AgentWorkflow.Core.Infrastructure;
using System.Text.Json.Serialization;

namespace AgentWorkflow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowApi(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://127.0.0.1:5173"));
        });

        services.AddAgentWorkflowCore();

        return services;
    }
}
