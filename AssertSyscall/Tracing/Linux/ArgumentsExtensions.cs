using System.Text.RegularExpressions;

namespace AssertSyscall.Tracing.Linux;

public static partial class ArgumentsExtensions
{
    public static string Arg(this IReadOnlyList<string> values, int index)
    {
        return (index >= 0 && index < values.Count) ? values[index] : "";
    }

    public static string ArgProperty(this IReadOnlyList<string> values, int index, string property)
    {
        if (index >= 0 && index < values.Count)
        {
            var arg = values[index];
            var props = ScalarProperties().Matches(arg);

            if (props != null)
            {
                foreach (Match prop in props)
                {
                    if (string.Equals(prop.Groups["p"].Value, property, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return prop.Groups["v"].Value;
                    }
                }
            }
        }

        return "";
    }

    public static string ArgComplexProperty(this IReadOnlyList<string> values, int index, string property)
    {
        if (index >= 0 && index < values.Count)
        {
            var arg = values[index];
            var props = ComplexProperties().Matches(arg);

            if (props != null)
            {
                foreach (Match prop in props)
                {
                    if (string.Equals(prop.Groups["p"].Value, property, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return prop.Groups["v"].Value;
                    }
                }
            }
        }

        return "";
    }

    public static string JoinPaths(this IReadOnlyList<string> arguments, int basePathIndex, int relativePathIndex)
    {
        return JoinPaths(Arg(arguments, basePathIndex), Arg(arguments, relativePathIndex));
    }

    private static string JoinPaths(string? basePath, string? relativePath)
    {
        var emptyBase = string.IsNullOrWhiteSpace(basePath);
        var emptyRelative = string.IsNullOrWhiteSpace(relativePath);

        if (emptyBase && emptyRelative)
        {
            return "";
        }

        if (!emptyBase && emptyRelative)
        {
            return basePath!.Trim();
        }

        if (emptyBase && !emptyRelative)
        {
            return relativePath!.Trim();
        }

        relativePath = relativePath!.Trim();

        // Ignore the base path if the relative turns out absolute.
        if (relativePath.StartsWith("/"))
        {
            return relativePath;
        }

        basePath = basePath!.Trim().TrimEnd('/') + '/';

        // Ignore the base path if it is part of the relative.
        if (relativePath.StartsWith(basePath))
        {
            return relativePath;
        }

        return basePath + relativePath.TrimStart('/');
    }


    [GeneratedRegex("(?<p>\\w+)\\s*=\\s*(?<v>.+?)[,}]", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ScalarProperties();

    [GeneratedRegex("(?<p>\\w+)\\s*=\\s*(?<m>\\w+)\\s*\\(\\s*(?<v>.+?)\\s*\\)", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex ComplexProperties();
}
