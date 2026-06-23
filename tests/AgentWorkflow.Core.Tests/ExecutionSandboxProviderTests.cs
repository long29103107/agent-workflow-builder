using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class ExecutionSandboxProviderTests
{
    [Fact]
    public async Task ProvisionAndDestroy_RecordLifecycleEvents()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);

        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest(
                "workspace-alpha",
                "project-alpha",
                "https://github.com/example/repo",
                "main",
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var destroyed = await provider.DestroyAsync(
            new SandboxDestroyRequest(
                new SandboxActionContext(lease.Id, lease.WorkspaceId),
                "done"),
            CancellationToken.None);

        Assert.Equal("workspace-alpha", lease.WorkspaceId);
        Assert.Equal(SandboxLeaseStatus.Destroyed, destroyed.Status);
        Assert.NotNull(destroyed.DestroyedAt);
        Assert.Collection(
            provider.GetLifecycleEvents(lease.Id),
            item => Assert.Equal(SandboxLifecycleEventType.Provisioned, item.Type),
            item => Assert.Equal(SandboxLifecycleEventType.Destroyed, item.Type));
    }

    [Fact]
    public async Task ActionsRequireTheMatchingWorkspaceLease()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(
                    new SandboxActionContext(lease.Id, "workspace-beta"),
                    "dotnet",
                    ["test"],
                    ".",
                    TimeSpan.FromMinutes(5)),
                CancellationToken.None));
    }

    [Fact]
    public async Task CodeCommandGitAndArtifactActionsAreWorkspaceScoped()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);
        var context = new SandboxActionContext(lease.Id, lease.WorkspaceId);

        var code = await provider.ApplyCodeAsync(
            new SandboxCodeActionRequest(context, "update endpoint", ["src/Program.cs"]),
            CancellationToken.None);
        var command = await provider.ExecuteCommandAsync(
            new SandboxCommandActionRequest(context, "dotnet", ["test"], ".", TimeSpan.FromMinutes(5)),
            CancellationToken.None);
        var git = await provider.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Commit, ["-m", "test"]),
            CancellationToken.None);
        var artifact = await provider.CaptureArtifactAsync(
            new SandboxArtifactRequest(context, "test-results", "artifacts/test.log", "text/plain"),
            CancellationToken.None);

        Assert.Equal(context, code.Context);
        Assert.Equal(context, command.Context);
        Assert.Equal("mock:workspace-alpha:dotnet test", command.StandardOutput);
        Assert.Equal(context, git.Context);
        Assert.Equal(context, artifact.Context);
        Assert.Single(provider.GetArtifacts(lease.Id));
        Assert.Equal(5, provider.GetLifecycleEvents(lease.Id).Count);
    }
}
