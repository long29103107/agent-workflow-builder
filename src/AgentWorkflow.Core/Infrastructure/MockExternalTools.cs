using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class MockJiraMcpTool : IJiraMcpTool
{
    public string EndpointName => "mock://jira";

    private static readonly IReadOnlyList<TaskItem> Tasks =
    [
        new(
            "jira-awb-101",
            "Jira",
            "AWB-101",
            "Investigate repository onboarding workflow",
            "Create a first-pass investigation plan for connecting local repositories to the agent workflow builder.",
            "Ready",
            "High",
            ["repo", "workflow", "mvp"]),
        new(
            "jira-awb-118",
            "Jira",
            "AWB-118",
            "Design memory graph linking",
            "Map how task, repository, entity, and context nodes should connect before the Neo4j implementation.",
            "Backlog",
            "Medium",
            ["memory", "neo4j"]),
        new(
            "jira-awb-124",
            "Jira",
            "AWB-124",
            "Prepare Notion MCP discovery",
            "Define the Notion pages and database fields required for richer planning context.",
            "Ready",
            "Medium",
            ["mcp", "notion"])
    ];

    public Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken) => Task.FromResult(Tasks);

    public Task<TaskItem?> GetTaskAsync(string taskId, CancellationToken cancellationToken) =>
        Task.FromResult(Tasks.FirstOrDefault(task =>
            string.Equals(task.Id, taskId, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(task.Key, taskId, StringComparison.OrdinalIgnoreCase)));
}

public sealed class MockNotionContextTool : INotionContextTool
{
    public string EndpointName => "mock://notion";

    public Task<string> GetTaskContextAsync(TaskItem task, CancellationToken cancellationToken)
    {
        // TODO: Replace with a real Notion MCP client.
        var context = $"Mock Notion context for {task.Key}: product notes prefer a transparent agent timeline, pragmatic mock integrations first, and swappable tool interfaces.";
        return Task.FromResult(context);
    }
}

public sealed class InMemorySettingsStore : ISettingsStore
{
    private ToolEndpointSettings _settings = new(
        "mock://jira",
        "mock://notion",
        RepositoryPathDefaults.Resolve());

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
                : settings.RepositoryPath
        };

        return _settings;
    }
}

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

public sealed class LocalRepositoryReader : IRepositoryReader
{
    public Task<RepositoryContext> GetContextAsync(string? repositoryPath, CancellationToken cancellationToken)
    {
        var path = string.IsNullOrWhiteSpace(repositoryPath)
            ? RepositoryPathDefaults.Resolve()
            : repositoryPath;

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

        return Task.FromResult(new RepositoryContext(fullPath, name, files, technologies, summary));
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

public sealed class MockMemoryService : IMemoryService
{
    private readonly List<MemoryItem> _items =
    [
        new("mem-001", "Previous workflow MVP", "Keep the first version runnable with mock services and clear extension points.", ["workflow", "mvp"], DateTimeOffset.UtcNow.AddDays(-3)),
        new("mem-002", "Tool abstraction note", "MCP tool clients should be hidden behind interfaces so Jira and Notion auth can evolve independently.", ["mcp", "tools"], DateTimeOffset.UtcNow.AddDays(-2)),
        new("mem-003", "Memory roadmap", "Start with interfaces for vector and graph memory before adding Qdrant and Neo4j clients.", ["memory", "qdrant", "neo4j"], DateTimeOffset.UtcNow.AddDays(-1))
    ];

    public Task<MemoryItem> StoreMemoryAsync(MemoryItem item, CancellationToken cancellationToken)
    {
        // TODO: Store in Qdrant and Neo4j when real infrastructure is wired.
        var stored = item with
        {
            Id = string.IsNullOrWhiteSpace(item.Id) ? $"mem-{Guid.NewGuid():N}" : item.Id,
            CreatedAt = item.CreatedAt == default ? DateTimeOffset.UtcNow : item.CreatedAt
        };

        lock (_items)
        {
            _items.Add(stored);
        }

        return Task.FromResult(stored);
    }

    public Task<IReadOnlyList<MemoryItem>> SearchVectorMemoryAsync(string query, CancellationToken cancellationToken)
    {
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        IReadOnlyList<MemoryItem> results;

        lock (_items)
        {
            results = _items
                .Where(item => terms.Length == 0 ||
                               terms.Any(term => item.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                                 item.Content.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                                 item.Tags.Any(tag => tag.Contains(term, StringComparison.OrdinalIgnoreCase))))
                .Take(5)
                .ToList();
        }

        return Task.FromResult(results);
    }

    public Task<IReadOnlyList<GraphEntity>> ReadGraphRelationshipsAsync(string entityId, CancellationToken cancellationToken)
    {
        IReadOnlyList<GraphEntity> entities =
        [
            new("entity-task", "Task", entityId, new Dictionary<string, string> { ["source"] = "Jira" }, ["entity-repo", "entity-memory"]),
            new("entity-repo", "Repository", "agent-workflow-builder", new Dictionary<string, string> { ["role"] = "codebase" }, ["entity-task"]),
            new("entity-memory", "Context", "workflow-memory", new Dictionary<string, string> { ["store"] = "mock-graph" }, ["entity-task"])
        ];

        return Task.FromResult(entities);
    }

    public Task LinkTaskRepositoryEntityAsync(string taskId, string repositoryName, string entityName, CancellationToken cancellationToken)
    {
        // TODO: Create task/repo/entity/context relationships in Neo4j.
        return Task.CompletedTask;
    }
}
