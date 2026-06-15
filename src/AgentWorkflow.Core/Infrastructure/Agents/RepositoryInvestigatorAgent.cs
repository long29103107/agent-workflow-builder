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
