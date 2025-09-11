using AssertSyscall.NUnit;

namespace AssertSyscall.Constraints;

public sealed class DenyBindTCPConstraint(IEnumerable<int>? excludePorts) : ISyscallConstraint
{
    private const string BIND_FUNC = "bind";
    private const string PORT_ARG = "port";


    public IEnumerable<Syscall> FindViolations(IEnumerable<Syscall> syscalls)
    {
        var bindSyscalls = syscalls.NetworkModifies()
                .Where(syscall => syscall.IsFuncCall(BIND_FUNC));

        return excludePorts != null && excludePorts.Any()
            ? bindSyscalls.Where(syscall => !excludePorts.Any(p => syscall.IsFuncCall(BIND_FUNC, PORT_ARG, p.ToString())))
            : bindSyscalls;
    }
}
