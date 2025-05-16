using System.Text.Json;

namespace AssertSyscall.Tracing;

internal class SyscallTracerClient(TextReader responseReader, TextWriter requestWriter)
{
    public void AwaitReadyToTrace()
    {
        responseReader.ReadLine();
    }

    public void AwaitTraceStarted()
    {
        responseReader.ReadLine();
    }

    public void SendTraceCommand(TraceCommand command)
    {
        var request = JsonSerializer.Serialize(command);
        requestWriter.WriteLine(request);
        requestWriter.Flush();
    }

    public TraceResult? ReceiveTraceResult()
    {
        var response = responseReader.ReadLine();

        return !string.IsNullOrEmpty(response)
            ? JsonSerializer.Deserialize<TraceResult>(response)
            : null;
    }
}
