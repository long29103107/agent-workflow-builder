using AgentWorkflow.Cli.Application;
using AgentWorkflow.Cli.Extensions;
using Microsoft.Extensions.DependencyInjection;

var options = CliOptions.FromArgs(args);

var services = new ServiceCollection()
    .AddAgentWorkflowCli()
    .BuildServiceProvider();

var runner = services.GetRequiredService<CliRunner>();

return await runner.RunAsync(options, CancellationToken.None);
