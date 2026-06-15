using AgentWorkflow.Mcp.Extensions;
using AgentWorkflow.Mcp.Protocol;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection()
    .AddAgentWorkflowMcp()
    .BuildServiceProvider();

var server = services.GetRequiredService<McpStdioServer>();

return await server.RunAsync(CancellationToken.None);
