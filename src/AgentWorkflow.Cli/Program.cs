using System.Text.Json;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

var taskId = args.Length > 0 ? args[0] : "jira-awb-101";
var repositoryPath = args.Length > 1 ? args[1] : null;

var services = new ServiceCollection()
    .AddAgentWorkflowCore()
    .BuildServiceProvider();

var workflowEngine = services.GetRequiredService<IWorkflowEngine>();
var run = await workflowEngine.StartInvestigationAsync(
    new InvestigationRequest(taskId, repositoryPath, RequestedAgents: []),
    CancellationToken.None);

var json = JsonSerializer.Serialize(run, new JsonSerializerOptions
{
    WriteIndented = true
});

Console.WriteLine(json);
