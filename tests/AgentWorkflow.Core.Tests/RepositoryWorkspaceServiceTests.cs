using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using AgentWorkflow.Core.Infrastructure;

namespace AgentWorkflow.Core.Tests;

public sealed class RepositoryWorkspaceServiceTests
{
    [Fact]
    public void GitHubAuthenticator_AppliesTokenOnlyToCloneUrl()
    {
        var authenticator = new GitHubRepositoryAuthenticator();
        var connection = new RepositoryConnection(
            "github",
            "https://github.com/example/repo.git",
            null,
            "example",
            "repo",
            "main",
            "Connected",
            "ready");

        var target = authenticator.CreateCloneTarget(connection, "ghp_secretToken");

        Assert.Equal("github-token", target.AuthenticationMode);
        Assert.Contains("ghp_secretToken", target.CloneUrl);
        Assert.DoesNotContain("ghp_secretToken", target.DisplayUrl);
        Assert.Equal("https://github.com/example/repo.git", target.DisplayUrl);
    }

    [Fact]
    public async Task CloneAsync_ClonesIntoSandboxAndDetectsMetadata()
    {
        var sandbox = new FakeSandboxProvider();
        var service = new RepositoryWorkspaceService(sandbox, new GitHubRepositoryAuthenticator());
        var connection = new RepositoryConnection(
            "github",
            "https://github.com/example/repo.git",
            null,
            "example",
            "repo",
            "main",
            "Connected",
            "ready");

        var workspace = await service.CloneAsync(
            new RepositoryCloneRequest(
                "workspace-alpha",
                "project-alpha",
                connection,
                TimeSpan.FromMinutes(30),
                "ghp_secretToken"),
            CancellationToken.None);

        Assert.Equal("workspace-alpha", workspace.Lease.WorkspaceId);
        Assert.Equal("main", workspace.Metadata.DefaultBranch);
        Assert.Equal("abc123", workspace.Metadata.BaseSha);
        Assert.Equal(".NET, Node, Docker Compose", workspace.Metadata.ProjectType);
        Assert.Contains("AgentWorkflowBuilder.slnx", workspace.Metadata.ImportantFiles);
        Assert.Contains("src/AgentWorkflow.Core/AgentWorkflow.Core.csproj", workspace.Metadata.ImportantFiles);
        Assert.Equal("Cloned", workspace.Connection.Status);
        Assert.Equal(SandboxGitActionKind.Clone, sandbox.GitActions.Single().Action);
        Assert.DoesNotContain("ghp_secretToken", string.Join(" ", workspace.CloneEvidence.Select(item => item.Summary)));
    }

    [Fact]
    public async Task PrepareBranchAsync_ChecksOutCleanBaseAndCreatesPolicyBranch()
    {
        var sandbox = new FakeSandboxProvider();
        var service = new RepositoryWorkspaceService(sandbox, new GitHubRepositoryAuthenticator());
        var workspace = await service.CloneAsync(
            new RepositoryCloneRequest(
                "workspace-alpha",
                "project-alpha",
                Connection(),
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var preparation = await service.PrepareBranchAsync(
            new RepositoryBranchPreparationRequest(
                workspace,
                new ProjectBranchPolicy("main", "agent/", AllowForcePush: false),
                "AWB-123_Checkout Base"),
            CancellationToken.None);

        Assert.Equal("abc123", preparation.BaseSha);
        Assert.Equal("agent/awb-123-checkout-base", preparation.BranchName);
        Assert.Equal(
            [
                SandboxGitActionKind.Clone,
                SandboxGitActionKind.Fetch,
                SandboxGitActionKind.Checkout,
                SandboxGitActionKind.Branch,
                SandboxGitActionKind.Checkout
            ],
            sandbox.GitActions.Select(action => action.Action).ToArray());
        Assert.Contains(sandbox.Commands, command => command.Arguments.SequenceEqual(["clean", "-fdx"]));
        Assert.Contains(sandbox.Commands, command => command.Arguments.SequenceEqual(["reset", "--hard", "abc123"]));
        Assert.Equal(["-f", "agent/awb-123-checkout-base", "abc123"], sandbox.GitActions[^2].Arguments);
        Assert.Equal(["agent/awb-123-checkout-base"], sandbox.GitActions[^1].Arguments);
    }

    [Fact]
    public async Task PrepareBranchAsync_RejectsDefaultBranchMutation()
    {
        var sandbox = new FakeSandboxProvider();
        var service = new RepositoryWorkspaceService(sandbox, new GitHubRepositoryAuthenticator());
        var workspace = await service.CloneAsync(
            new RepositoryCloneRequest(
                "workspace-alpha",
                "project-alpha",
                Connection(),
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.PrepareBranchAsync(
                new RepositoryBranchPreparationRequest(
                    workspace,
                    new ProjectBranchPolicy("main", "", AllowForcePush: false),
                    "main"),
                CancellationToken.None));
    }

    [Fact]
    public async Task FinalizeAsync_CapturesWorkspaceArtifactsAndDestroysSuccessfulWorkspace()
    {
        var sandbox = new FakeSandboxProvider();
        var service = new RepositoryWorkspaceService(sandbox, new GitHubRepositoryAuthenticator());
        var workspace = await service.CloneAsync(
            new RepositoryCloneRequest(
                "workspace-alpha",
                "project-alpha",
                Connection(),
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var finalization = await service.FinalizeAsync(
            new RepositoryWorkspaceFinalizationRequest(
                workspace,
                Succeeded: true,
                new SandboxArtifactRetentionPolicy(MaxArtifactCount: 6),
                ["artifacts/test-results.trx", "artifacts/build.log", "artifacts/extra.zip"]),
            CancellationToken.None);

        Assert.Equal(SandboxLeaseStatus.Destroyed, finalization.Lease.Status);
        Assert.Equal(
            [
                "repository-metadata",
                "repository-diff",
                "repository-status",
                "repository-log",
                "test-results.trx",
                "build.log"
            ],
            finalization.Artifacts.Select(artifact => artifact.Name).ToArray());
        Assert.Contains(sandbox.Commands, command =>
            command.Command == "sh" &&
            command.Arguments[0] == "-c" &&
            command.Arguments[1].Contains("repository-metadata.json", StringComparison.Ordinal));
        Assert.Single(sandbox.DestroyRequests);
        Assert.False(sandbox.DestroyRequests.Single().Quarantine);
    }

    [Fact]
    public async Task FinalizeAsync_QuarantinesFailedWorkspaceWhenRequested()
    {
        var sandbox = new FakeSandboxProvider();
        var service = new RepositoryWorkspaceService(sandbox, new GitHubRepositoryAuthenticator());
        var workspace = await service.CloneAsync(
            new RepositoryCloneRequest(
                "workspace-alpha",
                "project-alpha",
                Connection(),
                TimeSpan.FromMinutes(30)),
            CancellationToken.None);

        var finalization = await service.FinalizeAsync(
            new RepositoryWorkspaceFinalizationRequest(
                workspace,
                Succeeded: false,
                new SandboxArtifactRetentionPolicy(MaxArtifactCount: 4)),
            CancellationToken.None);

        Assert.Equal(SandboxLeaseStatus.Quarantined, finalization.Lease.Status);
        Assert.True(sandbox.DestroyRequests.Single().Quarantine);
        Assert.Contains(finalization.Evidence, item => item.Type == SandboxLifecycleEventType.Quarantined);
    }

    private static RepositoryConnection Connection() =>
        new(
            "github",
            "https://github.com/example/repo.git",
            null,
            "example",
            "repo",
            "main",
            "Connected",
            "ready");

    private sealed class FakeSandboxProvider : IExecutionSandboxProvider
    {
        private readonly Guid _leaseId = Guid.NewGuid();
        private readonly List<SandboxLifecycleEvent> _events = [];

        public List<SandboxGitActionRequest> GitActions { get; } = [];
        public List<SandboxCommandActionRequest> Commands { get; } = [];
        public List<SandboxArtifact> Artifacts { get; } = [];
        public List<SandboxDestroyRequest> DestroyRequests { get; } = [];

        public Task<SandboxWorkspaceLease> ProvisionAsync(
            SandboxProvisionRequest request,
            CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;
            var lease = new SandboxWorkspaceLease(
                _leaseId,
                request.WorkspaceId,
                request.ProjectId,
                "fake",
                "/workspace",
                SandboxLeaseStatus.Active,
                now,
                now.Add(request.LeaseDuration));
            _events.Add(new SandboxLifecycleEvent(
                Guid.NewGuid(),
                _leaseId,
                request.WorkspaceId,
                SandboxLifecycleEventType.Provisioned,
                now,
                $"Provisioned clone workspace for {request.RepositoryUrl}."));
            return Task.FromResult(lease);
        }

        public Task<SandboxActionResult> ApplyCodeAsync(
            SandboxCodeActionRequest request,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<SandboxActionResult> RunGitAsync(
            SandboxGitActionRequest request,
            CancellationToken cancellationToken)
        {
            GitActions.Add(request);
            _events.Add(new SandboxLifecycleEvent(
                Guid.NewGuid(),
                request.Context.LeaseId,
                request.Context.WorkspaceId,
                SandboxLifecycleEventType.GitActionExecuted,
                DateTimeOffset.UtcNow,
                "Executed git clone."));
            return Task.FromResult(new SandboxActionResult(
                Guid.NewGuid(),
                request.Context,
                "clone complete",
                DateTimeOffset.UtcNow));
        }

        public Task<SandboxCommandResult> ExecuteCommandAsync(
            SandboxCommandActionRequest request,
            CancellationToken cancellationToken)
        {
            Commands.Add(request);
            var output = request.Arguments switch
            {
                ["symbolic-ref", "--short", "refs/remotes/origin/HEAD"] => "origin/main\n",
                ["rev-parse", "HEAD"] => "abc123\n",
                ["ls-files"] => """
                    README.md
                    AgentWorkflowBuilder.slnx
                    src/AgentWorkflow.Core/AgentWorkflow.Core.csproj
                    src/agent-workflow-ui/package.json
                    docker-compose.yml
                    src/AgentWorkflow.Core/Domain/WorkflowModels.cs
                    """,
                _ => ""
            };

            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new SandboxCommandResult(
                Guid.NewGuid(),
                request.Context,
                string.Join(" ", new[] { request.Command }.Concat(request.Arguments)),
                0,
                output,
                "",
                now,
                now,
                TimeSpan.Zero));
        }

        public Task<SandboxArtifact> CaptureArtifactAsync(
            SandboxArtifactRequest request,
            CancellationToken cancellationToken)
        {
            var artifact = new SandboxArtifact(
                Guid.NewGuid(),
                request.Context,
                request.Name,
                request.RelativePath,
                request.ContentType,
                DateTimeOffset.UtcNow);
            Artifacts.Add(artifact);
            _events.Add(new SandboxLifecycleEvent(
                Guid.NewGuid(),
                request.Context.LeaseId,
                request.Context.WorkspaceId,
                SandboxLifecycleEventType.ArtifactCaptured,
                DateTimeOffset.UtcNow,
                $"Captured artifact '{artifact.Name}'."));
            return Task.FromResult(artifact);
        }

        public Task<SandboxWorkspaceLease> DestroyAsync(
            SandboxDestroyRequest request,
            CancellationToken cancellationToken)
        {
            DestroyRequests.Add(request);
            var lease = new SandboxWorkspaceLease(
                _leaseId,
                request.Context.WorkspaceId,
                "project-alpha",
                "fake",
                "/workspace",
                request.Quarantine ? SandboxLeaseStatus.Quarantined : SandboxLeaseStatus.Destroyed,
                DateTimeOffset.UtcNow.AddMinutes(-1),
                DateTimeOffset.UtcNow.AddMinutes(29),
                DateTimeOffset.UtcNow);
            _events.Add(new SandboxLifecycleEvent(
                Guid.NewGuid(),
                request.Context.LeaseId,
                request.Context.WorkspaceId,
                request.Quarantine ? SandboxLifecycleEventType.Quarantined : SandboxLifecycleEventType.Destroyed,
                DateTimeOffset.UtcNow,
                request.Reason));
            return Task.FromResult(lease);
        }

        public IReadOnlyList<SandboxLifecycleEvent> GetLifecycleEvents(Guid leaseId) => _events.ToList();

        public IReadOnlyList<SandboxArtifact> GetArtifacts(Guid leaseId) => [];
    }
}
