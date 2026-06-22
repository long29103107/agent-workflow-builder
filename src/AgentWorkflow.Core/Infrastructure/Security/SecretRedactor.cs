using System.Text.RegularExpressions;
using AgentWorkflow.Core.Application;

namespace AgentWorkflow.Core.Infrastructure;

public sealed partial class SecretRedactor : ISecretRedactor
{
    public string Redact(string value)
    {
        var redacted = CredentialAssignmentPattern().Replace(value, "$1=[REDACTED]");
        redacted = BearerTokenPattern().Replace(redacted, "Bearer [REDACTED]");
        return OpenAiKeyPattern().Replace(redacted, "[REDACTED]");
    }

    [GeneratedRegex("(?i)[\"']?(api[_-]?key|access[_-]?token|token|password|secret)[\"']?\\s*[:=]\\s*[\"']?([^\"'\\s,;}]+)[\"']?")]
    private static partial Regex CredentialAssignmentPattern();

    [GeneratedRegex("(?i)\\bBearer\\s+[A-Za-z0-9._~+/-]+=*")]
    private static partial Regex BearerTokenPattern();

    [GeneratedRegex("\\bsk-[A-Za-z0-9_-]{8,}\\b")]
    private static partial Regex OpenAiKeyPattern();
}
