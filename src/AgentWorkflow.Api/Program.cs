using AgentWorkflow.Api.Endpoints;
using AgentWorkflow.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAgentWorkflowApi();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseCors();

app.MapAgentWorkflowApi();

app.Run();
