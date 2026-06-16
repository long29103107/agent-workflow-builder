using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemorySettingsStore : ISettingsStore
{
    private ToolEndpointSettings _settings = new(
        "mock://jira",
        "mock://notion",
        RepositoryPathDefaults.Resolve(),
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL") ?? string.Empty,
        "github");

    public ToolEndpointSettings GetSettings() => _settings;

    public ToolEndpointSettings UpdateSettings(ToolEndpointSettings settings)
    {
        _settings = settings with
        {
            JiraMcpEndpoint = string.IsNullOrWhiteSpace(settings.JiraMcpEndpoint)
                ? "mock://jira"
                : settings.JiraMcpEndpoint,
            NotionMcpEndpoint = string.IsNullOrWhiteSpace(settings.NotionMcpEndpoint)
                ? "mock://notion"
                : settings.NotionMcpEndpoint,
            RepositoryPath = string.IsNullOrWhiteSpace(settings.RepositoryPath)
                ? RepositoryPathDefaults.Resolve()
                : settings.RepositoryPath,
            RepositoryUrl = settings.RepositoryUrl?.Trim() ?? string.Empty,
            RepositoryProvider = string.IsNullOrWhiteSpace(settings.RepositoryProvider)
                ? "github"
                : settings.RepositoryProvider.Trim()
        };

        return _settings;
    }
}
