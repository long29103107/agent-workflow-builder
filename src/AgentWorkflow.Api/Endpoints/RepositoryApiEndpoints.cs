using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class RepositoryApiEndpoints
{
    public static RouteGroupBuilder MapRepositoryApi(this RouteGroupBuilder api)
    {
        var repositories = api.MapGroup("/repos").WithTags("Repositories");

        repositories.MapGet("/context", async (
            string? path,
            string? url,
            IRepositoryConnectionService repositoryConnection,
            IRepositoryReader repositoryReader,
            CancellationToken cancellationToken) =>
        {
            var connection = repositoryConnection.ResolveConnection(path, url);
            var context = await repositoryReader.GetContextAsync(connection, cancellationToken);
            return Results.Ok(context);
        });

        repositories.MapGet("/connection", (IRepositoryConnectionService repositoryConnection) =>
            Results.Ok(repositoryConnection.GetConnection()));

        repositories.MapPost("/connection", (
            RepositoryConnection connection,
            IRepositoryConnectionService repositoryConnection) =>
        {
            var updated = repositoryConnection.UpdateConnection(connection);
            return Results.Ok(updated);
        });

        return repositories;
    }
}
