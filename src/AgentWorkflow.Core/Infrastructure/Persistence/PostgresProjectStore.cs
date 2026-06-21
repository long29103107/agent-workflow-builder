using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using Microsoft.EntityFrameworkCore;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class PostgresProjectStore(
    IDbContextFactory<AgentWorkflowDbContext> contextFactory,
    WorkspaceDefaults defaults,
    IProjectPolicyValidator validator) : IProjectStore
{
    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entities = await context.Projects
            .AsNoTracking()
            .OrderBy(project => project.CreatedAt)
            .ToListAsync(cancellationToken);
        return entities.Select(ToDomain).ToList();
    }

    public async Task<Project?> GetProjectAsync(
        string projectId,
        CancellationToken cancellationToken)
    {
        await EnsureSeededAsync(cancellationToken);
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Projects
            .AsNoTracking()
            .SingleOrDefaultAsync(project => project.Id == projectId, cancellationToken);
        return entity is null ? null : ToDomain(entity);
    }

    public async Task<Project> CreateProjectAsync(
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        EnsureValid(validator.Validate(request));
        var project = CreateProject(Guid.NewGuid().ToString("N"), request);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await EnsureCodeAvailableAsync(context, project.Code, null, cancellationToken);
        context.Projects.Add(ToEntity(project));
        await context.SaveChangesAsync(cancellationToken);
        return project;
    }

    public async Task<Project?> UpdateProjectAsync(
        string projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        EnsureValid(validator.Validate(request));
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Projects.SingleOrDefaultAsync(
            project => project.Id == projectId,
            cancellationToken);
        if (entity is null)
        {
            return null;
        }

        var current = ToDomain(entity);
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? current.Code
            : ProjectCode.Normalize(request.Code, request.Name);
        await EnsureCodeAvailableAsync(context, code, projectId, cancellationToken);
        var updated = current with
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
        entity.PayloadJson = JsonSerializer.Serialize(updated, PersistenceJson.Options);
        entity.UpdatedAt = updated.UpdatedAt;
        await context.SaveChangesAsync(cancellationToken);
        return updated;
    }

    public async Task<bool> DeleteProjectAsync(
        string projectId,
        CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var entity = await context.Projects.SingleOrDefaultAsync(
            project => project.Id == projectId,
            cancellationToken);
        if (entity is null)
        {
            return false;
        }

        context.Projects.Remove(entity);
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task EnsureSeededAsync(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        if (await context.Projects.AnyAsync(
            project => project.Id == ProjectPolicyDefaults.DefaultProjectId,
            cancellationToken))
        {
            return;
        }

        var normalizedDefaults = defaults with
        {
            RepositoryPath = string.IsNullOrWhiteSpace(defaults.RepositoryPath)
                ? RepositoryPathDefaults.Resolve()
                : defaults.RepositoryPath.Trim()
        };
        var request = ProjectPolicyDefaults.Create(normalizedDefaults);
        EnsureValid(validator.Validate(request));
        context.Projects.Add(ToEntity(CreateProject(
            ProjectPolicyDefaults.DefaultProjectId,
            request)));

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Another request may have inserted the fixed seed concurrently.
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

    private static async Task EnsureCodeAvailableAsync(
        AgentWorkflowDbContext context,
        string code,
        string? excludedProjectId,
        CancellationToken cancellationToken)
    {
        var payloads = await context.Projects
            .AsNoTracking()
            .Where(project => project.Id != excludedProjectId)
            .Select(project => project.PayloadJson)
            .ToListAsync(cancellationToken);
        if (payloads
            .Select(payload => JsonSerializer.Deserialize<Project>(payload, PersistenceJson.Options))
            .Any(project => string.Equals(project?.Code, code, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Project code '{code}' is already in use.");
        }
    }

    private static ProjectEntity ToEntity(Project project) =>
        new()
        {
            Id = project.Id,
            PayloadJson = JsonSerializer.Serialize(project, PersistenceJson.Options),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };

    private static Project ToDomain(ProjectEntity entity) =>
        JsonSerializer.Deserialize<Project>(entity.PayloadJson, PersistenceJson.Options)
        ?? throw new InvalidOperationException($"Project '{entity.Id}' contains invalid persisted data.");

    private static void EnsureValid(IReadOnlyList<ProjectValidationError> errors)
    {
        if (errors.Count > 0)
        {
            throw new ProjectPolicyValidationException(errors);
        }
    }
}
