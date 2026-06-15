using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Cli.Application;

public sealed class CliRunner(IWorkflowEngine workflowEngine)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true
    };

    public async Task<int> RunAsync(CliOptions options, CancellationToken cancellationToken)
    {
        var run = await workflowEngine.StartInvestigationAsync(
            new InvestigationRequest(options.TaskId, options.RepositoryPath, RequestedAgents: []),
            cancellationToken);

        var json = JsonSerializer.Serialize(run, SerializerOptions);
        Console.WriteLine(json);

        return 0;
    }
}
