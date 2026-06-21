using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AgentWorkflow.Api.Tests;

internal sealed class AgentWorkflowApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Persistence:Provider", "InMemory");
    }
}
