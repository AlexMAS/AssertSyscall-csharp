namespace AssertSyscall.NUnit.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public abstract class SyscallConstraintAttribute : Attribute
{
    public abstract ISyscallConstraint CreateConstraint();
}
