using System.Text;

namespace AgentWorkflow.Core.Domain;

public static class ProjectCode
{
    public const int MaxLength = 10;

    public static string Normalize(string? code, string projectName)
    {
        var candidate = string.IsNullOrWhiteSpace(code)
            ? CreateFromName(projectName)
            : code.Trim().ToUpperInvariant();
        return candidate;
    }

    public static bool IsValid(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var value = code.Trim();
        return value.Length is >= 2 and <= MaxLength &&
               char.IsAsciiLetter(value[0]) &&
               value.All(character => char.IsAsciiLetterOrDigit(character));
    }

    private static string CreateFromName(string projectName)
    {
        var words = projectName.Split(
            [' ', '-', '_', '.', '/'],
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var builder = new StringBuilder(MaxLength);

        foreach (var word in words)
        {
            var character = word.FirstOrDefault(char.IsAsciiLetterOrDigit);
            if (character != default)
            {
                builder.Append(char.ToUpperInvariant(character));
            }

            if (builder.Length == MaxLength)
            {
                break;
            }
        }

        if (builder.Length < 2)
        {
            builder.Clear();
            foreach (var character in projectName.Where(char.IsAsciiLetterOrDigit))
            {
                builder.Append(char.ToUpperInvariant(character));
                if (builder.Length == MaxLength)
                {
                    break;
                }
            }
        }

        return builder.Length >= 2 ? builder.ToString() : "PRJ";
    }
}
