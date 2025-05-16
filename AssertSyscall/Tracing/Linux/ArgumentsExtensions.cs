namespace AssertSyscall.Tracing.Linux;

public static class ArgumentsExtensions
{
    public static string Arg(this IReadOnlyList<string> values, int index)
    {
        return (index >= 0 && index < values.Count) ? values[index] : "";
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
}
