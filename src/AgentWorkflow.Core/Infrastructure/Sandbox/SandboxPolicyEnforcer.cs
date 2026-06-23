using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

internal static class SandboxPolicyEnforcer
{
    private static readonly string[] DeploymentCommandNames =
    [
        "deploy",
        "kubectl",
        "helm",
        "terraform",
        "pulumi",
        "vercel",
        "netlify",
        "fly",
        "gcloud",
        "az",
        "aws"
    ];

    public static void ValidateCodeAction(SandboxCodeActionRequest request)
    {
        foreach (var path in request.RelativePaths)
        {
            var normalized = NormalizeRelativePath(path, nameof(request.RelativePaths));
            ValidateProtectedPath(request.Context.Policy, normalized);
        }
    }

    public static string ValidateWorkingDirectory(SandboxCommandActionRequest request)
    {
        var normalized = NormalizeRelativePath(request.WorkingDirectory, nameof(request.WorkingDirectory), allowCurrentDirectory: true);
        ValidateProtectedPath(request.Context.Policy, normalized);
        ValidateCommandPolicy(request.Context.Policy, request.Command, request.Arguments);
        return normalized;
    }

    public static void ValidateGitAction(SandboxGitActionRequest request)
    {
        if (request.Action is SandboxGitActionKind.Push &&
            request.Context.Policy?.AllowExternalWriteAccess is not true)
        {
            throw new InvalidOperationException("Git push requires explicit external write policy.");
        }
    }

    public static string ValidateArtifactPath(SandboxArtifactRequest request)
    {
        var normalized = NormalizeRelativePath(request.RelativePath, nameof(request.RelativePath));
        ValidateProtectedPath(request.Context.Policy, normalized);
        return normalized;
    }

    private static void ValidateCommandPolicy(
        SandboxWorkspacePolicy? policy,
        string command,
        IReadOnlyList<string> arguments)
    {
        if (IsDeploymentCommand(command, arguments) && policy?.AllowDeploymentCommands is not true)
        {
            throw new InvalidOperationException("Deployment commands require explicit deployment policy.");
        }

        if (IsExternalWriteCommand(command, arguments) && policy?.AllowExternalWriteAccess is not true)
        {
            throw new InvalidOperationException("External write commands require explicit external write policy.");
        }
    }

    private static bool IsDeploymentCommand(string command, IReadOnlyList<string> arguments)
    {
        var name = Path.GetFileNameWithoutExtension(command.Trim());
        if (DeploymentCommandNames.Any(item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return string.Equals(name, "dotnet", StringComparison.OrdinalIgnoreCase) &&
            arguments.Any(argument => string.Equals(argument, "publish", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsExternalWriteCommand(string command, IReadOnlyList<string> arguments)
    {
        var name = Path.GetFileNameWithoutExtension(command.Trim());
        return (string.Equals(name, "git", StringComparison.OrdinalIgnoreCase) &&
                arguments.Any(argument => string.Equals(argument, "push", StringComparison.OrdinalIgnoreCase))) ||
            (string.Equals(name, "gh", StringComparison.OrdinalIgnoreCase) &&
                arguments.Count >= 2 &&
                string.Equals(arguments[0], "pr", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(arguments[1], "create", StringComparison.OrdinalIgnoreCase));
    }

    private static void ValidateProtectedPath(SandboxWorkspacePolicy? policy, string normalizedPath)
    {
        if (policy is null || normalizedPath == ".")
        {
            return;
        }

        foreach (var protectedPath in policy.ProtectedPaths)
        {
            var normalizedProtectedPath = NormalizeRelativePath(protectedPath, nameof(policy.ProtectedPaths));
            if (string.Equals(normalizedPath, normalizedProtectedPath, StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.StartsWith($"{normalizedProtectedPath}/", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Path '{normalizedPath}' is protected by project policy.");
            }
        }
    }

    private static string NormalizeRelativePath(
        string path,
        string parameterName,
        bool allowCurrentDirectory = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);

        var normalized = path.Trim().Replace('\\', '/');
        if (allowCurrentDirectory && normalized == ".")
        {
            return ".";
        }

        if (Path.IsPathRooted(path) || normalized.StartsWith('/') || normalized.StartsWith('~'))
        {
            throw new InvalidOperationException($"Path '{path}' must stay relative to the sandbox workspace root.");
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0 || segments.Any(segment => segment is "." or ".."))
        {
            throw new InvalidOperationException($"Path '{path}' must stay relative to the sandbox workspace root.");
        }

        return string.Join('/', segments);
    }
}
