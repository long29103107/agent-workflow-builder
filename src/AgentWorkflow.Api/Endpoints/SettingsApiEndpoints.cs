using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Endpoints;

public static class SettingsApiEndpoints
{
    public static RouteGroupBuilder MapSettingsApi(this RouteGroupBuilder api)
    {
        var settings = api.MapGroup("/settings").WithTags("Settings");

        settings.MapGet("", (ISettingsStore settingsStore) =>
            Results.Ok(settingsStore.GetSettings()));

        settings.MapPost("", (ToolEndpointSettings value, ISettingsStore settingsStore) =>
        {
            var updated = settingsStore.UpdateSettings(value);
            return Results.Ok(updated);
        });

        return settings;
    }
}
