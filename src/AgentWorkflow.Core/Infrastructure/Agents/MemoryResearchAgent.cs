using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

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
