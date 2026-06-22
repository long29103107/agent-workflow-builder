using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class OpenAiLeadAgent : ILeadAgent
{
    private readonly ITaskSource _tasks;
    private readonly IWorkspaceTaskSource _workspaceTasks;
    private readonly INotionContextTool _notion;
    private readonly IRepositoryConnectionService _repositoryConnection;
    private readonly IRepositoryReader _repositoryReader;
    private readonly IMemoryService _memory;
    private readonly IAgentReasoningService _reasoning;
    private readonly IEnumerable<ISubagent> _subagents;

    public OpenAiLeadAgent(
        ITaskSource tasks,
        IWorkspaceTaskSource workspaceTasks,
        INotionContextTool notion,
        IRepositoryConnectionService repositoryConnection,
        IRepositoryReader repositoryReader,
        IMemoryService memory,
        IAgentReasoningService reasoning,
        IEnumerable<ISubagent> subagents)
    {
        _tasks = tasks;
        _workspaceTasks = workspaceTasks;
        _notion = notion;
        _repositoryConnection = repositoryConnection;
        _repositoryReader = repositoryReader;
        _memory = memory;
        _reasoning = reasoning;
        _subagents = subagents;
    }

    public async Task<InvestigationResult> InvestigateAsync(
        InvestigationRequest request,
        Func<WorkflowStage, string, string, Task> advanceStage,
        CancellationToken cancellationToken)
    {
        await advanceStage(WorkflowStage.LoadingTaskContext, "LeadAgent", "Loading task context.");
        var task = request.WorkspaceId is null
            ? await _tasks.GetTaskAsync(request.TaskId, cancellationToken)
            : await _workspaceTasks.GetTaskAsync(request.WorkspaceId, request.TaskId, cancellationToken);
        if (task is null)
        {
            throw new InvalidOperationException($"Task '{request.TaskId}' was not found.");
        }

        var notionContext = await _notion.GetTaskContextAsync(task, cancellationToken);

        await advanceStage(WorkflowStage.ResolvingRepository, "LeadAgent", "Resolving repository connection.");
        var connection = _repositoryConnection.ResolveConnection(request.RepositoryPath, request.RepositoryUrl);

        await advanceStage(WorkflowStage.LoadingMemory, "LeadAgent", $"Reading repository context from {connection.Provider} target and querying memory.");
        var repository = await _repositoryReader.GetContextAsync(connection, cancellationToken);

        var memories = await _memory.SearchVectorMemoryAsync($"{task.Title} {task.Description}", cancellationToken);
        var graph = await _memory.ReadGraphRelationshipsAsync(task.Key, cancellationToken);
        await _memory.LinkTaskRepositoryEntityAsync(task.Key, repository.Name, "workflow-context", cancellationToken);

        var context = new AgentWorkContext(task, notionContext, repository, memories, graph);
        var activeAgents = SelectAgents(request.RequestedAgents).ToList();
        var results = new List<SubagentResult>();

        await advanceStage(WorkflowStage.Investigating, "LeadAgent", "Delegating investigation to selected agents.");

        foreach (var agent in activeAgents)
        {
            await advanceStage(WorkflowStage.Investigating, agent.Name, "Investigation started.");
            results.Add(await agent.InvestigateAsync(context, cancellationToken));
            await advanceStage(WorkflowStage.Investigating, agent.Name, "Investigation completed.");
        }

        await advanceStage(WorkflowStage.Aggregating, "LeadAgent", "Aggregating subagent outputs with OpenAI SDK reasoning.");
        var steps = results.SelectMany(result => result.SuggestedSteps)
            .OrderBy(step => step.Order)
            .ToList();
        var messages = results
            .Select(result => new AgentMessage(result.AgentName, "assistant", result.Summary, DateTimeOffset.UtcNow))
            .ToList();

        var reasoning = await _reasoning.SummarizeInvestigationAsync(
            new AgentReasoningRequest(
                task.Key,
                task.Title,
                repository.Name,
                results.Select(result => result.Summary).ToList()),
            cancellationToken);

        var risks = results.SelectMany(result => result.Risks)
            .Concat(reasoning.SuggestedRisks)
            .Distinct()
            .ToList();
        var questions = results.SelectMany(result => result.OpenQuestions)
            .Concat(reasoning.OpenQuestions)
            .Distinct()
            .ToList();

        var plan = new ExecutionPlan(
            $"Execution plan for {task.Key}",
            steps,
            risks,
            questions);

        return new InvestigationResult(reasoning.Summary, plan, messages, repository, memories, graph);
    }

    private IEnumerable<ISubagent> SelectAgents(IReadOnlyList<string>? requestedAgents)
    {
        if (requestedAgents is null || requestedAgents.Count == 0)
        {
            return _subagents;
        }

        var selectedAgents = _subagents
            .Where(agent => requestedAgents.Any(requested =>
                agent.Name.Contains(requested, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        return selectedAgents.Count == 0 ? _subagents : selectedAgents;
    }
}
