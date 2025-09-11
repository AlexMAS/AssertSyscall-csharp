using AssertSyscall.Constraints;

namespace AssertSyscall.NUnit.Tests;

public class DenyFileWriteAttribute(string[]? includePaths = null, string[]? excludePaths = null) : SyscallConstraintAttribute
{
    public override DenyFileWriteConstraint CreateConstraint() => new(includePaths, excludePaths);
}
