using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173"));
});

builder.Services.AddAgentWorkflowCore();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

app.UseCors();

var api = app.MapGroup("/api");

api.MapGet("/health", () => Results.Ok(new { status = "ok" }));

api.MapGet("/tasks", async (ITaskSource tasks, CancellationToken cancellationToken) =>
{
    var result = await tasks.GetTasksAsync(cancellationToken);
    return Results.Ok(result);
});

api.MapPost("/workflows/investigate", async (
    InvestigationRequest request,
    IWorkflowEngine workflowEngine,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.TaskId))
    {
        return Results.BadRequest(new { error = "taskId is required." });
    }

    var run = await workflowEngine.StartInvestigationAsync(request, cancellationToken);
    return Results.Created($"/api/workflows/{run.Id}", run);
});

api.MapGet("/workflows/{runId:guid}", (Guid runId, IWorkflowRunStore store) =>
{
    var run = store.GetRun(runId);
    return run is null ? Results.NotFound() : Results.Ok(run);
});

api.MapGet("/workflows/{runId:guid}/events", (Guid runId, IWorkflowRunStore store) =>
{
    if (store.GetRun(runId) is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(store.GetEvents(runId));
});

api.MapGet("/memory/search", async (
    string query,
    IMemoryService memory,
    CancellationToken cancellationToken) =>
{
    var results = await memory.SearchVectorMemoryAsync(query, cancellationToken);
    return Results.Ok(results);
});

api.MapPost("/memory", async (
    MemoryItem item,
    IMemoryService memory,
    CancellationToken cancellationToken) =>
{
    var stored = await memory.StoreMemoryAsync(item, cancellationToken);
    return Results.Created($"/api/memory/search?query={Uri.EscapeDataString(stored.Title)}", stored);
});

api.MapGet("/repos/context", async (
    string? path,
    IRepositoryReader repositoryReader,
    CancellationToken cancellationToken) =>
{
    var context = await repositoryReader.GetContextAsync(path, cancellationToken);
    return Results.Ok(context);
});

api.MapGet("/settings", (ISettingsStore settingsStore) =>
{
    return Results.Ok(settingsStore.GetSettings());
});

api.MapPost("/settings", (ToolEndpointSettings settings, ISettingsStore settingsStore) =>
{
    var updated = settingsStore.UpdateSettings(settings);
    return Results.Ok(updated);
});

app.Run();
