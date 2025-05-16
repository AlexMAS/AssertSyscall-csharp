using NUnit.Framework;

namespace AssertSyscall;

[TestFixture]
internal class SyscallExtensionsTest
{
    [Test]
    public void ShouldDetectFuncCallWithArg()
    {
        // Given
        var syscall = new Syscall(1, 1, SyscallCategory.File, SyscallType.Modify, "m_map", new Dictionary<string, string>() { { "path", "/memfd:doublemapper" } });

        // When
        var result = syscall.IsFuncCall("m_map", "path", "/memfd:doublemapper");

        // Then
        Assert.That(result, Is.True);
    }
}
