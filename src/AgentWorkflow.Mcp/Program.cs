using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddAgentWorkflowCore()
    .BuildServiceProvider();

var workflowEngine = services.GetRequiredService<IWorkflowEngine>();
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = false
};

Console.Error.WriteLine("AgentWorkflow.Mcp ready. Send one JSON request per line.");

string? line;
while ((line = await Console.In.ReadLineAsync()) is not null)
{
    try
    {
        var request = JsonSerializer.Deserialize<McpInvestigationRequest>(line, jsonOptions);
        if (request is null || !string.Equals(request.Method, "workflow.investigate", StringComparison.OrdinalIgnoreCase))
        {
            WriteResponse(new { error = "Unsupported method. Use workflow.investigate." });
            continue;
        }

        var run = await workflowEngine.StartInvestigationAsync(
            new InvestigationRequest(request.TaskId, request.RepositoryPath, request.RequestedAgents ?? []),
            CancellationToken.None);

        WriteResponse(new { result = run });
    }
    catch (Exception ex)
    {
        WriteResponse(new { error = ex.Message });
    }
}

void WriteResponse(object response)
{
    Console.WriteLine(JsonSerializer.Serialize(response, jsonOptions));
    Console.Out.Flush();
}

internal sealed record McpInvestigationRequest(
    string Method,
    string TaskId,
    string? RepositoryPath,
    IReadOnlyList<string>? RequestedAgents);
