using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Mcp.Contracts;

namespace AgentWorkflow.Mcp.Protocol;

public sealed class McpStdioServer(IWorkflowEngine workflowEngine)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<int> RunAsync(CancellationToken cancellationToken)
    {
        Console.Error.WriteLine("AgentWorkflow.Mcp ready. Send one JSON request per line.");

        string? line;
        while ((line = await Console.In.ReadLineAsync()) is not null)
        {
            await HandleLineAsync(line, cancellationToken);
        }

        return 0;
    }

    private async Task HandleLineAsync(string line, CancellationToken cancellationToken)
    {
        try
        {
            var request = JsonSerializer.Deserialize<McpInvestigationRequest>(line, JsonOptions);
            if (request is null || !string.Equals(request.Method, "workflow.investigate", StringComparison.OrdinalIgnoreCase))
            {
                WriteResponse(new { error = "Unsupported method. Use workflow.investigate." });
                return;
            }

            var run = await workflowEngine.StartInvestigationAsync(
                new InvestigationRequest(request.TaskId, request.RepositoryPath, request.RequestedAgents ?? []),
                cancellationToken);

            WriteResponse(new { result = run });
        }
        catch (Exception ex)
        {
            WriteResponse(new { error = ex.Message });
        }
    }

    private static void WriteResponse(object response)
    {
        Console.WriteLine(JsonSerializer.Serialize(response, JsonOptions));
        Console.Out.Flush();
    }
}
