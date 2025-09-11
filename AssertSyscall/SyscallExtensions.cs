using System.Text.RegularExpressions;

namespace AssertSyscall;

public static class SyscallExtensions
{
    public const string PATH_ARG = "path";

    public static bool IsFuncCall(this Syscall syscall, string func, string? arg = null, string? value = null)
    {
        return string.Equals(syscall.Func, func)
            && (string.IsNullOrEmpty(arg) || (
                syscall.Args != null
                && syscall.Args.TryGetValue(arg, out var v)
                && string.Equals(v, value)));
    }

    public static IEnumerable<Syscall> FileWrites(this IEnumerable<Syscall> syscalls)
    {
        return syscalls.Where(i => i.Category == SyscallCategory.File
            && (i.Type == SyscallType.Modify || i.Type == SyscallType.Create || i.Type == SyscallType.Delete));
    }

    public static IEnumerable<Syscall> IncludePaths(this IEnumerable<Syscall> syscalls, IEnumerable<string>? includePathTemplates)
    {
        return includePathTemplates != null && includePathTemplates.Any()
            ? syscalls.Where(p => PathMatches(p.Args, includePathTemplates))
            : [];
    }

    public static IEnumerable<Syscall> ExcludePaths(this IEnumerable<Syscall> syscalls, IEnumerable<string>? excludePathTemplates)
    {
        return excludePathTemplates != null && excludePathTemplates.Any()
            ? syscalls.Where(p => !PathMatches(p.Args, excludePathTemplates))
            : syscalls;
    }

    private static bool PathMatches(IReadOnlyDictionary<string, string>? args, IEnumerable<string> pathTemplates)
    {
        if (args != null && args.TryGetValue(PATH_ARG, out var path))
        {
            path = path?.Trim() ?? "";
        }
        else
        {
            path = "";
        }

        var emptyPath = string.IsNullOrEmpty(path);

        return pathTemplates.Any(t =>
            (emptyPath && string.IsNullOrEmpty(t))
            || (!emptyPath && !string.IsNullOrEmpty(t) && Regex.IsMatch(path, t)));
    }

    public static IEnumerable<Syscall> NetworkModifies(this IEnumerable<Syscall> syscalls)
    {
        return syscalls.Where(i => i.Category == SyscallCategory.Network
            && (i.Type == SyscallType.Modify));
    }
}
