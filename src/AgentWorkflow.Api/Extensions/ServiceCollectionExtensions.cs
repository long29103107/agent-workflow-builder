using AgentWorkflow.Core.Infrastructure;
using AgentWorkflow.Core.Domain;
using System.Text.Json.Serialization;

namespace AgentWorkflow.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        var workspaceDefaults = configuration.GetSection("WorkspaceDefaults").Get<WorkspaceDefaults>()
            ?? new WorkspaceDefaults("Project Alpha", string.Empty, string.Empty, "github");
        var toolEndpoints = new ToolEndpointSettings(
            configuration["ToolEndpoints:JiraMcpEndpoint"] ?? "mock://jira",
            configuration["ToolEndpoints:NotionMcpEndpoint"] ?? "mock://notion",
            workspaceDefaults.RepositoryPath,
            workspaceDefaults.RepositoryUrl,
            workspaceDefaults.RepositoryProvider);

        services.AddSingleton(workspaceDefaults);
        services.AddSingleton(toolEndpoints);
        services.AddProblemDetails();
        services.AddOpenApi();
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(allowedOrigins.Length > 0
                        ? allowedOrigins
                        : ["http://localhost:5173", "http://127.0.0.1:5173"]));
        });

        var persistenceProvider = configuration["Persistence:Provider"];
        var postgresConnectionString = string.Equals(
            persistenceProvider,
            "PostgreSql",
            StringComparison.OrdinalIgnoreCase)
            ? configuration.GetConnectionString("AgentWorkflowDb")
                ?? throw new InvalidOperationException(
                    "ConnectionStrings:AgentWorkflowDb is required when PostgreSQL persistence is enabled.")
            : null;

        services.AddAgentWorkflowCore(postgresConnectionString);

        return services;
    }
}
