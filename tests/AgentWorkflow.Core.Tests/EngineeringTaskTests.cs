using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class EngineeringTaskTests
{
    [Fact]
    public async Task Store_CreatesPlatformTaskWithLinkedJiraAndNotionWorkItems()
    {
        var store = new InMemoryEngineeringTaskStore();
        var task = await store.CreateTaskAsync(
            new CreateEngineeringTaskRequest(
                ProjectPolicyDefaults.DefaultProjectId,
                "Implement durable task intake",
                "Separate the platform task from provider work items.",
                ScheduledTaskPriority.High,
                [
                    WorkItem(WorkItemSource.Jira, "AWB-200"),
                    WorkItem(WorkItemSource.Notion, "notion-task-intake")
                ]),
            CancellationToken.None);

        var workItems = await store.GetWorkItemsAsync(task.Id, CancellationToken.None);

        Assert.Equal(EngineeringTaskStatus.New, task.Status);
        Assert.Equal(2, task.WorkItemIds.Count);
        Assert.Equal(2, workItems.Count);
        Assert.All(workItems, item => Assert.Equal(task.Id, item.EngineeringTaskId));
        Assert.Contains(workItems, item => item.Source == WorkItemSource.Jira);
        Assert.Contains(workItems, item => item.Source == WorkItemSource.Notion);
    }

    [Theory]
    [InlineData(EngineeringTaskStatus.Investigating, false)]
    [InlineData(EngineeringTaskStatus.Completed, true)]
    [InlineData(EngineeringTaskStatus.Failed, true)]
    public async Task Store_UpdatesTypedLifecycleAndTerminalTimestamp(
        EngineeringTaskStatus status,
        bool isTerminal)
    {
        var store = new InMemoryEngineeringTaskStore();
        var task = await store.CreateTaskAsync(
            new CreateEngineeringTaskRequest(
                ProjectPolicyDefaults.DefaultProjectId,
                "Lifecycle task",
                "Exercise typed lifecycle state.",
                ScheduledTaskPriority.Medium,
                []),
            CancellationToken.None);

        var updated = await store.UpdateStatusAsync(task.Id, status, CancellationToken.None);

        Assert.NotNull(updated);
        Assert.Equal(status, updated.Status);
        Assert.Equal(isTerminal, updated.CompletedAt is not null);
    }

    [Fact]
    public async Task CompatibilitySource_PreservesExistingMockTaskContract()
    {
        var store = new InMemoryEngineeringTaskStore();
        var source = new EngineeringTaskSource(store, store);

        var tasks = await source.GetTasksAsync(CancellationToken.None);
        var task = await source.GetTaskAsync("AWB-101", CancellationToken.None);

        Assert.Equal(3, tasks.Count);
        Assert.NotNull(task);
        Assert.Equal("jira-awb-101", task.Id);
        Assert.Equal("Jira", task.Source);
        Assert.Equal("AWB-101", task.Key);
        Assert.Equal("Ready", task.Status);
        Assert.Equal("High", task.Priority);
        Assert.Equal(["repo", "workflow", "mvp"], task.Tags);
    }

    [Fact]
    public async Task WorkspaceRequest_CreatesProjectOwnedEngineeringTask()
    {
        var taskStore = new InMemoryEngineeringTaskStore();
        var projectStore = new InMemoryProjectStore();
        var workspaceStore = new InMemoryWorkspaceStore(
            projectStore,
            new ToolEndpointSettings("mock://jira", "mock://notion", ".", "", "github"));
        var requestStore = new InMemoryRequestIntakeStore(workspaceStore, taskStore);

        var request = await requestStore.CreateRequestAsync(
            ProjectPolicyDefaults.DefaultProjectId,
            new CreateWorkspaceUserRequest("Add engineering task intake"),
            CancellationToken.None);
        var task = await taskStore.GetTaskAsync(request.Id, CancellationToken.None);

        Assert.NotNull(task);
        Assert.Equal(ProjectPolicyDefaults.DefaultProjectId, task.ProjectId);
        Assert.Equal(request.Content, task.Description);
        Assert.Equal(EngineeringTaskStatus.New, task.Status);
    }

    private static CreateWorkItemRequest WorkItem(
        WorkItemSource source,
        string sourceKey) =>
        new(
            source,
            sourceKey,
            "Provider task",
            "Provider description",
            "Ready",
            "High",
            ["provider"]);
}
