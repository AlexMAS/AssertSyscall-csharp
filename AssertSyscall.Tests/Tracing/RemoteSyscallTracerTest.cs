using NUnit.Framework;

namespace AssertSyscall.Tracing;

[TestFixture]
internal class RemoteSyscallTracerTest
{
    [Test]
    public void ShouldTraceTest()
    {
        // Given
        var testId = Guid.NewGuid().ToString();
        using var target = new RemoteSyscallTracer<NullTraceProcess>();
        target.Initialize();

        // When
        var trace = target.Start(testId);
        var traceResult = trace.Stop();

        // Then
        Assert.That(traceResult, Is.Not.Null);
        Assert.That(traceResult!.TestId, Is.EqualTo(testId));
    }
}
