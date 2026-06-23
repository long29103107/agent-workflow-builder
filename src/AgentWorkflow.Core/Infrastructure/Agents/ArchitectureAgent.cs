using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class ArchitectureAgent : ISubagent
{
    public string Name => "Architecture Agent";

    public Task<SubagentResult> InvestigateAsync(AgentWorkContext context, CancellationToken cancellationToken)
    {
        var boundaries = DetectBoundaries(context.Repository.ImportantFiles);
        var boundarySummary = boundaries.Count == 0
            ? "No explicit project boundaries were detected from the repository context."
            : $"Detected architecture boundaries: {string.Join(", ", boundaries)}.";
        var steps = new[]
        {
            new ExecutionStep(3, "Map architecture impact", "Identify the Core contracts, adapters, and tests affected before implementation.", Name, "Proposed"),
            new ExecutionStep(4, "Preserve provider boundaries", "Keep external systems behind interfaces and update mock-first implementations before real providers.", Name, "Proposed")
        };
        var risks = new[]
        {
            "Architecture analysis is based on repository context signals until deeper source indexing is available."
        };
        var questions = boundaries.Count == 0
            ? ["Which application boundary owns this task?"]
            : Array.Empty<string>();
        var summary = $"{boundarySummary} The implementation plan should keep AgentWorkflow.Core as the source of truth and adapters thin.";

        return Task.FromResult(new SubagentResult(Name, summary, steps, risks, questions));
    }

    private static IReadOnlyList<string> DetectBoundaries(IReadOnlyList<string> importantFiles)
    {
        var boundaries = new List<string>();
        AddBoundary(boundaries, importantFiles, "src/AgentWorkflow.Core", "Core");
        AddBoundary(boundaries, importantFiles, "src/AgentWorkflow.Api", "API adapter");
        AddBoundary(boundaries, importantFiles, "src/AgentWorkflow.Cli", "CLI adapter");
        AddBoundary(boundaries, importantFiles, "src/AgentWorkflow.Mcp", "MCP adapter");
        AddBoundary(boundaries, importantFiles, "src/agent-workflow-ui", "React UI");
        AddBoundary(boundaries, importantFiles, "tests/", "Tests");
        return boundaries;
    }

    private static void AddBoundary(
        List<string> boundaries,
        IReadOnlyList<string> importantFiles,
        string pathSegment,
        string boundary)
    {
        if (importantFiles.Any(file =>
            file.Replace('\\', '/').Contains(pathSegment, StringComparison.OrdinalIgnoreCase)))
        {
            boundaries.Add(boundary);
        }
    }
}
