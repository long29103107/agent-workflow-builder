using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class ProjectPolicyValidator : IProjectPolicyValidator
{
    public IReadOnlyList<ProjectValidationError> Validate(CreateProjectRequest request) =>
        Validate(
            request.Name,
            request.Code,
            request.Repository,
            request.GitHub,
            request.Agents,
            request.CodingStandards,
            request.Commands,
            request.BranchPolicy,
            request.ProtectedPaths,
            request.ApprovalPolicy);

    public IReadOnlyList<ProjectValidationError> Validate(UpdateProjectRequest request) =>
        Validate(
            request.Name,
            request.Code,
            request.Repository,
            request.GitHub,
            request.Agents,
            request.CodingStandards,
            request.Commands,
            request.BranchPolicy,
            request.ProtectedPaths,
            request.ApprovalPolicy);

    private static IReadOnlyList<ProjectValidationError> Validate(
        string name,
        string code,
        ProjectRepositorySettings repository,
        ProjectGitHubSettings github,
        ProjectAgentSettings agents,
        ProjectCodingStandardSettings codingStandards,
        ProjectCommandSettings commands,
        ProjectBranchPolicy branchPolicy,
        ProjectProtectedPathPolicy protectedPaths,
        ProjectApprovalPolicy approvalPolicy)
    {
        var errors = new List<ProjectValidationError>();

        if (string.IsNullOrWhiteSpace(name))
        {
            errors.Add(new("name", "Project name is required."));
        }
        else if (name.Trim().Length > 120)
        {
            errors.Add(new("name", "Project name cannot exceed 120 characters."));
        }

        if (!ProjectCode.IsValid(ProjectCode.Normalize(code, name)))
        {
            errors.Add(new(
                "code",
                $"Project code must start with a letter and contain 2-{ProjectCode.MaxLength} letters or digits."));
        }

        if (string.IsNullOrWhiteSpace(repository.Provider))
        {
            errors.Add(new("repository.provider", "Repository provider is required."));
        }

        if (string.IsNullOrWhiteSpace(repository.LocalPath) &&
            string.IsNullOrWhiteSpace(repository.Url))
        {
            errors.Add(new("repository", "A local path or repository URL is required."));
        }

        ValidateGitRef(repository.DefaultBranch, "repository.defaultBranch", errors);
        ValidateGitRef(branchPolicy.BaseBranch, "branchPolicy.baseBranch", errors);
        ValidateBranchPrefix(branchPolicy.BranchPrefix, errors);

        if (branchPolicy.AllowForcePush)
        {
            errors.Add(new("branchPolicy.allowForcePush", "Force push is not allowed by the MVP safety policy."));
        }

        if (string.Equals(repository.Provider, "github", StringComparison.OrdinalIgnoreCase) &&
            !string.IsNullOrWhiteSpace(repository.Url) &&
            (string.IsNullOrWhiteSpace(github.Owner) || string.IsNullOrWhiteSpace(github.Repository)))
        {
            errors.Add(new("github", "GitHub owner and repository are required when a GitHub URL is configured."));
        }

        ValidateDistinctValues(agents.EnabledAgentNames, "agents.enabledAgentNames", "At least one enabled agent is required.", errors);
        ValidateDistinctValues(codingStandards.InstructionFiles, "codingStandards.instructionFiles", "At least one instruction file is required.", errors);
        ValidateDistinctValues(codingStandards.Rules, "codingStandards.rules", "At least one coding rule is required.", errors);

        foreach (var instructionFile in codingStandards.InstructionFiles)
        {
            ValidateRelativePath(instructionFile, "codingStandards.instructionFiles", errors);
        }

        ValidateCommands(commands.Build, "commands.build", required: true, errors);
        ValidateCommands(commands.Test, "commands.test", required: true, errors);
        ValidateCommands(commands.Setup, "commands.setup", required: false, errors);
        ValidateCommands(commands.Lint, "commands.lint", required: false, errors);

        if (commands.TimeoutSeconds is < 1 or > 3600)
        {
            errors.Add(new("commands.timeoutSeconds", "Command timeout must be between 1 and 3600 seconds."));
        }

        if (protectedPaths.Paths.Count == 0)
        {
            errors.Add(new("protectedPaths.paths", "At least one protected path is required."));
        }

        foreach (var path in protectedPaths.Paths)
        {
            ValidateRelativePath(path, "protectedPaths.paths", errors);
        }

        if (!protectedPaths.BlockProductionEnvironmentFiles)
        {
            errors.Add(new("protectedPaths.blockProductionEnvironmentFiles", "Production environment files must remain protected."));
        }

        if (!approvalPolicy.RequireInvestigationPlanApproval ||
            !approvalPolicy.RequireImplementationApproval ||
            !approvalPolicy.RequirePullRequestApproval ||
            !approvalPolicy.RequireMergeApproval)
        {
            errors.Add(new("approvalPolicy", "All four approval gates are required before repository write automation."));
        }

        return errors;
    }

    private static void ValidateDistinctValues(
        IReadOnlyList<string> values,
        string field,
        string emptyMessage,
        List<ProjectValidationError> errors)
    {
        if (values.Count == 0 || values.All(string.IsNullOrWhiteSpace))
        {
            errors.Add(new(field, emptyMessage));
            return;
        }

        if (values.Any(string.IsNullOrWhiteSpace))
        {
            errors.Add(new(field, "Values cannot be blank."));
        }

        if (values.Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count() != values.Count(value => !string.IsNullOrWhiteSpace(value)))
        {
            errors.Add(new(field, "Duplicate values are not allowed."));
        }
    }

    private static void ValidateCommands(
        IReadOnlyList<string> commands,
        string field,
        bool required,
        List<ProjectValidationError> errors)
    {
        if (required && commands.Count == 0)
        {
            errors.Add(new(field, "At least one command is required."));
        }

        if (commands.Any(command =>
            string.IsNullOrWhiteSpace(command) ||
            command.Contains('\r') ||
            command.Contains('\n') ||
            command.Contains('\0')))
        {
            errors.Add(new(field, "Commands cannot be blank or contain control characters."));
        }
    }

    private static void ValidateRelativePath(
        string path,
        string field,
        List<ProjectValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            errors.Add(new(field, "Paths cannot be blank."));
            return;
        }

        var normalized = path.Replace('\\', '/');
        if (Path.IsPathRooted(path) ||
            normalized.Split('/', StringSplitOptions.RemoveEmptyEntries).Contains(".."))
        {
            errors.Add(new(field, $"Path '{path}' must stay relative to the project root."));
        }
    }

    private static void ValidateGitRef(
        string value,
        string field,
        List<ProjectValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) || HasUnsafeGitRefCharacters(value))
        {
            errors.Add(new(field, "A valid branch name is required."));
        }
    }

    private static void ValidateBranchPrefix(
        string value,
        List<ProjectValidationError> errors)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            !value.EndsWith('/') ||
            HasUnsafeGitRefCharacters(value[..^1]))
        {
            errors.Add(new("branchPolicy.branchPrefix", "Branch prefix must be a safe value ending in '/'."));
        }
    }

    private static bool HasUnsafeGitRefCharacters(string value) =>
        value.Contains("..", StringComparison.Ordinal) ||
        value.Any(character =>
            char.IsWhiteSpace(character) ||
            character is '~' or '^' or ':' or '?' or '*' or '[' or '\\');
}
