using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class ProjectPolicyTests
{
    [Fact]
    public void Validate_DefaultPolicy_IsValid()
    {
        var validator = new ProjectPolicyValidator();
        var request = CreateValidRequest();

        var errors = validator.Validate(request);

        Assert.Empty(errors);
    }

    [Fact]
    public void Validate_UnsafePolicies_ReturnsActionableErrors()
    {
        var validator = new ProjectPolicyValidator();
        var valid = CreateValidRequest();
        var request = valid with
        {
            Commands = valid.Commands with { Test = [], TimeoutSeconds = 7200 },
            BranchPolicy = valid.BranchPolicy with { AllowForcePush = true },
            ProtectedPaths = new ProjectProtectedPathPolicy(
                ["../outside", @"C:\secrets"],
                BlockProductionEnvironmentFiles: false),
            ApprovalPolicy = valid.ApprovalPolicy with
            {
                RequireImplementationApproval = false
            }
        };

        var errors = validator.Validate(request);

        Assert.Contains(errors, error => error.Field == "commands.test");
        Assert.Contains(errors, error => error.Field == "commands.timeoutSeconds");
        Assert.Contains(errors, error => error.Field == "branchPolicy.allowForcePush");
        Assert.Contains(errors, error => error.Field == "protectedPaths.paths");
        Assert.Contains(errors, error => error.Field == "protectedPaths.blockProductionEnvironmentFiles");
        Assert.Contains(errors, error => error.Field == "approvalPolicy");
    }

    [Fact]
    public async Task ProjectStore_SeedsDefaultProjectWithRequiredPolicies()
    {
        var store = new InMemoryProjectStore();

        var projects = await store.GetProjectsAsync(CancellationToken.None);
        var project = Assert.Single(projects);

        Assert.Equal(ProjectPolicyDefaults.DefaultProjectId, project.Id);
        Assert.NotEmpty(project.Commands.Build);
        Assert.NotEmpty(project.Commands.Test);
        Assert.False(project.BranchPolicy.AllowForcePush);
        Assert.True(project.ApprovalPolicy.RequireInvestigationPlanApproval);
        Assert.True(project.ApprovalPolicy.RequireImplementationApproval);
        Assert.True(project.ApprovalPolicy.RequirePullRequestApproval);
        Assert.True(project.ApprovalPolicy.RequireMergeApproval);
    }

    [Fact]
    public async Task WorkspaceStore_CreatesProjectBackedWorkspace()
    {
        var defaults = new WorkspaceDefaults("Project Alpha", ".", "", "github");
        var validator = new ProjectPolicyValidator();
        var projectStore = new InMemoryProjectStore(defaults, validator);
        var workspaceStore = new InMemoryWorkspaceStore(
            projectStore,
            new ToolEndpointSettings("mock://jira", "mock://notion", ".", "", "github"));

        var workspace = await workspaceStore.CreateWorkspaceAsync(
            new CreateWorkspaceRequest("Project Beta", ".", "", "github"),
            CancellationToken.None);
        var project = await projectStore.GetProjectAsync(workspace.Id, CancellationToken.None);

        Assert.NotNull(project);
        Assert.Equal(workspace.Name, project.Name);
        Assert.Equal(workspace.RepositoryPath, project.Repository.LocalPath);
        Assert.Equal(workspace.RepositoryProvider, project.Repository.Provider);
    }

    [Fact]
    public async Task ProjectStore_RejectsUnsafeProject()
    {
        var store = new InMemoryProjectStore();
        var valid = CreateValidRequest();
        var unsafeRequest = valid with
        {
            BranchPolicy = valid.BranchPolicy with { AllowForcePush = true }
        };

        var exception = await Assert.ThrowsAsync<ProjectPolicyValidationException>(() =>
            store.CreateProjectAsync(unsafeRequest, CancellationToken.None));

        Assert.Contains(exception.Errors, error => error.Field == "branchPolicy.allowForcePush");
    }

    private static CreateProjectRequest CreateValidRequest() =>
        ProjectPolicyDefaults.Create(new WorkspaceDefaults(
            "Project Test",
            ".",
            "",
            "github"));
}
