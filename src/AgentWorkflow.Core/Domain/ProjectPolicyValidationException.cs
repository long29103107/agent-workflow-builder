namespace AgentWorkflow.Core.Domain;

public sealed class ProjectPolicyValidationException(
    IReadOnlyList<ProjectValidationError> errors)
    : ArgumentException(string.Join(
        " ",
        errors.Select(error => $"{error.Field}: {error.Message}")))
{
    public IReadOnlyList<ProjectValidationError> Errors { get; } = errors;
}
