using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class MockNotionContextTool : INotionContextTool
{
    public string EndpointName => "mock://notion";

    public Task<string> GetTaskContextAsync(TaskItem task, CancellationToken cancellationToken)
    {
        // TODO: Replace with a real Notion MCP client.
        var context = $"Mock Notion context for {task.Key}: product notes prefer a transparent agent timeline, pragmatic mock integrations first, and swappable tool interfaces.";
        return Task.FromResult(context);
    }
}
