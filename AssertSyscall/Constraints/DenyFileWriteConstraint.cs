using AssertSyscall.NUnit;

namespace AssertSyscall.Constraints;

public sealed class DenyFileWriteConstraint(IEnumerable<string>? includePaths = null, IEnumerable<string>? excludePaths = null) : ISyscallConstraint
{
    public IEnumerable<Syscall> FindViolations(IEnumerable<Syscall> syscalls)
    {
        return syscalls.FileWrites()
            .IncludePaths(includePaths)
            .ExcludePaths(excludePaths);
    }
}
