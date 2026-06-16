namespace AgentWorkflow.Cli.Application;

public sealed record CliOptions(string TaskId, string? RepositoryPath, string? RepositoryUrl)
{
    public static CliOptions FromArgs(string[] args)
    {
        var positional = new List<string>();
        string? repositoryUrl = null;

        for (var index = 0; index < args.Length; index++)
        {
            if (args[index] == "--repo-url" && index + 1 < args.Length)
            {
                repositoryUrl = args[index + 1];
                index++;
                continue;
            }

            positional.Add(args[index]);
        }

        var taskId = positional.Count > 0 ? positional[0] : "jira-awb-101";
        var repositoryPath = positional.Count > 1 ? positional[1] : null;

        return new CliOptions(taskId, repositoryPath, repositoryUrl);
    }
}
