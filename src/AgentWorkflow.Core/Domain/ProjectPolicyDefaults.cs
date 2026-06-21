namespace AgentWorkflow.Core.Domain;

public static class ProjectPolicyDefaults
{
    public const string DefaultProjectId = "workspace-default";

    public static CreateProjectRequest Create(WorkspaceDefaults defaults)
    {
        var name = string.IsNullOrWhiteSpace(defaults.Name) ? "Project Alpha" : defaults.Name.Trim();
        var (owner, repository) = ParseGitHubTarget(defaults.RepositoryUrl);
        return new CreateProjectRequest(
            name,
            new ProjectRepositorySettings(
                string.IsNullOrWhiteSpace(defaults.RepositoryProvider) ? "github" : defaults.RepositoryProvider.Trim(),
                defaults.RepositoryPath?.Trim() ?? string.Empty,
                defaults.RepositoryUrl?.Trim() ?? string.Empty,
                "main"),
            new ProjectGitHubSettings(owner, repository, InstallationId: null),
            new ProjectAgentSettings(
                [
                    "Repository Investigator Agent",
                    "Jira Notion Context Agent",
                    "Memory Research Agent",
                    "Planning Agent"
                ],
                RequireExplicitSelection: false),
            new ProjectCodingStandardSettings(
                ["AGENTS.md", "docs/knowledge/index.md"],
                [
                    "Keep AgentWorkflow.Core as the source of truth.",
                    "Preserve mock-first provider boundaries.",
                    "Keep adapters thin."
                ]),
            new ProjectCommandSettings(
                Setup: [],
                Build: ["dotnet build AgentWorkflowBuilder.slnx", "bun run build"],
                Test: ["dotnet test AgentWorkflowBuilder.slnx"],
                Lint: [],
                TimeoutSeconds: 900),
            new ProjectBranchPolicy(
                BaseBranch: "main",
                BranchPrefix: "agent/",
                AllowForcePush: false),
            new ProjectProtectedPathPolicy(
                [".git", ".env", ".env.local", "appsettings.Production.json"],
                BlockProductionEnvironmentFiles: true),
            new ProjectApprovalPolicy(
                RequireInvestigationPlanApproval: true,
                RequireImplementationApproval: true,
                RequirePullRequestApproval: true,
                RequireMergeApproval: true),
            ProjectCode.Normalize(defaults.Code, name));
    }

    private static (string Owner, string Repository) ParseGitHubTarget(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (string.Empty, string.Empty);
        }

        var shorthand = value.Trim().TrimEnd('/');
        if (Uri.TryCreate(shorthand, UriKind.Absolute, out var uri))
        {
            shorthand = uri.AbsolutePath.Trim('/');
        }

        var segments = shorthand.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
        {
            return (string.Empty, NormalizeRepository(segments.LastOrDefault() ?? string.Empty));
        }

        return (segments[^2], NormalizeRepository(segments[^1]));
    }

    private static string NormalizeRepository(string value) =>
        value.EndsWith(".git", StringComparison.OrdinalIgnoreCase)
            ? value[..^4]
            : value;
}
