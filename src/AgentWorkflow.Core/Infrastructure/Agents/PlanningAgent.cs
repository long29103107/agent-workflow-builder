using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

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
