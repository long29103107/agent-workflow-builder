using AgentWorkflow.Cli.Application;
using AgentWorkflow.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AgentWorkflow.Cli.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowCli(this IServiceCollection services)
    {
        services.AddAgentWorkflowCore();
        services.AddTransient<CliRunner>();

        return services;
    }
}
