using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Tests;

public sealed class WorkspaceEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task WorkspaceFlow_CreatesRequestApprovesPlanAndProcessesTask()
    {
        await using var factory = new AgentWorkflowApiFactory();
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
        Assert.Equal(ApprovalGate.InvestigationPlan, approval.Approval.Gate);
        Assert.Equal(ApprovalStatus.Approved, approval.Approval.Status);
        Assert.NotNull(approval.Approval.Binding.ArtifactHash);
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
        await using var factory = new AgentWorkflowApiFactory();
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

    [Fact]
    public async Task PlannerEditAndKanbanAssignment_UseEnabledProjectAgents()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();
        var workspace = (await client.GetFromJsonAsync<IReadOnlyList<WorkspaceProject>>(
            "/api/workspaces",
            JsonOptions,
            CancellationToken.None))!.Single();
        var agents = await client.GetFromJsonAsync<IReadOnlyList<string>>(
            $"/api/workspaces/{workspace.Id}/agents",
            JsonOptions,
            CancellationToken.None);

        Assert.NotNull(agents);
        Assert.True(agents.Count >= 2);

        var requestResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/requests",
            new CreateWorkspaceUserRequest("Build editable planner"),
            CancellationToken.None);
        var submission = await requestResponse.Content.ReadFromJsonAsync<RequestSubmissionResult>(
            JsonOptions,
            CancellationToken.None);
        var editedSteps = submission!.PlannerLog.Steps
            .Select((step, index) => index == 0
                ? step with
                {
                    Title = "Edited plan step",
                    Owner = agents[0]
                }
                : step)
            .ToList();

        var editResponse = await client.PutAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/planner/logs/{submission.PlannerLog.Id}",
            new UpdatePlannerLogRequest(editedSteps),
            CancellationToken.None);
        var edited = await editResponse.Content.ReadFromJsonAsync<PlannerLog>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, editResponse.StatusCode);
        Assert.Equal("Edited plan step", edited!.Steps[0].Title);
        Assert.Equal(agents[0], edited.Steps[0].Owner);

        var approveResponse = await client.PostAsync(
            $"/api/workspaces/{workspace.Id}/planner/logs/{submission.PlannerLog.Id}/approve",
            content: null,
            cancellationToken: CancellationToken.None);
        var approval = await approveResponse.Content.ReadFromJsonAsync<PlannerApprovalResult>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(agents[0], approval!.Tasks[0].AssignedAgent);

        var assignResponse = await client.PutAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/tasks/jira-awb-101/agent",
            new AssignTaskAgentRequest(agents[1]),
            CancellationToken.None);
        var assignedTask = await assignResponse.Content.ReadFromJsonAsync<TaskItem>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, assignResponse.StatusCode);
        Assert.Equal(agents[1], assignedTask!.AssignedAgent);

        var enqueueResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspace.Id}/scheduler/tasks",
            new ScheduleTaskRequest("jira-awb-101", null, null, null, workspace.Id),
            CancellationToken.None);
        var scheduled = await enqueueResponse.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(agents[1], scheduled!.AssignedAgent);
    }

    [Fact]
    public async Task PlannerApproval_UsesProjectCodeAndIncrementsTaskNumbersPerProject()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();
        var defaultWorkspace = (await client.GetFromJsonAsync<IReadOnlyList<WorkspaceProject>>(
            "/api/workspaces",
            JsonOptions,
            CancellationToken.None))!.Single();

        var firstTasks = await SubmitAndApproveAsync(client, defaultWorkspace.Id, "First plan");
        var secondTasks = await SubmitAndApproveAsync(client, defaultWorkspace.Id, "Second plan");

        Assert.Equal("AWB-1", firstTasks[0].Key);
        Assert.Equal($"AWB-{firstTasks.Count + 1}", secondTasks[0].Key);

        var createResponse = await client.PostAsJsonAsync(
            "/api/workspaces",
            new CreateWorkspaceRequest("Beta Project", null, null, "github", "beta"),
            CancellationToken.None);
        var beta = await createResponse.Content.ReadFromJsonAsync<WorkspaceProject>(
            JsonOptions,
            CancellationToken.None);
        var betaTasks = await SubmitAndApproveAsync(client, beta!.Id, "Beta plan");

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal("BETA", beta.Code);
        Assert.Equal("BETA-1", betaTasks[0].Key);
    }

    [Fact]
    public async Task WorkspaceScheduler_CanProcessTheExactDraggedTodoItem()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();
        var workspace = (await client.GetFromJsonAsync<IReadOnlyList<WorkspaceProject>>(
            "/api/workspaces",
            JsonOptions,
            CancellationToken.None))!.Single();
        var tasks = await SubmitAndApproveAsync(client, workspace.Id, "Drag exact task");
        var first = await EnqueueAsync(client, workspace.Id, tasks[0].Id);
        var second = await EnqueueAsync(client, workspace.Id, tasks[1].Id);

        var processResponse = await client.PostAsync(
            $"/api/workspaces/{workspace.Id}/scheduler/tasks/{second.Id}/process",
            content: null,
            cancellationToken: CancellationToken.None);
        var processed = await processResponse.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            CancellationToken.None);
        var scheduled = await client.GetFromJsonAsync<IReadOnlyList<ScheduledTask>>(
            $"/api/workspaces/{workspace.Id}/scheduler/tasks",
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, processResponse.StatusCode);
        Assert.Equal(second.Id, processed!.Id);
        Assert.Equal(ScheduledTaskStatus.Completed, processed.Status);
        Assert.Equal(ScheduledTaskStatus.Queued, scheduled!.Single(item => item.Id == first.Id).Status);
    }

    private static async Task<ScheduledTask> EnqueueAsync(
        HttpClient client,
        string workspaceId,
        string taskId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/scheduler/tasks",
            new ScheduleTaskRequest(taskId, null, null, null, workspaceId),
            CancellationToken.None);
        return (await response.Content.ReadFromJsonAsync<ScheduledTask>(
            JsonOptions,
            CancellationToken.None))!;
    }

    private static async Task<IReadOnlyList<TaskItem>> SubmitAndApproveAsync(
        HttpClient client,
        string workspaceId,
        string content)
    {
        var requestResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/requests",
            new CreateWorkspaceUserRequest(content),
            CancellationToken.None);
        var submission = await requestResponse.Content.ReadFromJsonAsync<RequestSubmissionResult>(
            JsonOptions,
            CancellationToken.None);
        var approveResponse = await client.PostAsync(
            $"/api/workspaces/{workspaceId}/planner/logs/{submission!.PlannerLog.Id}/approve",
            content: null,
            cancellationToken: CancellationToken.None);
        var approval = await approveResponse.Content.ReadFromJsonAsync<PlannerApprovalResult>(
            JsonOptions,
            CancellationToken.None);
        return approval!.Tasks;
    }

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
