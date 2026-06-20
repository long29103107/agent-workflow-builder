using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Domain;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AgentWorkflow.Api.Tests;

public sealed class WorkspaceEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task WorkspaceFlow_CreatesRequestApprovesPlanAndProcessesTask()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var workspaces = await client.GetFromJsonAsync<IReadOnlyList<WorkspaceProject>>(
            "/api/workspaces",
            JsonOptions,
            CancellationToken.None);
        var workspace = Assert.Single(workspaces!);

        var requestResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/requests",
            new CreateWorkspaceUserRequest("Build workspace API integration"),
            CancellationToken.None);
        var submission = await requestResponse.Content.ReadFromJsonAsync<RequestSubmissionResult>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, requestResponse.StatusCode);
        Assert.NotNull(submission);
        Assert.Equal(PlannerLogStatus.PendingApproval, submission.PlannerLog.Status);

        var approveResponse = await client.PostAsync(
            $"/api/workspaces/{workspace.Id}/planner/logs/{submission.PlannerLog.Id}/approve",
            content: null,
            cancellationToken: CancellationToken.None);
        var approval = await approveResponse.Content.ReadFromJsonAsync<PlannerApprovalResult>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);
        Assert.NotNull(approval);
        Assert.Equal(PlannerLogStatus.Approved, approval.PlannerLog.Status);
        Assert.NotEmpty(approval.Tasks);

        var task = approval.Tasks[0];
        var enqueueResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/scheduler/tasks",
            new ScheduleTaskRequest(task.Id, null, null, null, workspace.Id),
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.Created, enqueueResponse.StatusCode);

        var processResponse = await client.PostAsync(
            $"/api/workspaces/{workspace.Id}/scheduler/process-next",
            content: null,
            cancellationToken: CancellationToken.None);
        var processed = await processResponse.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);
        Assert.NotNull(processed);
        Assert.Equal(workspace.Id, processed.WorkspaceId);
        Assert.Equal(ScheduledTaskStatus.Completed, processed.Status);
    }

    [Fact]
    public async Task WorkspaceQueues_AreIsolated()
    {
        await using var factory = new WebApplicationFactory<Program>();
        using var client = factory.CreateClient();

        var first = (await client.GetFromJsonAsync<IReadOnlyList<WorkspaceProject>>(
            "/api/workspaces",
            JsonOptions,
            CancellationToken.None))!.Single();
        var createResponse = await client.PostAsJsonAsync(
            "/api/workspaces",
            new CreateWorkspaceRequest("Project Beta", null, null, "github"),
            CancellationToken.None);
        var second = await createResponse.Content.ReadFromJsonAsync<WorkspaceProject>(
            JsonOptions,
            CancellationToken.None);

        await client.PostAsJsonAsync(
            $"/api/workspaces/{first.Id}/scheduler/tasks",
            new ScheduleTaskRequest("jira-awb-101", null, null, null, first.Id),
            CancellationToken.None);

        var secondQueue = await client.GetFromJsonAsync<IReadOnlyList<ScheduledTask>>(
            $"/api/workspaces/{second!.Id}/scheduler/tasks",
            JsonOptions,
            CancellationToken.None);

        Assert.Empty(secondQueue!);
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
