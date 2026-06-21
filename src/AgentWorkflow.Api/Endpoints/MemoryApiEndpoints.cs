using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class MemoryApiEndpoints
{
    public static RouteGroupBuilder MapMemoryApi(this RouteGroupBuilder api)
    {
        var memory = api.MapGroup("/memory").WithTags("Memory");

        memory.MapGet("/search", async (
            string query,
            IMemoryService memoryService,
            CancellationToken cancellationToken) =>
        {
            var results = await memoryService.SearchVectorMemoryAsync(query, cancellationToken);
            return Results.Ok(results);
        });

        memory.MapPost("", async (
            MemoryItem item,
            IMemoryService memoryService,
            CancellationToken cancellationToken) =>
        {
            var stored = await memoryService.StoreMemoryAsync(item, cancellationToken);
            return Results.Created($"/api/memory/search?query={Uri.EscapeDataString(stored.Title)}", stored);
        });

        return memory;
    }
}
