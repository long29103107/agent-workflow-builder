using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class MockExecutionSandboxProvider(TimeProvider timeProvider) : IExecutionSandboxProvider
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, SandboxWorkspaceLease> _leases = [];
    private readonly Dictionary<Guid, List<SandboxLifecycleEvent>> _events = [];
    private readonly Dictionary<Guid, List<SandboxArtifact>> _artifacts = [];
    private int _nextId;

    public Task<SandboxWorkspaceLease> ProvisionAsync(
        SandboxProvisionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectId);
        if (request.LeaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Lease duration must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var now = timeProvider.GetUtcNow();
            var lease = new SandboxWorkspaceLease(
                NextGuid(),
                request.WorkspaceId.Trim(),
                request.ProjectId.Trim(),
                "mock",
                $"/mock-sandboxes/{request.WorkspaceId.Trim()}",
                SandboxLeaseStatus.Active,
                now,
                now.Add(request.LeaseDuration));

            _leases[lease.Id] = lease;
            _events[lease.Id] = [];
            _artifacts[lease.Id] = [];
            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.Provisioned, $"Provisioned mock sandbox for {lease.WorkspaceId}.");

            return Task.FromResult(lease);
        }
    }

    public Task<SandboxActionResult> ApplyCodeAsync(
        SandboxCodeActionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        SandboxPolicyEnforcer.ValidateCodeAction(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var lease = RequireActiveLease(request.Context);
            var result = new SandboxActionResult(
                NextGuid(),
                request.Context,
                $"Applied code action to {request.RelativePaths.Count} file(s) in {lease.WorkspaceId}.",
                timeProvider.GetUtcNow());

            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.CodeApplied, result.Summary);
            return Task.FromResult(result);
        }
    }

    public Task<SandboxActionResult> RunGitAsync(
        SandboxGitActionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        SandboxPolicyEnforcer.ValidateGitAction(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var lease = RequireActiveLease(request.Context);
            var result = new SandboxActionResult(
                NextGuid(),
                request.Context,
                $"Executed git {request.Action.ToString().ToLowerInvariant()} in {lease.WorkspaceId}.",
                timeProvider.GetUtcNow());

            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.GitActionExecuted, result.Summary);
            return Task.FromResult(result);
        }
    }

    public Task<SandboxCommandResult> ExecuteCommandAsync(
        SandboxCommandActionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Command);
        SandboxPolicyEnforcer.ValidateWorkingDirectory(request);
        if (request.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Command timeout must be positive.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var lease = RequireActiveLease(request.Context);
            var now = timeProvider.GetUtcNow();
            var commandLine = string.Join(" ", new[] { request.Command }.Concat(request.Arguments));
            var result = new SandboxCommandResult(
                NextGuid(),
                request.Context,
                commandLine,
                ExitCode: 0,
                StandardOutput: $"mock:{lease.WorkspaceId}:{commandLine}",
                StandardError: string.Empty,
                StartedAt: now,
                CompletedAt: now,
                Runtime: TimeSpan.Zero);

            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.CommandExecuted, $"Executed command '{commandLine}'.");
            return Task.FromResult(result);
        }
    }

    public Task<SandboxArtifact> CaptureArtifactAsync(
        SandboxArtifactRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        var relativePath = SandboxPolicyEnforcer.ValidateArtifactPath(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var lease = RequireActiveLease(request.Context);
            var artifact = new SandboxArtifact(
                NextGuid(),
                request.Context,
                request.Name.Trim(),
                relativePath,
                request.ContentType.Trim(),
                timeProvider.GetUtcNow());

            _artifacts[lease.Id].Add(artifact);
            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.ArtifactCaptured, $"Captured artifact '{artifact.Name}'.");
            return Task.FromResult(artifact);
        }
    }

    public Task<SandboxWorkspaceLease> DestroyAsync(
        SandboxDestroyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_syncRoot)
        {
            var lease = RequireLease(request.Context);
            if (lease.Status is SandboxLeaseStatus.Destroyed or SandboxLeaseStatus.Quarantined)
            {
                return Task.FromResult(lease);
            }

            var destroyed = lease with
            {
                Status = request.Quarantine ? SandboxLeaseStatus.Quarantined : SandboxLeaseStatus.Destroyed,
                DestroyedAt = timeProvider.GetUtcNow()
            };
            _leases[lease.Id] = destroyed;
            AddEvent(
                lease.Id,
                lease.WorkspaceId,
                request.Quarantine ? SandboxLifecycleEventType.Quarantined : SandboxLifecycleEventType.Destroyed,
                string.IsNullOrWhiteSpace(request.Reason)
                    ? request.Quarantine ? "Quarantined mock sandbox." : "Destroyed mock sandbox."
                    : request.Reason.Trim());
            return Task.FromResult(destroyed);
        }
    }

    public IReadOnlyList<SandboxLifecycleEvent> GetLifecycleEvents(Guid leaseId)
    {
        lock (_syncRoot)
        {
            return _events.TryGetValue(leaseId, out var events)
                ? events.ToList()
                : [];
        }
    }

    public IReadOnlyList<SandboxArtifact> GetArtifacts(Guid leaseId)
    {
        lock (_syncRoot)
        {
            return _artifacts.TryGetValue(leaseId, out var artifacts)
                ? artifacts.ToList()
                : [];
        }
    }

    private SandboxWorkspaceLease RequireActiveLease(SandboxActionContext context)
    {
        var lease = RequireLease(context);
        if (lease.Status is not SandboxLeaseStatus.Active)
        {
            throw new InvalidOperationException($"Sandbox lease '{lease.Id}' is not active.");
        }

        return lease;
    }

    private SandboxWorkspaceLease RequireLease(SandboxActionContext context)
    {
        if (context.LeaseId == Guid.Empty)
        {
            throw new ArgumentException("Sandbox action context requires a lease ID.", nameof(context));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(context.WorkspaceId);

        if (!_leases.TryGetValue(context.LeaseId, out var lease))
        {
            throw new KeyNotFoundException($"Sandbox lease '{context.LeaseId}' was not found.");
        }

        if (!string.Equals(lease.WorkspaceId, context.WorkspaceId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Sandbox lease '{lease.Id}' belongs to workspace '{lease.WorkspaceId}', not '{context.WorkspaceId}'.");
        }

        return lease;
    }

    private void AddEvent(Guid leaseId, string workspaceId, SandboxLifecycleEventType type, string summary)
    {
        _events[leaseId].Add(new SandboxLifecycleEvent(
            NextGuid(),
            leaseId,
            workspaceId,
            type,
            timeProvider.GetUtcNow(),
            summary));
    }

    private Guid NextGuid()
    {
        var bytes = new byte[16];
        BitConverter.GetBytes(++_nextId).CopyTo(bytes, 0);
        return new Guid(bytes);
    }
}
