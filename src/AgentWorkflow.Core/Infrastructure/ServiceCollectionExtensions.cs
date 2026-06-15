using AgentWorkflow.Core.Application;
using Microsoft.Extensions.DependencyInjection;

namespace AgentWorkflow.Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowCore(this IServiceCollection services)
    {
        services.AddSingleton<IWorkflowRunStore, InMemoryWorkflowRunStore>();
        services.AddSingleton<MockJiraMcpTool>();
        services.AddSingleton<IJiraMcpTool>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<ITaskSource>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<INotionContextTool, MockNotionContextTool>();
        services.AddSingleton<IRepositoryReader, LocalRepositoryReader>();
        services.AddSingleton<IMemoryService, MockMemoryService>();
        services.AddSingleton<ISettingsStore, InMemorySettingsStore>();
        services.AddSingleton<IAgentReasoningService, OpenAiAgentReasoningService>();
        services.AddSingleton<ISubagent, RepositoryInvestigatorAgent>();
        services.AddSingleton<ISubagent, JiraNotionContextAgent>();
        services.AddSingleton<ISubagent, MemoryResearchAgent>();
        services.AddSingleton<ISubagent, PlanningAgent>();
        services.AddSingleton<ILeadAgent, OpenAiLeadAgent>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();

        return services;
    }
}
