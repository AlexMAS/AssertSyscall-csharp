using AssertSyscall.Constraints;
using AssertSyscall.NUnit.Tests;

namespace AssertSyscall.NUnit;

public class DenyBindTCPAttribute(int[]? excludePorts = null) : SyscallConstraintAttribute
{
    public override DenyBindTCPConstraint CreateConstraint() => new(excludePorts);
}
