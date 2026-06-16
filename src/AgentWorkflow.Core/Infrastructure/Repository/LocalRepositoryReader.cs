using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class LocalRepositoryReader : IRepositoryReader
{
    public Task<RepositoryContext> GetContextAsync(RepositoryConnection connection, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(connection.Url))
        {
            var remoteFiles = new[]
            {
                "README.md",
                "AGENTS.md",
                "src/",
                ".github/workflows/"
            };

            var remoteTechnologies = new[] { "GitHub", "Mock Repository Workspace" };
            var remotePath = $"mock-git://{connection.Owner}/{connection.Name}";
            var remoteSummary = $"Mock GitHub repository workspace resolved for '{connection.Owner}/{connection.Name}' on branch '{connection.DefaultBranch}'. Clone and checkout are planned for the next workspace slice.";

            return Task.FromResult(new RepositoryContext(
                remotePath,
                connection.Name,
                connection,
                remoteFiles,
                remoteTechnologies,
                remoteSummary));
        }

        var path = string.IsNullOrWhiteSpace(connection.LocalPath)
            ? RepositoryPathDefaults.Resolve()
            : connection.LocalPath;

        var fullPath = Path.GetFullPath(path);
        var name = new DirectoryInfo(fullPath).Name;

        IReadOnlyList<string> files = Directory.Exists(fullPath)
            ? Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(fullPath, file))
                .Where(IsRelevantRepositoryFile)
                .OrderByDescending(IsHighSignalFile)
                .ThenBy(file => file)
                .Take(12)
                .ToList()
            : ["Repository path was not found; using mock file inventory."];

        var technologies = new List<string>();
        if (files.Any(file => file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase))) technologies.Add(".NET");
        if (files.Any(file => file.EndsWith("package.json", StringComparison.OrdinalIgnoreCase))) technologies.Add("React");
        if (files.Any(file => file.EndsWith("docker-compose.yml", StringComparison.OrdinalIgnoreCase))) technologies.Add("Docker Compose");
        if (technologies.Count == 0) technologies.AddRange(["Mock Repository", "Future Git Provider"]);

        var summary = Directory.Exists(fullPath)
            ? $"Local repository '{name}' inspected; {files.Count} representative files captured."
            : "Mock repository context generated because the configured path does not exist.";

        var localConnection = connection with
        {
            LocalPath = fullPath,
            Name = string.IsNullOrWhiteSpace(connection.Name) ? name : connection.Name
        };

        return Task.FromResult(new RepositoryContext(fullPath, name, localConnection, files, technologies, summary));
    }

    private static bool IsRelevantRepositoryFile(string relativePath)
    {
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        string[] ignoredSegments = [".git", ".npm-cache", "bin", "obj", "node_modules", "dist", "run-logs"];
        return !segments.Any(segment => ignoredSegments.Contains(segment, StringComparer.OrdinalIgnoreCase));
    }

    private static bool IsHighSignalFile(string relativePath)
    {
        string[] endings =
        [
            ".csproj",
            ".sln",
            ".slnx",
            "package.json",
            "docker-compose.yml",
            "README.md",
            "AGENTS.md",
            "REQUEST.md"
        ];

        return endings.Any(ending => relativePath.EndsWith(ending, StringComparison.OrdinalIgnoreCase));
    }
}
