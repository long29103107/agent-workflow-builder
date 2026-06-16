using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class MockRepositoryConnectionService : IRepositoryConnectionService
{
    private RepositoryConnection _connection = CreateConnection(
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_PATH"),
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_URL"));

    public RepositoryConnection GetConnection() => _connection;

    public RepositoryConnection UpdateConnection(RepositoryConnection connection)
    {
        _connection = CreateConnection(connection.LocalPath, connection.Url);
        return _connection;
    }

    public RepositoryConnection ResolveConnection(string? repositoryPath, string? repositoryUrl)
    {
        if (!string.IsNullOrWhiteSpace(repositoryPath) || !string.IsNullOrWhiteSpace(repositoryUrl))
        {
            return CreateConnection(repositoryPath, repositoryUrl);
        }

        return _connection;
    }

    private static RepositoryConnection CreateConnection(string? repositoryPath, string? repositoryUrl)
    {
        var path = string.IsNullOrWhiteSpace(repositoryPath)
            ? RepositoryPathDefaults.Resolve()
            : Path.GetFullPath(repositoryPath);

        if (!string.IsNullOrWhiteSpace(repositoryUrl))
        {
            var (owner, name) = ParseGitHubRepository(repositoryUrl);
            return new RepositoryConnection(
                "github",
                repositoryUrl.Trim(),
                path,
                owner,
                name,
                "main",
                "MockConnected",
                "GitHub repository target resolved through a mock provider boundary.");
        }

        var directoryName = new DirectoryInfo(path).Name;
        return new RepositoryConnection(
            "local",
            null,
            path,
            "local",
            directoryName,
            "current",
            Directory.Exists(path) ? "Connected" : "Missing",
            Directory.Exists(path)
                ? "Local repository target resolved."
                : "Local repository path is missing; mock context will be used.");
    }

    private static (string Owner, string Name) ParseGitHubRepository(string repositoryUrl)
    {
        if (Uri.TryCreate(repositoryUrl.Trim(), UriKind.Absolute, out var uri))
        {
            var segments = uri.AbsolutePath
                .Trim('/')
                .Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
            {
                return (segments[0], NormalizeRepositoryName(segments[1]));
            }
        }

        var shorthand = repositoryUrl.Trim().TrimEnd('/');
        var shorthandSegments = shorthand.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (shorthandSegments.Length >= 2)
        {
            return (shorthandSegments[^2], NormalizeRepositoryName(shorthandSegments[^1]));
        }

        return ("unknown", NormalizeRepositoryName(shorthand));
    }

    private static string NormalizeRepositoryName(string value)
    {
        var name = value.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? value[..^4]
            : value;

        return string.IsNullOrWhiteSpace(name) ? "repository" : name;
    }
}
