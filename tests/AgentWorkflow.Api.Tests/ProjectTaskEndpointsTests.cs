using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Api.Tests;

public sealed class ProjectTaskEndpointsTests
{
    private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

    [Fact]
    public async Task ProjectCrud_CreatesUpdatesAndDeletesNonDefaultProject()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();
        var template = await GetDefaultProjectAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            "/api/projects",
            ToCreateRequest(template, "API Project"),
            CancellationToken.None);
        var created = await createResponse.Content.ReadFromJsonAsync<Project>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);
        Assert.Equal("AP", created.Code);

        var updateResponse = await client.PutAsJsonAsync(
            $"/api/projects/{created.Id}",
            ToUpdateRequest(created, "Updated API Project"),
            CancellationToken.None);
        var updated = await updateResponse.Content.ReadFromJsonAsync<Project>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        Assert.Equal("Updated API Project", updated!.Name);
        Assert.Equal("AP", updated.Code);

        var duplicateResponse = await client.PostAsJsonAsync(
            "/api/projects",
            ToCreateRequest(template, "Another Project") with { Code = created.Code },
            CancellationToken.None);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        var deleteResponse = await client.DeleteAsync(
            $"/api/projects/{created.Id}",
            CancellationToken.None);
        var getResponse = await client.GetAsync(
            $"/api/projects/{created.Id}",
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task ProjectTaskFlow_ReturnsLifecycleAndLinkedWorkItems()
    {
        await using var factory = new AgentWorkflowApiFactory();
        using var client = factory.CreateClient();
        var project = await GetDefaultProjectAsync(client);

        var createResponse = await client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/tasks",
            new CreateProjectTaskRequest(
                "Implement project task API",
                "Expose task lifecycle and provider links.",
                ScheduledTaskPriority.High,
                [new CreateWorkItemRequest(
                    WorkItemSource.Jira,
                    "AWB-200",
                    "Project task API",
                    "Jira source item",
                    "Ready",
                    "High",
                    ["api"])]),
            CancellationToken.None);
        var created = await createResponse.Content.ReadFromJsonAsync<EngineeringTaskDetails>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(created);
        Assert.Equal(project.Id, created.Task.ProjectId);
        Assert.Equal(EngineeringTaskStatus.New, created.Task.Status);
        Assert.Single(created.WorkItems);

        var statusResponse = await client.PatchAsJsonAsync(
            $"/api/projects/{project.Id}/tasks/{created.Task.Id}/status",
            new UpdateEngineeringTaskStatusRequest(EngineeringTaskStatus.Investigating),
            JsonOptions,
            CancellationToken.None);
        var updated = await statusResponse.Content.ReadFromJsonAsync<EngineeringTaskDetails>(
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        Assert.Equal(EngineeringTaskStatus.Investigating, updated!.Task.Status);

        var linkResponse = await client.PostAsJsonAsync(
            $"/api/projects/{project.Id}/tasks/{created.Task.Id}/work-items",
            new CreateWorkItemRequest(
                WorkItemSource.Notion,
                "notion-project-task-api",
                "Project task notes",
                "Notion source item",
                "Draft",
                "Medium",
                ["notion"]),
            CancellationToken.None);
        var details = await client.GetFromJsonAsync<EngineeringTaskDetails>(
            $"/api/projects/{project.Id}/tasks/{created.Task.Id}",
            JsonOptions,
            CancellationToken.None);

        Assert.Equal(HttpStatusCode.Created, linkResponse.StatusCode);
        Assert.NotNull(details);
        Assert.Equal(2, details.WorkItems.Count);
        Assert.Contains(details.WorkItems, item => item.Source == WorkItemSource.Jira);
        Assert.Contains(details.WorkItems, item => item.Source == WorkItemSource.Notion);
    }

    private static async Task<Project> GetDefaultProjectAsync(HttpClient client)
    {
        var projects = await client.GetFromJsonAsync<IReadOnlyList<Project>>(
            "/api/projects",
            JsonOptions,
            CancellationToken.None);
        return Assert.Single(projects!);
    }

    private static CreateProjectRequest ToCreateRequest(Project project, string name) =>
        new(
            name,
            project.Repository,
            project.GitHub,
            project.Agents,
            project.CodingStandards,
            project.Commands,
            project.BranchPolicy,
            project.ProtectedPaths,
            project.ApprovalPolicy);

    private static UpdateProjectRequest ToUpdateRequest(Project project, string name) =>
        new(
            name,
            project.Repository,
            project.GitHub,
            project.Agents,
            project.CodingStandards,
            project.Commands,
            project.BranchPolicy,
            project.ProtectedPaths,
            project.ApprovalPolicy);

    private static JsonSerializerOptions CreateJsonOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }
}
