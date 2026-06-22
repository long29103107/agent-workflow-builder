using System.Security.Cryptography;
using System.Text;

namespace AgentWorkflow.Core.Domain;

public static class ApprovalInputHasher
{
    public static string Compute(string content) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(content)));
}
