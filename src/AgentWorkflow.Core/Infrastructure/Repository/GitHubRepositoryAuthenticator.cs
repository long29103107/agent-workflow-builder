using AgentWorkflow.Core.Application;
using AgentWorkflow.Core.Domain;

namespace AgentWorkflow.Core.Infrastructure;

public sealed class GitHubRepositoryAuthenticator : IGitHubRepositoryAuthenticator
{
    public RepositoryCloneTarget CreateCloneTarget(
        RepositoryConnection connection,
        string? accessToken)
    {
        if (!string.Equals(connection.Provider, "github", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(connection.Url))
        {
            var localTarget = string.IsNullOrWhiteSpace(connection.LocalPath)
                ? connection.Url ?? string.Empty
                : connection.LocalPath;
            return new RepositoryCloneTarget(localTarget, localTarget, "none");
        }

        var displayUrl = connection.Url.Trim();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new RepositoryCloneTarget(displayUrl, displayUrl, "anonymous");
        }

        if (!Uri.TryCreate(displayUrl, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return new RepositoryCloneTarget(displayUrl, displayUrl, "token-unapplied");
        }

        var builder = new UriBuilder(uri)
        {
            UserName = "x-access-token",
            Password = accessToken.Trim()
        };

        return new RepositoryCloneTarget(builder.Uri.ToString(), displayUrl, "github-token");
    }
}
