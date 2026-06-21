using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Cli.Application;

public sealed class CliRunner(IWorkflowEngine workflowEngine)
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public async Task<int> RunAsync(CliOptions options, CancellationToken cancellationToken)
    {
        var run = await workflowEngine.StartInvestigationAsync(
            new InvestigationRequest(options.TaskId, options.RepositoryPath, options.RepositoryUrl, RequestedAgents: []),
            cancellationToken);

        var json = JsonSerializer.Serialize(run, SerializerOptions);
        Console.WriteLine(json);

        return 0;
    }
}
