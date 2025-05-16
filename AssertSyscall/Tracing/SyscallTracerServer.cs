using System.Text.Json;

namespace AssertSyscall.Tracing;

internal class SyscallTracerServer(TextReader requestReader, TextWriter responseWriter)
{
    public void RaiseReadyToTrace()
    {
        responseWriter.WriteLine("ReadyToTrace");
        responseWriter.Flush();
    }

    public void RaiseTraceStarted()
    {
        responseWriter.WriteLine("TraceStarted");
        responseWriter.Flush();
    }

    public TraceCommand ReceiveTraceCommand()
    {
        var request = requestReader.ReadLine()!;
        return JsonSerializer.Deserialize<TraceCommand>(request)!;
    }

    public void SendTraceResult(TraceResult traceResult)
    {
        var response = JsonSerializer.Serialize(traceResult);
        responseWriter.WriteLine(response);
        responseWriter.Flush();
    }
}
