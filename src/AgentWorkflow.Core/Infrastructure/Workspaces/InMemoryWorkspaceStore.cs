using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryWorkspaceStore : IWorkspaceStore, IWorkspaceSettingsStore
{
    public const string DefaultWorkspaceId = ProjectPolicyDefaults.DefaultProjectId;

    private readonly Lock _sync = new();
    private readonly Dictionary<string, ToolEndpointSettings> _settings = new(StringComparer.OrdinalIgnoreCase);
    private readonly IProjectStore _projects;
    private readonly ToolEndpointSettings _toolDefaults;

    public InMemoryWorkspaceStore()
        : this(new InMemoryProjectStore(), CreateToolDefaults())
    {
    }

    public InMemoryWorkspaceStore(
        IProjectStore projects,
        ToolEndpointSettings toolDefaults)
    {
        _projects = projects;
        _toolDefaults = toolDefaults;
    }

    public async Task<IReadOnlyList<WorkspaceProject>> GetWorkspacesAsync(
        CancellationToken cancellationToken)
    {
        var projects = await _projects.GetProjectsAsync(cancellationToken);
        return projects.Select(ToWorkspace).ToList();
    }

    public async Task<WorkspaceProject?> GetWorkspaceAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetProjectAsync(workspaceId, cancellationToken);
        return project is null ? null : ToWorkspace(project);
    }

    public async Task<WorkspaceProject> CreateWorkspaceAsync(
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var defaults = ProjectPolicyDefaults.Create(new WorkspaceDefaults(
            request.Name,
            ResolvePath(request.RepositoryPath, request.RepositoryUrl),
            request.RepositoryUrl?.Trim() ?? string.Empty,
            NormalizeProvider(request.RepositoryProvider),
            request.Code));
        var project = await _projects.CreateProjectAsync(defaults, cancellationToken);
        return ToWorkspace(project);
    }

    public async Task<WorkspaceProject?> UpdateWorkspaceAsync(
        string workspaceId,
        UpdateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetProjectAsync(workspaceId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var compatibilityDefaults = ProjectPolicyDefaults.Create(new WorkspaceDefaults(
            request.Name,
            ResolvePath(request.RepositoryPath, request.RepositoryUrl),
            request.RepositoryUrl?.Trim() ?? string.Empty,
            NormalizeProvider(request.RepositoryProvider),
            request.Code ?? project.Code));
        var updated = await _projects.UpdateProjectAsync(
            workspaceId,
            ToUpdateRequest(
                project,
                request.Name,
                compatibilityDefaults.Repository,
                compatibilityDefaults.GitHub),
            cancellationToken);

        if (updated is null)
        {
            return null;
        }

        UpdateCachedRepositorySettings(updated);
        return ToWorkspace(updated);
    }

    public async Task<ToolEndpointSettings?> GetSettingsAsync(
        string workspaceId,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetProjectAsync(workspaceId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        lock (_sync)
        {
            if (_settings.TryGetValue(workspaceId, out var settings))
            {
                return settings;
            }

            settings = CreateSettings(project);
            _settings[workspaceId] = settings;
            return settings;
        }
    }

    public async Task<ToolEndpointSettings?> UpdateSettingsAsync(
        string workspaceId,
        ToolEndpointSettings settings,
        CancellationToken cancellationToken)
    {
        var project = await _projects.GetProjectAsync(workspaceId, cancellationToken);
        if (project is null)
        {
            return null;
        }

        var compatibilityDefaults = ProjectPolicyDefaults.Create(new WorkspaceDefaults(
            project.Name,
            ResolvePath(settings.RepositoryPath, settings.RepositoryUrl),
            settings.RepositoryUrl?.Trim() ?? string.Empty,
            NormalizeProvider(settings.RepositoryProvider),
            project.Code));
        var updatedProject = await _projects.UpdateProjectAsync(
            workspaceId,
            ToUpdateRequest(
                project,
                project.Name,
                compatibilityDefaults.Repository,
                compatibilityDefaults.GitHub),
            cancellationToken);

        if (updatedProject is null)
        {
            return null;
        }

        var updatedSettings = settings with
        {
            JiraMcpEndpoint = NormalizeValue(settings.JiraMcpEndpoint, _toolDefaults.JiraMcpEndpoint),
            NotionMcpEndpoint = NormalizeValue(settings.NotionMcpEndpoint, _toolDefaults.NotionMcpEndpoint),
            RepositoryPath = updatedProject.Repository.LocalPath,
            RepositoryUrl = updatedProject.Repository.Url,
            RepositoryProvider = updatedProject.Repository.Provider
        };

        lock (_sync)
        {
            _settings[workspaceId] = updatedSettings;
        }

        return updatedSettings;
    }

    private void UpdateCachedRepositorySettings(Project project)
    {
        lock (_sync)
        {
            if (!_settings.TryGetValue(project.Id, out var current))
            {
                return;
            }

            _settings[project.Id] = current with
            {
                RepositoryPath = project.Repository.LocalPath,
                RepositoryUrl = project.Repository.Url,
                RepositoryProvider = project.Repository.Provider
            };
        }
    }

    private ToolEndpointSettings CreateSettings(Project project) =>
        _toolDefaults with
        {
            RepositoryPath = project.Repository.LocalPath,
            RepositoryUrl = project.Repository.Url,
            RepositoryProvider = project.Repository.Provider
        };

    private static UpdateProjectRequest ToUpdateRequest(
        Project project,
        string name,
        ProjectRepositorySettings repository,
        ProjectGitHubSettings github) =>
        new(
            name,
            repository with { DefaultBranch = project.Repository.DefaultBranch },
            github with { InstallationId = project.GitHub.InstallationId },
            project.Agents,
            project.CodingStandards,
            project.Commands,
            project.BranchPolicy,
            project.ProtectedPaths,
            project.ApprovalPolicy,
            project.Code);

    private static WorkspaceProject ToWorkspace(Project project) =>
        new(
            project.Id,
            project.Name,
            project.Repository.LocalPath,
            project.Repository.Url,
            project.Repository.Provider,
            project.CreatedAt,
            project.UpdatedAt,
            project.Code);

    private static string ResolvePath(string? repositoryPath, string? repositoryUrl) =>
        string.IsNullOrWhiteSpace(repositoryPath) && string.IsNullOrWhiteSpace(repositoryUrl)
            ? RepositoryPathDefaults.Resolve()
            : repositoryPath?.Trim() ?? string.Empty;

    private static string NormalizeProvider(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "github" : value.Trim();

    private static string NormalizeValue(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

    private static ToolEndpointSettings CreateToolDefaults() =>
        new(
            "mock://jira",
            "mock://notion",
            RepositoryPathDefaults.Resolve(),
            Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL") ?? string.Empty,
            "github");
}
