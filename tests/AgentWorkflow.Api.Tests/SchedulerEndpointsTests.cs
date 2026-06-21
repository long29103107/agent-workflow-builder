using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Tests;

public sealed class SchedulerEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task SwaggerUi_IsAvailable()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/swagger/index.html", CancellationToken.None);
        var content = await response.Content.ReadAsStringAsync(CancellationToken.None);
        var documentResponse = await client.GetAsync(
            "/swagger/v1/swagger.json",
            CancellationToken.None);
        var document = await documentResponse.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(HttpStatusCode.OK, documentResponse.StatusCode);
        Assert.Contains("swagger-ui", content, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"openapi\"", document, StringComparison.Ordinal);

        using var openApi = JsonDocument.Parse(document);
        AssertOpenApiTag(openApi, "/api/scheduler/tasks", "get", "Scheduler");
        AssertOpenApiTag(openApi, "/api/workflows/investigate", "post", "Workflows");
        AssertOpenApiTag(openApi, "/api/workspaces", "get", "Workspaces");
    }

    [Fact]
    public async Task SchedulerEndpoints_QueueAndProcessHighestPriorityTask()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();

        var lowResponse = await client.PostAsJsonAsync(
            "/api/scheduler/tasks",
            new ScheduleTaskRequest("jira-awb-118", ScheduledTaskPriority.Low, ".", null),
            CancellationToken.None);
        var criticalResponse = await client.PostAsJsonAsync(
            "/api/scheduler/tasks",
            new ScheduleTaskRequest("jira-awb-101", ScheduledTaskPriority.Critical, ".", null),
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, lowResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Created, criticalResponse.StatusCode);

        var processResponse = await client.PostAsync(
            "/api/scheduler/process-next",
            content: null,
            cancellationToken: CancellationToken.None);
        var processed = await processResponse.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            cancellationToken: CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);
        Assert.NotNull(processed);
        Assert.Equal("jira-awb-101", processed.TaskId);
        Assert.Equal(ScheduledTaskStatus.Completed, processed.Status);

        var queue = await client.GetFromJsonAsync<IReadOnlyList<ScheduledTask>>(
            "/api/scheduler/tasks",
            JsonOptions,
            CancellationToken.None);

        Assert.NotNull(queue);
        Assert.Contains(queue, task =>
            task.TaskId == "jira-awb-118" &&
            task.Status == ScheduledTaskStatus.Queued);
    }

    [Fact]
    public async Task ProcessNext_ReturnsNotFoundWhenQueueIsEmpty()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/api/scheduler/process-next",
            content: null,
            cancellationToken: CancellationToken.None);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static void AssertOpenApiTag(
        JsonDocument openApi,
        string path,
        string method,
        string expectedTag)
    {
        var tags = openApi.RootElement
            .GetProperty("paths")
            .GetProperty(path)
            .GetProperty(method)
            .GetProperty("tags");

        Assert.Contains(tags.EnumerateArray(), tag => tag.GetString() == expectedTag);
    }
}
