using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class ProjectApiEndpoints
{
    public static RouteGroupBuilder MapProjectApi(this RouteGroupBuilder api)
    {
        var projects = api.MapGroup("/projects").WithTags("Projects");

        projects.MapGet("", async (
            IProjectStore store,
            CancellationToken cancellationToken) =>
            Results.Ok(await store.GetProjectsAsync(cancellationToken)));

        projects.MapGet("/{projectId}", async (
            string projectId,
            IProjectStore store,
            CancellationToken cancellationToken) =>
        {
            var project = await store.GetProjectAsync(projectId, cancellationToken);
            return project is null ? Results.NotFound() : Results.Ok(project);
        });

        projects.MapPost("", async (
            CreateProjectRequest request,
            IProjectStore store,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await store.CreateProjectAsync(request, cancellationToken);
                return Results.Created($"/api/projects/{project.Id}", project);
            }
            catch (ProjectPolicyValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        projects.MapPut("/{projectId}", async (
            string projectId,
            UpdateProjectRequest request,
            IProjectStore store,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var project = await store.UpdateProjectAsync(projectId, request, cancellationToken);
                return project is null ? Results.NotFound() : Results.Ok(project);
            }
            catch (ProjectPolicyValidationException ex)
            {
                return Results.BadRequest(new { errors = ex.Errors });
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });

        projects.MapDelete("/{projectId}", async (
            string projectId,
            IProjectStore store,
            CancellationToken cancellationToken) =>
        {
            if (string.Equals(
                projectId,
                ProjectPolicyDefaults.DefaultProjectId,
                StringComparison.OrdinalIgnoreCase))
            {
                return Results.Conflict(new { error = "The default project cannot be deleted." });
            }

            return await store.DeleteProjectAsync(projectId, cancellationToken)
                ? Results.NoContent()
                : Results.NotFound();
        });

        return projects;
    }
}
