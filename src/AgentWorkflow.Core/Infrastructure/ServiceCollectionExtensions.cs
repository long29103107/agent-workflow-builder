using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AgentWorkflow.Core.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentWorkflowCore(
        this IServiceCollection services,
        string? postgresConnectionString = null)
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
        services.AddSingleton<IProjectPolicyValidator, ProjectPolicyValidator>();
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton<ISecretRedactor, SecretRedactor>();
        services.AddAgentWorkflowPersistence(postgresConnectionString);
        services.AddSingleton<IApprovalPolicyEngine, ApprovalPolicyEngine>();
        services.AddSingleton<EngineeringTaskSource>();
        services.AddSingleton<MockJiraMcpTool>();
        services.AddSingleton<IJiraMcpTool>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<ITaskSource>(provider => provider.GetRequiredService<MockJiraMcpTool>());
        services.AddSingleton<INotionContextTool, MockNotionContextTool>();
        services.AddSingleton<IRepositoryConnectionService, MockRepositoryConnectionService>();
        services.AddSingleton<IRepositoryReader, LocalRepositoryReader>();
        services.AddSingleton<IGitHubRepositoryAuthenticator, GitHubRepositoryAuthenticator>();
        services.AddSingleton<IRepositoryWorkspaceService, RepositoryWorkspaceService>();
        services.AddSingleton<MockExecutionSandboxProvider>();
        services.AddSingleton<DockerSandboxOptions>();
        services.AddSingleton<IDockerCliRunner, DockerCliRunner>();
        services.AddSingleton<LocalDockerExecutionSandboxProvider>();
        services.AddSingleton<IExecutionSandboxProvider>(provider =>
            string.Equals(
                Environment.GetEnvironmentVariable("AGENT_WORKFLOW_SANDBOX_PROVIDER"),
                "docker",
                StringComparison.OrdinalIgnoreCase)
                ? provider.GetRequiredService<LocalDockerExecutionSandboxProvider>()
                : provider.GetRequiredService<MockExecutionSandboxProvider>());
        services.AddSingleton<IMemoryService, MockMemoryService>();
        services.AddSingleton<ISettingsStore, InMemorySettingsStore>();
        services.AddSingleton<InMemoryWorkspaceStore>();
        services.AddSingleton<IWorkspaceStore>(provider => provider.GetRequiredService<InMemoryWorkspaceStore>());
        services.AddSingleton<IWorkspaceSettingsStore>(provider => provider.GetRequiredService<InMemoryWorkspaceStore>());
        services.AddSingleton<IRequestIntakeStore, InMemoryRequestIntakeStore>();
        services.AddSingleton<IPlannerLogStore, InMemoryPlannerLogStore>();
        services.AddSingleton<ITaskAssignmentStore, InMemoryTaskAssignmentStore>();
        services.AddSingleton<IWorkspaceTaskSource, WorkspaceTaskSource>();
        services.AddSingleton<IAgentReasoningService, OpenAiAgentReasoningService>();
        services.AddSingleton<ISubagent, RepositoryInvestigatorAgent>();
        services.AddSingleton<ISubagent, JiraNotionContextAgent>();
        services.AddSingleton<ISubagent, ArchitectureAgent>();
        services.AddSingleton<ISubagent, MemoryResearchAgent>();
        services.AddSingleton<ISubagent, PlanningAgent>();
        services.AddSingleton<ILeadAgent, OpenAiLeadAgent>();
        services.AddSingleton<IWorkflowEngine, WorkflowEngine>();
        services.AddSingleton<ITaskScheduler, InMemoryTaskScheduler>();

        return services;
    }

    private static void AddAgentWorkflowPersistence(
        this IServiceCollection services,
        string? postgresConnectionString)
    {
        if (string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            services.AddSingleton<IWorkflowRunStore, InMemoryWorkflowRunStore>();
            services.AddSingleton<IWorkflowEvidenceStore, InMemoryWorkflowEvidenceStore>();
            services.AddSingleton<IApprovalStore, InMemoryApprovalStore>();
            services.AddSingleton<ITaskActivityStore, InMemoryTaskActivityStore>();
            services.AddSingleton<IProjectStore, InMemoryProjectStore>();
            services.AddSingleton<InMemoryEngineeringTaskStore>();
            services.AddSingleton<IEngineeringTaskStore>(provider =>
                provider.GetRequiredService<InMemoryEngineeringTaskStore>());
            services.AddSingleton<IWorkItemStore>(provider =>
                provider.GetRequiredService<InMemoryEngineeringTaskStore>());
            return;
        }

        services.AddPooledDbContextFactory<AgentWorkflowDbContext>(options =>
            options.UseNpgsql(postgresConnectionString));
        services.AddSingleton<IWorkflowRunStore, PostgresWorkflowRunStore>();
        services.AddSingleton<IWorkflowEvidenceStore, PostgresWorkflowEvidenceStore>();
        services.AddSingleton<IApprovalStore, PostgresApprovalStore>();
        services.AddSingleton<ITaskActivityStore, PostgresTaskActivityStore>();
        services.AddSingleton<IProjectStore, PostgresProjectStore>();
        services.AddSingleton<PostgresEngineeringTaskStore>();
        services.AddSingleton<IEngineeringTaskStore>(provider =>
            provider.GetRequiredService<PostgresEngineeringTaskStore>());
        services.AddSingleton<IWorkItemStore>(provider =>
            provider.GetRequiredService<PostgresEngineeringTaskStore>());
    }
}
