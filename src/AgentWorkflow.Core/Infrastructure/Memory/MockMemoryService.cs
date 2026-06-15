using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

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
