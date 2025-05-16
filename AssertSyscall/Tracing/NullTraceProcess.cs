namespace AssertSyscall.Tracing;

internal class NullTraceProcess(int tracedProcessId) : TraceProcess(tracedProcessId)
{
    public override bool Start() => true;

    public override void Stop() { }
}
