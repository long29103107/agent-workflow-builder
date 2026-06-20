using AgentWorkflow.Api.Endpoints;
using AgentWorkflow.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAgentWorkflowApi(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseCors();

app.MapAgentWorkflowApiDocumentation();
app.MapAgentWorkflowApi();

app.Run();

public partial class Program;
