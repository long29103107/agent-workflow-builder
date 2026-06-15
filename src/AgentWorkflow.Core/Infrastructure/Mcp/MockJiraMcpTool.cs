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
