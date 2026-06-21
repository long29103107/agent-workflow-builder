using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class InMemoryProjectStore : IProjectStore
{
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Project> _projects = new(StringComparer.OrdinalIgnoreCase);
    private readonly IProjectPolicyValidator _validator;

    public InMemoryProjectStore()
        : this(
            new WorkspaceDefaults(
                "Project Alpha",
                RepositoryPathDefaults.Resolve(),
                Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL") ?? string.Empty,
                "github"),
            new ProjectPolicyValidator())
    {
    }

    public InMemoryProjectStore(
        WorkspaceDefaults defaults,
        IProjectPolicyValidator validator)
    {
        _validator = validator;
        var normalizedDefaults = defaults with
        {
            RepositoryPath = string.IsNullOrWhiteSpace(defaults.RepositoryPath)
                ? RepositoryPathDefaults.Resolve()
                : defaults.RepositoryPath.Trim()
        };
        var request = ProjectPolicyDefaults.Create(normalizedDefaults);
        EnsureValid(_validator.Validate(request));
        var project = CreateProject(ProjectPolicyDefaults.DefaultProjectId, request);
        _projects[project.Id] = project;
    }

    public Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Project>>(
                _projects.Values.OrderBy(project => project.CreatedAt).ToList());
        }
    }

    public Task<Project?> GetProjectAsync(
        string projectId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_projects.GetValueOrDefault(projectId));
        }
    }

    public Task<Project> CreateProjectAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureValid(_validator.Validate(request));
        var project = CreateProject(Guid.NewGuid().ToString("N"), request);

        lock (_sync)
        {
            EnsureCodeAvailable(project.Code);
            _projects[project.Id] = project;
        }

        return Task.FromResult(project);
    }

    public Task<Project?> UpdateProjectAsync(
        string projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureValid(_validator.Validate(request));

        lock (_sync)
        {
            if (!_projects.TryGetValue(projectId, out var project))
            {
                return Task.FromResult<Project?>(null);
            }

            var code = string.IsNullOrWhiteSpace(request.Code)
                ? project.Code
                : ProjectCode.Normalize(request.Code, request.Name);
            EnsureCodeAvailable(code, projectId);
            var updated = project with
            {
                Name = request.Name.Trim(),
                Code = code,
                Repository = request.Repository,
                GitHub = request.GitHub,
                Agents = request.Agents,
                CodingStandards = request.CodingStandards,
                Commands = request.Commands,
                BranchPolicy = request.BranchPolicy,
                ProtectedPaths = request.ProtectedPaths,
                ApprovalPolicy = request.ApprovalPolicy,
                UpdatedAt = DateTimeOffset.UtcNow
            };
            _projects[projectId] = updated;
            return Task.FromResult<Project?>(updated);
        }
    }

    public Task<bool> DeleteProjectAsync(
        string projectId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_sync)
        {
            return Task.FromResult(_projects.Remove(projectId));
        }
    }

    private static Project CreateProject(string projectId, CreateProjectRequest request)
    {
        var now = DateTimeOffset.UtcNow;
        return new Project(
            projectId,
            request.Name.Trim(),
            request.Repository,
            request.GitHub,
            request.Agents,
            request.CodingStandards,
            request.Commands,
            request.BranchPolicy,
            request.ProtectedPaths,
            request.ApprovalPolicy,
            now,
            now,
            ProjectCode.Normalize(request.Code, request.Name));
    }

    private void EnsureCodeAvailable(string code, string? excludedProjectId = null)
    {
        if (_projects.Values.Any(project =>
            !string.Equals(project.Id, excludedProjectId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(project.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Project code '{code}' is already in use.");
        }
    }

    private static void EnsureValid(IReadOnlyList<ProjectValidationError> errors)
    {
        if (errors.Count > 0)
        {
            throw new ProjectPolicyValidationException(errors);
        }
    }
}
