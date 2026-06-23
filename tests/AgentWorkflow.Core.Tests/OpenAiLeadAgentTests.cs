using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class OpenAiLeadAgentTests
{
    [Fact]
    public async Task InvestigateAsync_SelectedArchitectureAgentAddsArchitecturePlanSlice()
    {
        var repository = Repository();
        var skippedAgent = new RecordingSubagent("Planning Agent");
        var agent = CreateLeadAgent(
            repository,
            subagents: [new ArchitectureAgent(), skippedAgent]);

        var result = await agent.InvestigateAsync(
            new InvestigationRequest("task-1", ".", null, ["Architecture"]),
            (_, _, _) => Task.CompletedTask,
            CancellationToken.None);

        Assert.Contains(result.Plan.Steps, step =>
            step.OwnerAgent == "Architecture Agent" &&
            step.Title == "Map architecture impact");
        Assert.DoesNotContain(result.AgentMessages, message => message.AgentName == skippedAgent.Name);
        Assert.Equal(0, skippedAgent.Calls);
    }

    [Fact]
    public async Task InvestigateAsync_NoRequestedAgentsRunsAllSubagents()
    {
        var repository = Repository();
        var planningAgent = new RecordingSubagent("Planning Agent");
        var agent = CreateLeadAgent(
            repository,
            subagents: [new ArchitectureAgent(), planningAgent]);

        var result = await agent.InvestigateAsync(
            new InvestigationRequest("task-1", ".", null, null),
            (_, _, _) => Task.CompletedTask,
            CancellationToken.None);

        Assert.Contains(result.AgentMessages, message => message.AgentName == "Architecture Agent");
        Assert.Contains(result.AgentMessages, message => message.AgentName == planningAgent.Name);
        Assert.Equal(1, planningAgent.Calls);
    }

    [Fact]
    public async Task InvestigateAsync_AddsEvidenceBackedSourceReferencesToPlan()
    {
        var repository = Repository();
        var memory = new MemoryItem(
            "memory-1",
            "Prior plan rule",
            "Plans must include evidence.",
            ["plan"],
            DateTimeOffset.UtcNow);
        var graph = new GraphEntity(
            "graph-1",
            "Service",
            "AgentWorkflow.Core",
            new Dictionary<string, string>(),
            []);
        var agent = CreateLeadAgent(
            repository,
            memories: [memory],
            graphEntities: [graph],
            subagents: [new FakeSubagent()]);

        var result = await agent.InvestigateAsync(
            new InvestigationRequest("task-1", ".", null, null),
            (_, _, _) => Task.CompletedTask,
            CancellationToken.None);

        Assert.Equal(
        [
            "src/AgentWorkflow.Core/Domain/WorkflowModels.cs",
            "src/AgentWorkflow.Core/Infrastructure/Agents/OpenAiLeadAgent.cs"
        ], result.Plan.SourceReferences);
        Assert.Equal(
            "2 repository source reference(s), 1 memory item(s), and 1 graph entity match(es) informed this plan.",
            result.Plan.EvidenceSummary);
    }

    private static OpenAiLeadAgent CreateLeadAgent(
        RepositoryContext repository,
        IReadOnlyList<MemoryItem>? memories = null,
        IReadOnlyList<GraphEntity>? graphEntities = null,
        IReadOnlyList<ISubagent>? subagents = null) =>
        new(
            new FakeTaskSource(TaskFixture()),
            new EmptyWorkspaceTaskSource(),
            new FakeNotionContextTool(),
            new FakeRepositoryConnectionService(repository.Connection),
            new FakeRepositoryReader(repository),
            new FakeMemoryService(memories ?? [], graphEntities ?? []),
            new FakeReasoningService(),
            subagents ?? [new FakeSubagent()]);

    private static TaskItem TaskFixture() =>
        new(
            "task-1",
            "Jira",
            "AWB-1",
            "Add evidence-backed plan",
            "Produce source-linked implementation plan output.",
            "Ready",
            "High",
            ["planning"]);

    private static RepositoryContext Repository() =>
        new(
            ".",
            "agent-workflow-builder",
            new RepositoryConnection(
                "GitHub",
                "https://github.com/acme/agent-workflow-builder",
                ".",
                "acme",
                "agent-workflow-builder",
                "main",
                "Connected",
                "Connected repository"),
            [
                "src/AgentWorkflow.Core/Domain/WorkflowModels.cs",
                "src/AgentWorkflow.Core/Domain/WorkflowModels.cs",
                "src/AgentWorkflow.Core/Infrastructure/Agents/OpenAiLeadAgent.cs"
            ],
            ["dotnet"],
            "Core workflow repository context.");

    private sealed class FakeTaskSource(TaskItem task) : ITaskSource
    {
        public Task<IReadOnlyList<TaskItem>> GetTasksAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaskItem>>([task]);

        public Task<TaskItem?> GetTaskAsync(string taskId, CancellationToken cancellationToken) =>
            Task.FromResult<TaskItem?>(task);
    }

    private sealed class EmptyWorkspaceTaskSource : IWorkspaceTaskSource
    {
        public Task<IReadOnlyList<TaskItem>> GetTasksAsync(string workspaceId, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<TaskItem>>([]);

        public Task<TaskItem?> GetTaskAsync(string workspaceId, string taskId, CancellationToken cancellationToken) =>
            Task.FromResult<TaskItem?>(null);
    }

    private sealed class FakeNotionContextTool : INotionContextTool
    {
        public string EndpointName => "notion";

        public Task<string> GetTaskContextAsync(TaskItem task, CancellationToken cancellationToken) =>
            Task.FromResult("Task context");
    }

    private sealed class FakeRepositoryConnectionService(RepositoryConnection connection) : IRepositoryConnectionService
    {
        public RepositoryConnection GetConnection() => connection;

        public RepositoryConnection UpdateConnection(RepositoryConnection connection) => connection;

        public RepositoryConnection ResolveConnection(string? repositoryPath, string? repositoryUrl) => connection;
    }

    private sealed class FakeRepositoryReader(RepositoryContext repository) : IRepositoryReader
    {
        public Task<RepositoryContext> GetContextAsync(RepositoryConnection connection, CancellationToken cancellationToken) =>
            Task.FromResult(repository);
    }

    private sealed class FakeMemoryService(
        IReadOnlyList<MemoryItem> memories,
        IReadOnlyList<GraphEntity> graphEntities) : IMemoryService
    {
        public Task<MemoryItem> StoreMemoryAsync(MemoryItem item, CancellationToken cancellationToken) =>
            Task.FromResult(item);

        public Task<IReadOnlyList<MemoryItem>> SearchVectorMemoryAsync(string query, CancellationToken cancellationToken) =>
            Task.FromResult(memories);

        public Task<IReadOnlyList<GraphEntity>> ReadGraphRelationshipsAsync(string entityId, CancellationToken cancellationToken) =>
            Task.FromResult(graphEntities);

        public Task LinkTaskRepositoryEntityAsync(
            string taskId,
            string repositoryName,
            string entityName,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class FakeReasoningService : IAgentReasoningService
    {
        public Task<AgentReasoningResult> SummarizeInvestigationAsync(
            AgentReasoningRequest request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new AgentReasoningResult("Summary", [], []));
    }

    private sealed class FakeSubagent : ISubagent
    {
        public string Name => "Architecture";

        public Task<SubagentResult> InvestigateAsync(
            AgentWorkContext context,
            CancellationToken cancellationToken) =>
            Task.FromResult(new SubagentResult(
                Name,
                "Architecture reviewed repository evidence.",
                [new ExecutionStep(1, "Review sources", "Inspect cited source files.", Name, "planned")],
                [],
                []));
    }

    private sealed class RecordingSubagent(string name) : ISubagent
    {
        public string Name { get; } = name;
        public int Calls { get; private set; }

        public Task<SubagentResult> InvestigateAsync(
            AgentWorkContext context,
            CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult(new SubagentResult(
                Name,
                $"{Name} reviewed the task.",
                [new ExecutionStep(9, $"{Name} step", "Review task details.", Name, "Proposed")],
                [],
                []));
        }
    }
}
