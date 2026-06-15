namespace AgentWorkflow.Cli.Application;

public sealed record CliOptions(string TaskId, string? RepositoryPath)
{
    public static CliOptions FromArgs(string[] args)
    {
        var taskId = args.Length > 0 ? args[0] : "jira-awb-101";
        var repositoryPath = args.Length > 1 ? args[1] : null;

        return new CliOptions(taskId, repositoryPath);
    }
}
