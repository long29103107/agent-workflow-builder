using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

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
