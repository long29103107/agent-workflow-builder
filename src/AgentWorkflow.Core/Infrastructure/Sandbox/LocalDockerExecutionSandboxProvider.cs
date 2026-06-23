using System.Diagnostics;
using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class DockerSandboxOptions
{
    public string Image { get; init; } =
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_SANDBOX_IMAGE") ?? "mcr.microsoft.com/dotnet/sdk:10.0";
    public string WorkspaceRoot { get; init; } =
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_SANDBOX_WORKSPACE_ROOT") ?? "/workspace";
    public string ArtifactRootPath { get; init; } =
        Environment.GetEnvironmentVariable("AGENT_WORKFLOW_SANDBOX_ARTIFACT_ROOT") ??
        Path.Combine(Path.GetTempPath(), "agent-workflow-sandbox-artifacts");
    public SandboxResourceLimits DefaultResourceLimits { get; init; } = new(CpuCount: 1, MemoryMegabytes: 1024);
}

public sealed record DockerCliResult(
    int ExitCode,
    string StandardOutput,
    string StandardError,
    DateTimeOffset StartedAt,
    DateTimeOffset CompletedAt);

public interface IDockerCliRunner
{
    Task<DockerCliResult> RunAsync(
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken);
}

public sealed class DockerCliRunner(TimeProvider timeProvider) : IDockerCliRunner
{
    public async Task<DockerCliResult> RunAsync(
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetUtcNow();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(timeout);

        var startInfo = new ProcessStartInfo
        {
            FileName = "docker",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo) ??
            throw new InvalidOperationException("Failed to start docker process.");

        var stdout = process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
        var stderr = process.StandardError.ReadToEndAsync(timeoutSource.Token);
        await process.WaitForExitAsync(timeoutSource.Token);

        return new DockerCliResult(
            process.ExitCode,
            await stdout,
            await stderr,
            startedAt,
            timeProvider.GetUtcNow());
    }
}

public sealed class LocalDockerExecutionSandboxProvider(
    IDockerCliRunner docker,
    DockerSandboxOptions options,
    ISecretRedactor redactor,
    TimeProvider timeProvider) : IExecutionSandboxProvider
{
    private static readonly string[] SensitiveEnvironmentFragments =
    [
        "KEY",
        "TOKEN",
        "SECRET",
        "PASSWORD",
        "CREDENTIAL"
    ];

    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, DockerLeaseState> _leases = [];
    private readonly Dictionary<Guid, List<SandboxLifecycleEvent>> _events = [];
    private readonly Dictionary<Guid, List<SandboxArtifact>> _artifacts = [];

    public async Task<SandboxWorkspaceLease> ProvisionAsync(
        SandboxProvisionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectId);
        if (request.LeaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Lease duration must be positive.");
        }

        var limits = ValidateLimits(request.ResourceLimits ?? options.DefaultResourceLimits);
        var environment = ValidateEnvironment(request.Environment ?? new Dictionary<string, string>());
        var mounts = ValidateMounts(request.Mounts ?? []);
        var image = string.IsNullOrWhiteSpace(request.Image) ? options.Image : request.Image.Trim();
        var leaseId = Guid.NewGuid();
        var containerName = $"awb-{SanitizeName(request.WorkspaceId)}-{leaseId:N}";
        var now = timeProvider.GetUtcNow();
        var lease = new SandboxWorkspaceLease(
            leaseId,
            request.WorkspaceId.Trim(),
            request.ProjectId.Trim(),
            "docker",
            $"{containerName}:{options.WorkspaceRoot}",
            SandboxLeaseStatus.Active,
            now,
            now.Add(request.LeaseDuration));

        var arguments = new List<string>
        {
            "create",
            "--name",
            containerName,
            "--cpus",
            limits.CpuCount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture),
            "--memory",
            $"{limits.MemoryMegabytes}m",
            "--network",
            request.NetworkMode is SandboxNetworkMode.Bridge ? "bridge" : "none",
            "--workdir",
            options.WorkspaceRoot,
            "--label",
            $"agent-workflow.workspace={lease.WorkspaceId}",
            "--label",
            $"agent-workflow.lease={lease.Id}"
        };

        foreach (var pair in environment.OrderBy(item => item.Key, StringComparer.Ordinal))
        {
            arguments.Add("--env");
            arguments.Add($"{pair.Key}={pair.Value}");
        }

        foreach (var mount in mounts)
        {
            arguments.Add("--mount");
            arguments.Add(mount.Writable
                ? $"type=bind,source={mount.HostPath},target={mount.ContainerPath}"
                : $"type=bind,source={mount.HostPath},target={mount.ContainerPath},readonly");
        }

        arguments.Add(image);
        arguments.Add("sleep");
        arguments.Add(request.LeaseDuration.TotalSeconds.ToString("0", System.Globalization.CultureInfo.InvariantCulture));

        await RunDockerAsync(arguments, TimeSpan.FromMinutes(2), cancellationToken);
        await RunDockerAsync(["start", containerName], TimeSpan.FromMinutes(1), cancellationToken);

        lock (_syncRoot)
        {
            _leases[lease.Id] = new DockerLeaseState(lease, containerName);
            _events[lease.Id] = [];
            _artifacts[lease.Id] = [];
            AddEvent(lease.Id, lease.WorkspaceId, SandboxLifecycleEventType.Provisioned, $"Provisioned Docker sandbox '{containerName}'.");
        }

        return lease;
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
            var state = RequireActiveLease(request.Context);
            var result = new SandboxActionResult(
                Guid.NewGuid(),
                request.Context,
                $"Prepared {request.RelativePaths.Count} code path(s) for Docker sandbox '{state.ContainerName}'.",
                timeProvider.GetUtcNow());
            AddEvent(state.Lease.Id, state.Lease.WorkspaceId, SandboxLifecycleEventType.CodeApplied, result.Summary);
            return Task.FromResult(result);
        }
    }

    public async Task<SandboxActionResult> RunGitAsync(
        SandboxGitActionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        SandboxPolicyEnforcer.ValidateGitAction(request);
        var state = RequireActiveLease(request.Context);
        var gitArgs = new[] { "exec", state.ContainerName, "git", request.Action.ToString().ToLowerInvariant() }
            .Concat(request.Arguments)
            .ToArray();

        var result = await RunDockerAsync(gitArgs, TimeSpan.FromMinutes(5), cancellationToken);
        var actionResult = new SandboxActionResult(
            Guid.NewGuid(),
            request.Context,
            Redact(result.StandardOutput.Trim().Length == 0
                ? $"Executed git {request.Action.ToString().ToLowerInvariant()}."
                : result.StandardOutput.Trim()),
            result.CompletedAt);

        AddEvent(state.Lease.Id, state.Lease.WorkspaceId, SandboxLifecycleEventType.GitActionExecuted, actionResult.Summary);
        return actionResult;
    }

    public async Task<SandboxCommandResult> ExecuteCommandAsync(
        SandboxCommandActionRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Command);
        if (request.Timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Command timeout must be positive.");
        }

        var state = RequireActiveLease(request.Context);
        var workingDirectory = SandboxPolicyEnforcer.ValidateWorkingDirectory(request);
        var commandLine = string.Join(" ", new[] { request.Command }.Concat(request.Arguments));
        var result = await RunDockerAsync(
            new[] { "exec", "--workdir", NormalizeContainerPath(workingDirectory), state.ContainerName, request.Command }
                .Concat(request.Arguments)
                .ToArray(),
            request.Timeout,
            cancellationToken);

        var commandResult = new SandboxCommandResult(
            Guid.NewGuid(),
            request.Context,
            commandLine,
            result.ExitCode,
            Redact(result.StandardOutput),
            Redact(result.StandardError),
            result.StartedAt,
            result.CompletedAt,
            result.CompletedAt - result.StartedAt);

        AddEvent(
            state.Lease.Id,
            state.Lease.WorkspaceId,
            SandboxLifecycleEventType.CommandExecuted,
            $"Executed command '{commandLine}' with exit code {commandResult.ExitCode} in {commandResult.Runtime.TotalMilliseconds:0} ms.");
        return commandResult;
    }

    public async Task<SandboxArtifact> CaptureArtifactAsync(
        SandboxArtifactRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Name);
        var relativePath = SandboxPolicyEnforcer.ValidateArtifactPath(request);

        var state = RequireActiveLease(request.Context);
        var artifact = new SandboxArtifact(
            Guid.NewGuid(),
            request.Context,
            request.Name.Trim(),
            relativePath,
            request.ContentType.Trim(),
            timeProvider.GetUtcNow());

        var destination = Path.Combine(options.ArtifactRootPath, state.Lease.Id.ToString("N"), SanitizeArtifactName(artifact.Name));
        Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? options.ArtifactRootPath);
        await RunDockerAsync(
            ["cp", $"{state.ContainerName}:{CombineContainerPath(options.WorkspaceRoot, artifact.RelativePath)}", destination],
            TimeSpan.FromMinutes(2),
            cancellationToken);

        lock (_syncRoot)
        {
            _artifacts[state.Lease.Id].Add(artifact);
            AddEvent(state.Lease.Id, state.Lease.WorkspaceId, SandboxLifecycleEventType.ArtifactCaptured, $"Captured artifact '{artifact.Name}'.");
        }

        return artifact;
    }

    public async Task<SandboxWorkspaceLease> DestroyAsync(
        SandboxDestroyRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var state = RequireLease(request.Context);
        if (state.Lease.Status is SandboxLeaseStatus.Destroyed or SandboxLeaseStatus.Quarantined)
        {
            return state.Lease;
        }

        if (request.Quarantine)
        {
            await RunDockerAsync(["stop", state.ContainerName], TimeSpan.FromMinutes(1), cancellationToken);
        }
        else
        {
            await RunDockerAsync(["rm", "-f", state.ContainerName], TimeSpan.FromMinutes(1), cancellationToken);
        }

        lock (_syncRoot)
        {
            var destroyed = state.Lease with
            {
                Status = request.Quarantine ? SandboxLeaseStatus.Quarantined : SandboxLeaseStatus.Destroyed,
                DestroyedAt = timeProvider.GetUtcNow()
            };
            _leases[state.Lease.Id] = state with { Lease = destroyed };
            AddEvent(
                destroyed.Id,
                destroyed.WorkspaceId,
                request.Quarantine ? SandboxLifecycleEventType.Quarantined : SandboxLifecycleEventType.Destroyed,
                string.IsNullOrWhiteSpace(request.Reason)
                    ? request.Quarantine ? "Quarantined Docker sandbox." : "Destroyed Docker sandbox."
                    : request.Reason.Trim());
            return destroyed;
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

    private async Task<DockerCliResult> RunDockerAsync(
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var result = await docker.RunAsync(arguments, timeout, cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(Redact($"Docker command failed with exit code {result.ExitCode}: {result.StandardError}"));
        }

        return result with
        {
            StandardOutput = Redact(result.StandardOutput),
            StandardError = Redact(result.StandardError)
        };
    }

    private DockerLeaseState RequireActiveLease(SandboxActionContext context)
    {
        var state = RequireLease(context);
        if (state.Lease.Status is not SandboxLeaseStatus.Active)
        {
            throw new InvalidOperationException($"Sandbox lease '{state.Lease.Id}' is not active.");
        }

        return state;
    }

    private DockerLeaseState RequireLease(SandboxActionContext context)
    {
        if (context.LeaseId == Guid.Empty)
        {
            throw new ArgumentException("Sandbox action context requires a lease ID.", nameof(context));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(context.WorkspaceId);

        lock (_syncRoot)
        {
            if (!_leases.TryGetValue(context.LeaseId, out var state))
            {
                throw new KeyNotFoundException($"Sandbox lease '{context.LeaseId}' was not found.");
            }

            if (!string.Equals(state.Lease.WorkspaceId, context.WorkspaceId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException($"Sandbox lease '{state.Lease.Id}' belongs to workspace '{state.Lease.WorkspaceId}', not '{context.WorkspaceId}'.");
            }

            return state;
        }
    }

    private void AddEvent(Guid leaseId, string workspaceId, SandboxLifecycleEventType type, string summary)
    {
        _events[leaseId].Add(new SandboxLifecycleEvent(
            Guid.NewGuid(),
            leaseId,
            workspaceId,
            type,
            timeProvider.GetUtcNow(),
            Redact(summary)));
    }

    private string Redact(string value) => redactor.Redact(value);

    private static SandboxResourceLimits ValidateLimits(SandboxResourceLimits limits)
    {
        if (limits.CpuCount <= 0 || limits.CpuCount > 8)
        {
            throw new ArgumentOutOfRangeException(nameof(limits), "CPU limit must be greater than 0 and at most 8.");
        }

        if (limits.MemoryMegabytes < 128 || limits.MemoryMegabytes > 32768)
        {
            throw new ArgumentOutOfRangeException(nameof(limits), "Memory limit must be between 128 MB and 32768 MB.");
        }

        return limits;
    }

    private static IReadOnlyDictionary<string, string> ValidateEnvironment(
        IReadOnlyDictionary<string, string> environment)
    {
        foreach (var key in environment.Keys)
        {
            if (SensitiveEnvironmentFragments.Any(fragment => key.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidOperationException($"Environment variable '{key}' cannot be passed into a sandbox.");
            }
        }

        return environment;
    }

    private static IReadOnlyList<SandboxMount> ValidateMounts(IReadOnlyList<SandboxMount> mounts)
    {
        foreach (var mount in mounts)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(mount.HostPath);
            ArgumentException.ThrowIfNullOrWhiteSpace(mount.ContainerPath);

            var normalized = mount.HostPath.Replace('\\', '/');
            if (mount.Writable && IsProtectedHostPath(normalized))
            {
                throw new InvalidOperationException($"Writable mount '{mount.HostPath}' is not allowed.");
            }
        }

        return mounts;
    }

    private static bool IsProtectedHostPath(string path) =>
        path.Contains("/.ssh", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/.aws", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/.azure", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/.config", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("/production", StringComparison.OrdinalIgnoreCase) ||
        path.EndsWith(".env", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeName(string value)
    {
        var chars = value.Trim().ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray();
        return new string(chars).Trim('-');
    }

    private string NormalizeContainerPath(string path) =>
        string.IsNullOrWhiteSpace(path) || path == "."
            ? options.WorkspaceRoot
            : CombineContainerPath(options.WorkspaceRoot, path);

    private static string CombineContainerPath(string root, string relativePath) =>
        $"{root.TrimEnd('/')}/{relativePath.Replace('\\', '/').TrimStart('/')}";

    private static string SanitizeArtifactName(string value)
    {
        var name = Path.GetFileName(value.Trim());
        if (string.IsNullOrWhiteSpace(name) || name is "." or "..")
        {
            throw new InvalidOperationException("Artifact name must resolve inside the artifact root.");
        }

        return name;
    }

    private sealed record DockerLeaseState(SandboxWorkspaceLease Lease, string ContainerName);
}
