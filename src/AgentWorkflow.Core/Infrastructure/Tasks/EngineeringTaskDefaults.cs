using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

internal static class EngineeringTaskDefaults
{
    public static IReadOnlyList<EngineeringTaskSeed> Create() =>
    [
        Seed(
            "jira-awb-101",
            "AWB-101",
            "Investigate repository onboarding workflow",
            "Create a first-pass investigation plan for connecting local repositories to the agent workflow builder.",
            "Ready",
            ScheduledTaskPriority.High,
            ["repo", "workflow", "mvp"]),
        Seed(
            "jira-awb-118",
            "AWB-118",
            "Design memory graph linking",
            "Map how task, repository, entity, and context nodes should connect before the Neo4j implementation.",
            "Backlog",
            ScheduledTaskPriority.Medium,
            ["memory", "neo4j"]),
        Seed(
            "jira-awb-124",
            "AWB-124",
            "Prepare Notion MCP discovery",
            "Define the Notion pages and database fields required for richer planning context.",
            "Ready",
            ScheduledTaskPriority.Medium,
            ["mcp", "notion"])
    ];

    private static EngineeringTaskSeed Seed(
        string taskId,
        string sourceKey,
        string title,
        string description,
        string legacyStatus,
        ScheduledTaskPriority priority,
        IReadOnlyList<string> tags) =>
        new(
            taskId,
            legacyStatus,
            new CreateEngineeringTaskRequest(
                ProjectPolicyDefaults.DefaultProjectId,
                title,
                description,
                priority,
                [new CreateWorkItemRequest(
                    WorkItemSource.Jira,
                    sourceKey,
                    title,
                    description,
                    legacyStatus,
                    priority.ToString(),
                    tags)]));
}

internal sealed record EngineeringTaskSeed(
    string TaskId,
    string LegacyStatus,
    CreateEngineeringTaskRequest Request);
