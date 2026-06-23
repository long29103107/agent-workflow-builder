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
    public async Task Destroy_CanQuarantineWorkspaceAfterFailure()
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

        var quarantined = await provider.DestroyAsync(
            new SandboxDestroyRequest(
                new SandboxActionContext(lease.Id, lease.WorkspaceId),
                "failed",
                Quarantine: true),
            CancellationToken.None);

        Assert.Equal(SandboxLeaseStatus.Quarantined, quarantined.Status);
        Assert.Collection(
            provider.GetLifecycleEvents(lease.Id),
            item => Assert.Equal(SandboxLifecycleEventType.Provisioned, item.Type),
            item => Assert.Equal(SandboxLifecycleEventType.Quarantined, item.Type));
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

    [Fact]
    public async Task MockProvider_RejectsPathsOutsideWorkspaceRoot()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);
        var context = new SandboxActionContext(lease.Id, lease.WorkspaceId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ApplyCodeAsync(
                new SandboxCodeActionRequest(context, "escape", ["../outside.txt"]),
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(context, "dotnet", ["test"], "../outside", TimeSpan.FromMinutes(5)),
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.CaptureArtifactAsync(
                new SandboxArtifactRequest(context, "secret", "/etc/passwd", "text/plain"),
                CancellationToken.None));
    }

    [Fact]
    public async Task MockProvider_RejectsProtectedPaths()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);
        var context = new SandboxActionContext(
            lease.Id,
            lease.WorkspaceId,
            new SandboxWorkspacePolicy(["src/Production", ".env"]));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ApplyCodeAsync(
                new SandboxCodeActionRequest(context, "touch protected file", ["src/Production/settings.json"]),
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(context, "dotnet", ["test"], ".env", TimeSpan.FromMinutes(5)),
                CancellationToken.None));
    }

    [Fact]
    public async Task MockProvider_RequiresExplicitPolicyForExternalWritesAndDeployments()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);
        var context = new SandboxActionContext(lease.Id, lease.WorkspaceId, new SandboxWorkspacePolicy([]));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.RunGitAsync(
                new SandboxGitActionRequest(context, SandboxGitActionKind.Push, ["origin", "agent/test"]),
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(context, "kubectl", ["apply", "-f", "deployment.yaml"], ".", TimeSpan.FromMinutes(5)),
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(context, "git", ["push", "origin", "agent/test"], ".", TimeSpan.FromMinutes(5)),
                CancellationToken.None));
    }

    [Fact]
    public async Task MockProvider_AllowsExternalWritesAndDeploymentsWithExplicitPolicy()
    {
        var provider = new MockExecutionSandboxProvider(TimeProvider.System);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);
        var context = new SandboxActionContext(
            lease.Id,
            lease.WorkspaceId,
            new SandboxWorkspacePolicy([], AllowExternalWriteAccess: true, AllowDeploymentCommands: true));

        var push = await provider.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Push, ["origin", "agent/test"]),
            CancellationToken.None);
        var deploy = await provider.ExecuteCommandAsync(
            new SandboxCommandActionRequest(context, "kubectl", ["apply", "-f", "deployment.yaml"], ".", TimeSpan.FromMinutes(5)),
            CancellationToken.None);

        Assert.Equal(context, push.Context);
        Assert.Equal(0, deploy.ExitCode);
    }

    [Fact]
    public async Task LocalDockerProvider_ProvisionsContainerWithLimitsAndNoNetwork()
    {
        var runner = new FakeDockerCliRunner();
        var provider = CreateDockerProvider(runner);

        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest(
                "workspace-alpha",
                "project-alpha",
                "https://github.com/example/repo",
                "main",
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        Assert.Equal("docker", lease.Provider);
        Assert.Equal(SandboxLeaseStatus.Active, lease.Status);
        Assert.Equal("create", runner.Commands[0][0]);
        Assert.Contains("--cpus", runner.Commands[0]);
        Assert.Contains("1", runner.Commands[0]);
        Assert.Contains("--memory", runner.Commands[0]);
        Assert.Contains("1024m", runner.Commands[0]);
        Assert.Contains("--network", runner.Commands[0]);
        Assert.Contains("none", runner.Commands[0]);
        Assert.Equal("start", runner.Commands[1][0]);
        Assert.StartsWith("awb-workspace-alpha-", runner.Commands[1][1]);
    }

    [Fact]
    public async Task LocalDockerProvider_RedactsCommandOutput()
    {
        var runner = new FakeDockerCliRunner
        {
            NextOutput = "api_key=sk-secret123",
            NextError = "Bearer abc.def"
        };
        var provider = CreateDockerProvider(runner);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var result = await provider.ExecuteCommandAsync(
            new SandboxCommandActionRequest(
                new SandboxActionContext(lease.Id, lease.WorkspaceId),
                "dotnet",
                ["test"],
                ".",
                TimeSpan.FromMinutes(5)),
            CancellationToken.None);

        Assert.Contains("[REDACTED]", result.StandardOutput);
        Assert.Contains("[REDACTED]", result.StandardError);
        Assert.DoesNotContain("sk-secret123", result.StandardOutput);
        Assert.DoesNotContain("abc.def", result.StandardError);
        Assert.Contains("exec", runner.Commands.Last());
    }

    [Fact]
    public async Task LocalDockerProvider_RejectsCredentialEnvironment()
    {
        var provider = CreateDockerProvider(new FakeDockerCliRunner());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ProvisionAsync(
                new SandboxProvisionRequest(
                    "workspace-alpha",
                    "project-alpha",
                    "",
                    "main",
                    TimeSpan.FromMinutes(30),
                    Environment: new Dictionary<string, string> { ["GITHUB_TOKEN"] = "secret" }),
                CancellationToken.None));
    }

    [Fact]
    public async Task LocalDockerProvider_RejectsWritableProtectedMount()
    {
        var provider = CreateDockerProvider(new FakeDockerCliRunner());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ProvisionAsync(
                new SandboxProvisionRequest(
                    "workspace-alpha",
                    "project-alpha",
                    "",
                    "main",
                    TimeSpan.FromMinutes(30),
                    Mounts: [new SandboxMount("C:/Users/example/.ssh", "/workspace/.ssh", Writable: true)]),
                CancellationToken.None));
    }

    [Fact]
    public async Task LocalDockerProvider_RejectsUnsafeWorkingDirectoryBeforeDockerExec()
    {
        var runner = new FakeDockerCliRunner();
        var provider = CreateDockerProvider(runner);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.ExecuteCommandAsync(
                new SandboxCommandActionRequest(
                    new SandboxActionContext(lease.Id, lease.WorkspaceId),
                    "dotnet",
                    ["test"],
                    "../outside",
                    TimeSpan.FromMinutes(5)),
                CancellationToken.None));

        Assert.Equal(2, runner.Commands.Count);
    }

    [Fact]
    public async Task LocalDockerProvider_SanitizesArtifactNameBeforeHostCopy()
    {
        var runner = new FakeDockerCliRunner();
        var provider = CreateDockerProvider(runner);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        await provider.CaptureArtifactAsync(
            new SandboxArtifactRequest(
                new SandboxActionContext(lease.Id, lease.WorkspaceId),
                "../outside.log",
                "artifacts/test.log",
                "text/plain"),
            CancellationToken.None);

        var destination = runner.Commands.Last()[2];
        Assert.DoesNotContain("..", destination);
        Assert.EndsWith($"{Path.DirectorySeparatorChar}outside.log", destination);
    }

    [Fact]
    public async Task LocalDockerProvider_QuarantineStopsContainerWithoutRemovingIt()
    {
        var runner = new FakeDockerCliRunner();
        var provider = CreateDockerProvider(runner);
        var lease = await provider.ProvisionAsync(
            new SandboxProvisionRequest("workspace-alpha", "project-alpha", "", "main", TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var quarantined = await provider.DestroyAsync(
            new SandboxDestroyRequest(
                new SandboxActionContext(lease.Id, lease.WorkspaceId),
                "failed",
                Quarantine: true),
            CancellationToken.None);

        Assert.Equal(SandboxLeaseStatus.Quarantined, quarantined.Status);
        Assert.Equal("stop", runner.Commands.Last()[0]);
    }

    private static LocalDockerExecutionSandboxProvider CreateDockerProvider(FakeDockerCliRunner runner) =>
        new(
            runner,
            new DockerSandboxOptions
            {
                Image = "example/sdk:test",
                WorkspaceRoot = "/workspace",
                ArtifactRootPath = Path.Combine(Path.GetTempPath(), "awb-tests")
            },
            new SecretRedactor(),
            TimeProvider.System);

    private sealed class FakeDockerCliRunner : IDockerCliRunner
    {
        public List<IReadOnlyList<string>> Commands { get; } = [];
        public string NextOutput { get; set; } = "";
        public string NextError { get; set; } = "";

        public Task<DockerCliResult> RunAsync(
            IReadOnlyList<string> arguments,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            Commands.Add(arguments.ToList());
            var now = TimeProvider.System.GetUtcNow();
            return Task.FromResult(new DockerCliResult(0, NextOutput, NextError, now, now.AddMilliseconds(12)));
        }
    }
}
