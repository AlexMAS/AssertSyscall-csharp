namespace AssertSyscall.NUnit;

public interface ISyscallConstraint
{
    IEnumerable<Syscall> FindViolations(IEnumerable<Syscall> syscalls);
}
