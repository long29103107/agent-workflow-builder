using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class RepositoryInvestigatorAgent : ISubagent
{
    public string Name => "Repository Investigator Agent";

    public Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken)
    {
        var summary = $"{context.Repository.Name} uses {string.Join(", ", context.Repository.Technologies)}. Important files: {string.Join(", ", context.Repository.ImportantFiles.Take(5))}.";
        var steps = new[]
        {
            new ExecutionStep(1, "Confirm repository boundaries", "Review the listed files and identify the backend/frontend ownership boundaries.", Name, "Proposed"),
            new ExecutionStep(2, "Add real repo intelligence", "Replace the local reader with GitHub/GitLab search and file summarization.", Name, "Future")
        };

        return Task.FromResult(new SubagentResult(Name, summary, steps, ["Repository scan is shallow in the MVP."], []));
    }
}

public sealed class JiraNotionContextAgent : ISubagent
{
    public string Name => "Jira/Notion Context Agent";

    public Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken)
    {
        var summary = $"{context.Task.Key} is {context.Task.Priority} priority. Notion context says: {context.NotionContext}";
        var steps = new[]
        {
            new ExecutionStep(3, "Validate task acceptance criteria", "Pull full Jira fields and linked Notion decisions before implementation.", Name, "Proposed")
        };

        return Task.FromResult(new SubagentResult(Name, summary, steps, ["Mock Jira/Notion data may miss live blockers."], ["Which Notion database is authoritative for specs?"]));
    }
}

public sealed class MemoryResearchAgent : ISubagent
{
    public string Name => "Memory Research Agent";

    public Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken)
    {
        var memoryTitles = context.Memories.Count == 0
            ? "No matching memories"
            : string.Join(", ", context.Memories.Select(memory => memory.Title));
        var summary = $"Memory search found: {memoryTitles}. Graph relationships found: {context.GraphEntities.Count}.";
        var steps = new[]
        {
            new ExecutionStep(4, "Wire real memory stores", "Implement Qdrant search and Neo4j relationship persistence behind IMemoryService.", Name, "Future")
        };

        return Task.FromResult(new SubagentResult(Name, summary, steps, ["Memory ranking is keyword-based until Qdrant is connected."], []));
    }
}

public sealed class PlanningAgent : ISubagent
{
    public string Name => "Planning Agent";

    public Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken)
    {
        var steps = new[]
        {
            new ExecutionStep(5, "Implement the smallest safe workflow", "Keep the first implementation mock-backed, observable, and replaceable.", Name, "Proposed"),
            new ExecutionStep(6, "Add integration checkpoints", "Add tests and status events before swapping in real LLM/MCP/memory providers.", Name, "Proposed")
        };

        return Task.FromResult(new SubagentResult(Name, "Plan favors a runnable vertical slice with explicit TODOs for real integrations.", steps, ["Advanced orchestration is intentionally deferred."], []));
    }
}

public sealed class OpenAiLeadAgent : ILeadAgent
{
    private readonly ITaskSource _tasks;
    private readonly INotionContextTool _notion;
    private readonly IRepositoryReader _repositoryReader;
    private readonly IMemoryService _memory;
    private readonly IAgentReasoningService _reasoning;
    private readonly IEnumerable<ISubagent> _subagents;

    public OpenAiLeadAgent(
        ITaskSource tasks,
        INotionContextTool notion,
        IRepositoryReader repositoryReader,
        IMemoryService memory,
        IAgentReasoningService reasoning,
        IEnumerable<ISubagent> subagents)
    {
        _tasks = tasks;
        _notion = notion;
        _repositoryReader = repositoryReader;
        _memory = memory;
        _reasoning = reasoning;
        _subagents = subagents;
    }

    public async Task<InvestigationResult> InvestigateAsync(
        InvestigationRequest request,
        Action<string, string> emitEvent,
        CancellationToken cancellationToken)
    {
        emitEvent("LeadAgent", "Loading Jira task context.");
        var task = await _tasks.GetTaskAsync(request.TaskId, cancellationToken)
            ?? throw new InvalidOperationException($"Task '{request.TaskId}' was not found.");

        var notionContext = await _notion.GetTaskContextAsync(task, cancellationToken);

        emitEvent("LeadAgent", "Reading repository context.");
        var repository = await _repositoryReader.GetContextAsync(request.RepositoryPath, cancellationToken);

        emitEvent("LeadAgent", "Querying vector and graph memory.");
        var memories = await _memory.SearchVectorMemoryAsync($"{task.Title} {task.Description}", cancellationToken);
        var graph = await _memory.ReadGraphRelationshipsAsync(task.Key, cancellationToken);
        await _memory.LinkTaskRepositoryEntityAsync(task.Key, repository.Name, "workflow-context", cancellationToken);

        var context = new AgentWorkContext(task, notionContext, repository, memories, graph);
        var activeAgents = SelectAgents(request.RequestedAgents).ToList();
        var results = new List<SubagentResult>();

        foreach (var agent in activeAgents)
        {
            emitEvent(agent.Name, "Investigation started.");
            results.Add(await agent.InvestigateAsync(context, cancellationToken));
            emitEvent(agent.Name, "Investigation completed.");
        }

        emitEvent("LeadAgent", "Aggregating subagent outputs with OpenAI SDK reasoning.");
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

public sealed class WorkflowEngine : IWorkflowEngine
{
    private readonly IWorkflowRunStore _store;
    private readonly ILeadAgent _leadAgent;

    public WorkflowEngine(IWorkflowRunStore store, ILeadAgent leadAgent)
    {
        _store = store;
        _leadAgent = leadAgent;
    }

    public async Task<WorkflowRun> StartInvestigationAsync(InvestigationRequest request, CancellationToken cancellationToken)
    {
        var run = _store.CreateRun(request.TaskId);

        try
        {
            var result = await _leadAgent.InvestigateAsync(
                request,
                (agent, message) => _store.AddEvent(run.Id, agent, "Activity", message),
                cancellationToken);

            return _store.CompleteRun(run.Id, result);
        }
        catch (Exception ex)
        {
            return _store.FailRun(run.Id, ex.Message);
        }
    }
}
