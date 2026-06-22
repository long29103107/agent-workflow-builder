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
        AssertOpenApiTag(openApi, "/api/projects", "get", "Projects");
        AssertOpenApiTag(
            openApi,
            "/api/projects/{projectId}/tasks",
            "get",
            "Engineering Tasks");
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

        var critical = await criticalResponse.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            cancellationToken: CancellationToken.None);
        Assert.NotNull(critical?.WorkflowRunId);

        var persistedRunResponse = await client.GetAsync(
            $"/api/workflows/{critical.WorkflowRunId}",
            CancellationToken.None);
        var persistedRun = await persistedRunResponse.Content.ReadFromJsonAsync<WorkflowRun>(
            JsonOptions,
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, persistedRunResponse.StatusCode);
        Assert.Equal(WorkflowStage.Created, persistedRun?.Stage);

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

    [Fact]
    public async Task Investigate_QueuesWorkAndBackgroundWorkerCompletesIt()
    {
        await using var factory = new AgentWorkflowApiFactory(enableWorkflowWorker: true);
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/workflows/investigate",
            new InvestigationRequest("jira-awb-101", ".", null, []),
            CancellationToken.None);
        var queued = await response.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        Assert.NotNull(queued);

        ScheduledTask? current = null;
        for (var attempt = 0; attempt < 100; attempt++)
        {
            current = await client.GetFromJsonAsync<ScheduledTask>(
                $"/api/scheduler/tasks/{queued.Id}",
                JsonOptions,
                CancellationToken.None);
            if (current?.Status == ScheduledTaskStatus.Completed)
            {
                break;
            }

            await Task.Delay(20);
        }

        Assert.Equal(ScheduledTaskStatus.Completed, current?.Status);
        Assert.NotNull(current);
        Assert.NotNull(current?.WorkflowRunId);
        Assert.NotNull(current?.LastHeartbeatAt);
        Assert.Null(current?.LeaseExpiresAt);

        var evidence = await client.GetFromJsonAsync<WorkflowEvidenceBundle>(
            $"/api/workflows/{current!.WorkflowRunId}/evidence",
            JsonOptions,
            CancellationToken.None);
        Assert.NotNull(evidence);
        Assert.Equal(AgentExecutionStatus.Completed, Assert.Single(evidence!.AgentExecutions).Status);
        Assert.Contains(evidence.EvidenceItems, item => item.Kind == EvidenceKind.Rationale);
        Assert.Contains(evidence.Artifacts, item => item.Type == "ExecutionPlan");

        var history = await client.GetFromJsonAsync<TaskActivityHistory>(
            $"/api/tasks/{current.TaskId}/history",
            JsonOptions,
            CancellationToken.None);
        Assert.NotNull(history);
        Assert.NotEmpty(history.Items);
        Assert.Contains(history.Items, item => item.Category == TaskActivityCategory.Workflow);
        Assert.Contains(history.Items, item => item.Category == TaskActivityCategory.Agent);
        Assert.Contains(history.Items, item => item.Category == TaskActivityCategory.Evidence);
        Assert.Contains(history.Items, item => item.Category == TaskActivityCategory.Artifact);
        Assert.Equal(
            history.Items.OrderBy(item => item.Sequence).Select(item => item.Sequence),
            history.Items.Select(item => item.Sequence));

        var legacyEvents = await client.GetFromJsonAsync<IReadOnlyList<WorkflowEvent>>(
            $"/api/workflows/{current.WorkflowRunId}/events",
            JsonOptions,
            CancellationToken.None);
        Assert.NotEmpty(legacyEvents!);

        var queryReplay = await ReadFirstSseSequenceAsync(
            client,
            current.TaskId,
            queryCursor: history.Items[0].Sequence,
            lastEventId: null);
        Assert.True(queryReplay > history.Items[0].Sequence);

        var headerCursor = history.Items[1].Sequence;
        var reconnectReplay = await ReadFirstSseSequenceAsync(
            client,
            current.TaskId,
            queryCursor: 0,
            lastEventId: headerCursor);
        Assert.True(reconnectReplay > headerCursor);
    }

    private static async Task<long> ReadFirstSseSequenceAsync(
        HttpClient client,
        string taskId,
        long queryCursor,
        long? lastEventId)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/tasks/{taskId}/activity?afterSequence={queryCursor}&replayLimit=10");
        if (lastEventId is { } cursor)
        {
            request.Headers.TryAddWithoutValidation("Last-Event-ID", cursor.ToString());
        }

        using var response = await client.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellation.Token);
        Assert.Equal("text/event-stream", response.Content.Headers.ContentType?.MediaType);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellation.Token);
        using var reader = new StreamReader(stream);
        while (!cancellation.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellation.Token);
            if (line?.StartsWith("id: ", StringComparison.Ordinal) == true)
            {
                return long.Parse(line[4..]);
            }
        }

        throw new TimeoutException("No SSE activity event was received.");
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
