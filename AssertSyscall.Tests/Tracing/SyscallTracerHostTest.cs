using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Text.Json;

namespace AssertSyscall.Tracing;

[TestFixture]
internal class SyscallTracerHostTest
{
    [Test]
    public void ShouldThrowExceptionIfTracedProcessNotFound() =>
        Assert.Throws<SyscallTracerHostException>(() => new SyscallTracerHost(int.MinValue, typeof(FakeTraceProcess)));

    [Test]
    public void ShouldThrowExceptionIfWrongTracerClass() =>
        Assert.Throws<SyscallTracerHostException>(() => new SyscallTracerHost(Environment.ProcessId, typeof(object)));

    [Test]
    public void ShouldThrowExceptionIfNoConstructorWithPID() =>
        Assert.Throws<SyscallTracerHostException>(() => new SyscallTracerHost(Environment.ProcessId, typeof(TraceProcessWithoutConstructor)));

    [Test]
    public void ShouldThrowExceptionIfNoArgs() =>
        Assert.Throws<SyscallTracerHostException>(() => SyscallTracerHost.ForConsoleApp([]));

    [Test]
    public void ShouldThrowExceptionIfWrongTracedPIDFormat() =>
        Assert.Throws<SyscallTracerHostException>(() => SyscallTracerHost.ForConsoleApp(["wrong-PID", typeof(FakeTraceProcess).FullName!]));

    [Test]
    public void ShouldThrowExceptionIfUnknownTracerClass() =>
        Assert.Throws<SyscallTracerHostException>(() => SyscallTracerHost.ForConsoleApp([Environment.ProcessId.ToString(), "Unknown.Tracer.Class"]));

    [Test]
    public void ShouldTraceTest()
    {
        // Given

        var testId = Guid.NewGuid().ToString();

        using var requestStream = ForCommands
        (
            new TraceCommand(TraceCommandType.Start, [testId]),
            new TraceCommand(TraceCommandType.Stop),
            new TraceCommand(TraceCommandType.Terminate)
        );

        using var responseStream = new MemoryStream();

        using var target = new SyscallTracerHost(Environment.ProcessId, typeof(FakeTraceProcess), requestStream, new StreamWriter(responseStream));

        // When
        target.Start();

        // Then
        responseStream.Position = 0;
        using var responseStreamReader = new StreamReader(responseStream);
        responseStreamReader.ReadLine(); // Read "ReadyToTrace" signal
        responseStreamReader.ReadLine(); // Read "TraceStarted" signal
        var traceResult = JsonSerializer.Deserialize<TraceResult>(responseStreamReader.ReadLine()!);
        Assert.That(traceResult, Is.Not.Null);
        Assert.That(traceResult!.TestId, Is.EqualTo(testId));
    }

    [Test]
    public void ShouldTraceTwoTests()
    {
        // Given

        var test1Id = Guid.NewGuid().ToString();
        var test2Id = Guid.NewGuid().ToString();

        using var requestStream = ForCommands
        (
            new TraceCommand(TraceCommandType.Start, [test1Id]),
            new TraceCommand(TraceCommandType.Stop),
            new TraceCommand(TraceCommandType.Start, [test2Id]),
            new TraceCommand(TraceCommandType.Stop),
            new TraceCommand(TraceCommandType.Terminate)
        );

        using var responseStream = new MemoryStream();

        using var target = new SyscallTracerHost(Environment.ProcessId, typeof(FakeTraceProcess), requestStream, new StreamWriter(responseStream));

        // When
        target.Start();

        // Then
        responseStream.Position = 0;
        using var responseStreamReader = new StreamReader(responseStream);
        responseStreamReader.ReadLine(); // Read "ReadyToTrace" signal
        responseStreamReader.ReadLine(); // Read "TraceStarted" signal
        var traceResult1 = JsonSerializer.Deserialize<TraceResult>(responseStreamReader.ReadLine()!);
        Assert.That(traceResult1, Is.Not.Null);
        Assert.That(traceResult1!.TestId, Is.EqualTo(test1Id));
        responseStreamReader.ReadLine(); // Read "TraceStarted" signal
        var traceResult2 = JsonSerializer.Deserialize<TraceResult>(responseStreamReader.ReadLine()!);
        Assert.That(traceResult2, Is.Not.Null);
        Assert.That(traceResult2!.TestId, Is.EqualTo(test2Id));
    }

    private static StreamReader ForCommands(params TraceCommand[] commands)
    {
        var memoryStream = new MemoryStream();
        var requestStream = new StreamWriter(memoryStream);

        foreach (var command in commands)
        {
            requestStream.WriteLine(JsonSerializer.Serialize(command));
        }

        requestStream.Flush();
        memoryStream.Position = 0;

        return new StreamReader(memoryStream);
    }


    private class TraceProcessWithoutConstructor : NullTraceProcess
    {
        public TraceProcessWithoutConstructor() : base(0) { }
    }
}
