using NUnit.Framework;

namespace AssertSyscall.Tracing;

[TestFixture]
internal class LocalSyscallTracerTest
{
    [Test]
    public void ShouldTraceProcess()
    {
        // Given
        var traceProcess = new FakeTraceProcess(12345);
        var syscall1 = new Syscall(0, 123, SyscallCategory.File, SyscallType.Create, "open", null);
        var syscall2 = new Syscall(1, 123, SyscallCategory.File, SyscallType.Read, "read", null);
        var syscall3 = new Syscall(2, 123, SyscallCategory.File, SyscallType.Modify, "write", null);
        var testId = Guid.NewGuid().ToString();
        var target = new LocalSyscallTracer(traceProcess);

        // When
        var trace = target.Start(testId);
        traceProcess.AddSyscall(syscall1);
        traceProcess.AddSyscall(syscall2);
        traceProcess.AddSyscall(syscall3);
        var traceResult = trace.Stop();

        // Then
        Assert.That(traceResult, Is.Not.Null);
        Assert.That(traceResult.TestId, Is.EqualTo(testId));
        Assert.That(traceResult.Calls, Is.EquivalentTo([syscall1, syscall2, syscall3]));
    }
}
