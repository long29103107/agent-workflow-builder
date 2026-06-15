namespace AgentWorkflow.Core.Infrastructure;

internal static class RepositoryPathDefaults
{
    public static string Resolve()
    {
        var configuredPath = Environment.GetEnvironmentVariable("AGENT_WORKFLOW_REPOSITORY_PATH");
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            return Path.GetFullPath(configuredPath);
        }

        var current = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "REQUEST.md")) ||
                File.Exists(Path.Combine(current.FullName, "docker-compose.yml")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        return Directory.GetCurrentDirectory();
    }
}
