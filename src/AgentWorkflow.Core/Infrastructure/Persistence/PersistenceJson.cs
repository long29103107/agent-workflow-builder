using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgentWorkflow.Core.Infrastructure;

internal static class PersistenceJson
{
    public static JsonSerializerOptions Options { get; } = CreateOptions();

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
