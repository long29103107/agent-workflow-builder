using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AgentWorkflow.Api.Tests;

internal sealed class AgentWorkflowApiFactory(bool enableWorkflowWorker = false) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Persistence:Provider", "InMemory");
        builder.UseSetting("WorkflowWorker:Enabled", enableWorkflowWorker.ToString());
    }
}
