using AssertSyscall.NUnit.Tests;

namespace AssertSyscall.NUnit;

public class CustomSyscallConstraintAttribute(Type syscallConstraint) : SyscallConstraintAttribute
{
    public override ISyscallConstraint CreateConstraint()
    {
        return Activator.CreateInstance(syscallConstraint) as ISyscallConstraint
            ?? throw new ArgumentException(nameof(syscallConstraint));
    }
}
