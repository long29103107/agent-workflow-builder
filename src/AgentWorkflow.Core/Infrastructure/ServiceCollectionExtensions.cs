using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AgentWorkflow.Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowCore(this IServiceCollection services)
    {
        services.TryAddSingleton(new WorkspaceDefaults(
            "Project Alpha",
            RepositoryPathDefaults.Resolve(),
            Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL") ?? string.Empty,
            "github"));
        services.TryAddSingleton(new ToolEndpointSettings(
            "mock://jira",
            "mock://notion",
            RepositoryPathDefaults.Resolve(),
            Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL") ?? string.Empty,
            "github"));
        services.AddSingleton<IWorkflowRunStore, InMemoryWorkflowRunStore>();
        services.AddSingleton<MockJiraMcpTool>();
        services.AddSingleton<IJiraMcpTool>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<ITaskSource>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<INotionContextTool, MockNotionContextTool>();
        services.AddSingleton<IRepositoryConnectionService, MockRepositoryConnectionService>();
        services.AddSingleton<IRepositoryReader, LocalRepositoryReader>();
        services.AddSingleton<IMemoryService, MockMemoryService>();
        services.AddSingleton<ISettingsStore, InMemorySettingsStore>();
        services.AddSingleton<IProjectPolicyValidator, ProjectPolicyValidator>();
        services.AddSingleton<IProjectStore, InMemoryProjectStore>();
        services.AddSingleton<InMemoryWorkspaceStore>();
        services.AddSingleton<IWorkspaceStore>(provider => provider.GetRequiredService<InMemoryWorkspaceStore>());
        services.AddSingleton<IWorkspaceSettingsStore>(provider => provider.GetRequiredService<InMemoryWorkspaceStore>());
        services.AddSingleton<IRequestIntakeStore, InMemoryRequestIntakeStore>();
        services.AddSingleton<IPlannerLogStore, InMemoryPlannerLogStore>();
        services.AddSingleton<IWorkspaceTaskSource, WorkspaceTaskSource>();
        services.AddSingleton<IAgentReasoningService, OpenAiAgentReasoningService>();
        services.AddSingleton<ISubagent, RepositoryInvestigatorAgent>();
        services.AddSingleton<ISubagent, JiraNotionContextAgent>();
        services.AddSingleton<ISubagent, MemoryResearchAgent>();
        services.AddSingleton<ISubagent, PlanningAgent>();
        services.AddSingleton<ILeadAgent, OpenAiLeadAgent>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
        services.AddSingleton<ITaskScheduler, InMemoryTaskScheduler>();

        return services;
    }
}
