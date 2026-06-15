using AgentWorkflow.Core.Infrastructure;
using AgentWorkflow.Mcp.Protocol;
using Microsoft.Extensions.DependencyInjection;

namespace AgentWorkflow.Mcp.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowMcp(this IServiceCollection services)
    {
        services.AddAgentWorkflowCore();
        services.AddTransient<McpStdioServer>();

        return services;
    }
}
