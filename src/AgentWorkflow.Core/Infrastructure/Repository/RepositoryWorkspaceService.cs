using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;
using System.Text.Json;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class RepositoryWorkspaceService(
    IExecutionSandboxProvider sandbox,
    IGitHubRepositoryAuthenticator authenticator) : IRepositoryWorkspaceService
{
    private static readonly TimeSpan MetadataCommandTimeout = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan WorkspaceResetTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan FinalizationCommandTimeout = TimeSpan.FromMinutes(2);
    private const string ArtifactDirectory = ".agent-workflow/artifacts";

    public async Task<RepositoryWorkspace> CloneAsync(
        RepositoryCloneRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.WorkspaceId);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.ProjectId);

        var cloneTarget = authenticator.CreateCloneTarget(request.Connection, request.AccessToken);
        var lease = await sandbox.ProvisionAsync(
            new SandboxProvisionRequest(
                request.WorkspaceId,
                request.ProjectId,
                cloneTarget.DisplayUrl,
                request.Connection.DefaultBranch,
                request.LeaseDuration),
            cancellationToken);
        var context = new SandboxActionContext(lease.Id, lease.WorkspaceId);

        await sandbox.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Clone, [cloneTarget.CloneUrl, "."]),
            cancellationToken);

        var defaultBranch = NormalizeDefaultBranch(
            await ExecuteGitAsync(context, ["symbolic-ref", "--short", "refs/remotes/origin/HEAD"], cancellationToken),
            request.Connection.DefaultBranch);
        var baseSha = NormalizeSingleLine(
            await ExecuteGitAsync(context, ["rev-parse", "HEAD"], cancellationToken),
            "unknown");
        var files = ParseFiles(await ExecuteGitAsync(context, ["ls-files"], cancellationToken));

        var metadata = new RepositoryMetadata(
            request.Connection.Owner,
            request.Connection.Name,
            defaultBranch,
            baseSha,
            DetectProjectType(files),
            SelectImportantFiles(files));
        var updatedConnection = request.Connection with
        {
            DefaultBranch = defaultBranch,
            Status = "Cloned",
            Summary = $"Repository '{request.Connection.Owner}/{request.Connection.Name}' cloned into sandbox workspace '{lease.WorkspaceId}'."
        };

        return new RepositoryWorkspace(
            lease,
            updatedConnection,
            metadata,
            sandbox.GetLifecycleEvents(lease.Id));
    }

    public async Task<RepositoryBranchPreparation> PrepareBranchAsync(
        RepositoryBranchPreparationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.TaskId);

        var workspace = request.Workspace;
        var context = new SandboxActionContext(workspace.Lease.Id, workspace.Lease.WorkspaceId);
        var baseSha = NormalizeSingleLine(
            string.IsNullOrWhiteSpace(request.BaseSha) ? workspace.Metadata.BaseSha : request.BaseSha,
            workspace.Metadata.BaseSha);
        var branchName = CreateBranchName(request.BranchPolicy.BranchPrefix, request.TaskId);
        ValidateBranchName(branchName, request.BranchPolicy, workspace.Metadata.DefaultBranch);

        await sandbox.RunGitAsync(
            new SandboxGitActionRequest(
                context,
                SandboxGitActionKind.Fetch,
                ["origin", workspace.Metadata.DefaultBranch, "--prune"]),
            cancellationToken);
        await sandbox.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Checkout, ["--detach", baseSha]),
            cancellationToken);
        await ExecuteGitAsync(context, ["clean", "-fdx"], WorkspaceResetTimeout, cancellationToken);
        await ExecuteGitAsync(context, ["reset", "--hard", baseSha], WorkspaceResetTimeout, cancellationToken);
        await sandbox.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Branch, ["-f", branchName, baseSha]),
            cancellationToken);
        await sandbox.RunGitAsync(
            new SandboxGitActionRequest(context, SandboxGitActionKind.Checkout, [branchName]),
            cancellationToken);

        return new RepositoryBranchPreparation(
            workspace,
            baseSha,
            branchName,
            sandbox.GetLifecycleEvents(workspace.Lease.Id));
    }

    public async Task<RepositoryWorkspaceFinalization> FinalizeAsync(
        RepositoryWorkspaceFinalizationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Workspace);
        if (request.RetentionPolicy.MaxArtifactCount < 4)
        {
            throw new ArgumentOutOfRangeException(nameof(request), "Retention policy must keep at least four workspace artifacts.");
        }

        var workspace = request.Workspace;
        var context = new SandboxActionContext(workspace.Lease.Id, workspace.Lease.WorkspaceId);
        var artifacts = new List<SandboxArtifact>();

        await CreateFinalizationArtifactFilesAsync(workspace, context, cancellationToken);

        foreach (var artifact in StandardArtifacts())
        {
            artifacts.Add(await sandbox.CaptureArtifactAsync(
                new SandboxArtifactRequest(context, artifact.Name, artifact.RelativePath, artifact.ContentType),
                cancellationToken));
        }

        if (request.RetentionPolicy.IncludeGeneratedArtifacts)
        {
            foreach (var generatedPath in SelectGeneratedArtifacts(
                         request.GeneratedArtifactPaths ?? [],
                         request.RetentionPolicy.MaxArtifactCount - artifacts.Count))
            {
                artifacts.Add(await sandbox.CaptureArtifactAsync(
                    new SandboxArtifactRequest(
                        context,
                        CreateArtifactName(generatedPath),
                        generatedPath,
                        "application/octet-stream"),
                    cancellationToken));
            }
        }

        var finalLease = await sandbox.DestroyAsync(
            new SandboxDestroyRequest(
                context,
                request.Succeeded
                    ? "Repository workspace finalized and destroyed."
                    : "Repository workspace finalized and quarantined after failure.",
                Quarantine: !request.Succeeded && request.QuarantineOnFailure),
            cancellationToken);

        return new RepositoryWorkspaceFinalization(
            workspace,
            artifacts,
            finalLease,
            sandbox.GetLifecycleEvents(workspace.Lease.Id));
    }

    private async Task CreateFinalizationArtifactFilesAsync(
        RepositoryWorkspace workspace,
        SandboxActionContext context,
        CancellationToken cancellationToken)
    {
        var metadataJson = JsonSerializer.Serialize(new
        {
            workspace.Metadata.Owner,
            workspace.Metadata.Name,
            workspace.Metadata.DefaultBranch,
            workspace.Metadata.BaseSha,
            workspace.Metadata.ProjectType,
            workspace.Metadata.ImportantFiles
        });
        var script = string.Join(
            " && ",
            $"mkdir -p {ArtifactDirectory}",
            $"printf '%s\\n' '{EscapeShellSingleQuoted(metadataJson)}' > {ArtifactDirectory}/repository-metadata.json",
            $"git diff --binary > {ArtifactDirectory}/repository-diff.patch",
            $"git status --short > {ArtifactDirectory}/repository-status.log",
            $"git log --oneline -5 > {ArtifactDirectory}/repository-log.txt");

        var result = await sandbox.ExecuteCommandAsync(
            new SandboxCommandActionRequest(
                context,
                "sh",
                ["-c", script],
                ".",
                FinalizationCommandTimeout),
            cancellationToken);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Workspace finalization artifact command failed: {result.StandardError}");
        }
    }

    private static IReadOnlyList<(string Name, string RelativePath, string ContentType)> StandardArtifacts() =>
    [
        ("repository-metadata", $"{ArtifactDirectory}/repository-metadata.json", "application/json"),
        ("repository-diff", $"{ArtifactDirectory}/repository-diff.patch", "text/x-patch"),
        ("repository-status", $"{ArtifactDirectory}/repository-status.log", "text/plain"),
        ("repository-log", $"{ArtifactDirectory}/repository-log.txt", "text/plain")
    ];

    private static IEnumerable<string> SelectGeneratedArtifacts(
        IReadOnlyList<string> generatedArtifactPaths,
        int maxCount)
    {
        if (maxCount <= 0)
        {
            return [];
        }

        return generatedArtifactPaths
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(maxCount);
    }

    private static string CreateArtifactName(string relativePath)
    {
        var name = Path.GetFileName(relativePath.Replace('\\', '/'));
        return string.IsNullOrWhiteSpace(name)
            ? "generated-artifact"
            : name;
    }

    private static string EscapeShellSingleQuoted(string value) =>
        value.Replace("'", "'\"'\"'", StringComparison.Ordinal);

    private async Task<string> ExecuteGitAsync(
        SandboxActionContext context,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken) =>
        await ExecuteGitAsync(context, arguments, MetadataCommandTimeout, cancellationToken);

    private async Task<string> ExecuteGitAsync(
        SandboxActionContext context,
        IReadOnlyList<string> arguments,
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        var result = await sandbox.ExecuteCommandAsync(
            new SandboxCommandActionRequest(
                context,
                "git",
                arguments,
                ".",
                timeout),
            cancellationToken);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git metadata command failed: {result.StandardError}");
        }

        return result.StandardOutput;
    }

    private static string CreateBranchName(string prefix, string taskId)
    {
        var normalizedPrefix = string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.Trim();
        var normalizedTask = new string(taskId
            .Trim()
            .ToLowerInvariant()
            .Select(character => char.IsLetterOrDigit(character) ? character : '-')
            .ToArray())
            .Trim('-');

        if (string.IsNullOrWhiteSpace(normalizedTask))
        {
            throw new ArgumentException("Task ID must contain at least one branch-safe character.", nameof(taskId));
        }

        return $"{normalizedPrefix}{normalizedTask}";
    }

    private static void ValidateBranchName(
        string branchName,
        ProjectBranchPolicy branchPolicy,
        string defaultBranch)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            throw new InvalidOperationException("Prepared branch name cannot be empty.");
        }

        if (string.Equals(branchName, defaultBranch, StringComparison.Ordinal) ||
            string.Equals(branchName, branchPolicy.BaseBranch, StringComparison.Ordinal))
        {
            throw new InvalidOperationException($"Prepared branch '{branchName}' cannot be the default/base branch.");
        }
    }

    private static string NormalizeDefaultBranch(string output, string fallback)
    {
        var value = NormalizeSingleLine(output, fallback);
        const string originPrefix = "origin/";
        return value.StartsWith(originPrefix, StringComparison.OrdinalIgnoreCase)
            ? value[originPrefix.Length..]
            : value;
    }

    private static string NormalizeSingleLine(string output, string fallback)
    {
        var value = output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static IReadOnlyList<string> ParseFiles(string output) =>
        output
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static IReadOnlyList<string> SelectImportantFiles(IReadOnlyList<string> files) =>
        files
            .Where(IsHighSignalFile)
            .Take(12)
            .ToList();

    private static string DetectProjectType(IReadOnlyList<string> files)
    {
        var types = new List<string>();
        if (files.Any(file =>
                file.EndsWith(".sln", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".slnx", StringComparison.OrdinalIgnoreCase) ||
                file.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)))
        {
            types.Add(".NET");
        }

        if (files.Any(file => file.EndsWith("package.json", StringComparison.OrdinalIgnoreCase)))
        {
            types.Add("Node");
        }

        if (files.Any(file => file.EndsWith("docker-compose.yml", StringComparison.OrdinalIgnoreCase)))
        {
            types.Add("Docker Compose");
        }

        return types.Count == 0 ? "Unknown" : string.Join(", ", types);
    }

    private static bool IsHighSignalFile(string file)
    {
        string[] endings =
        [
            ".csproj",
            ".sln",
            ".slnx",
            "package.json",
            "docker-compose.yml",
            "README.md",
            "AGENTS.md",
            "REQUEST.md"
        ];

        return endings.Any(ending => file.EndsWith(ending, StringComparison.OrdinalIgnoreCase));
    }
}
