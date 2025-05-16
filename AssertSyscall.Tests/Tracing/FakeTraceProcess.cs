namespace AssertSyscall.Tracing;

internal class FakeTraceProcess(int tracedProcessId) : NullTraceProcess(tracedProcessId)
{
    public void AddSyscall(Syscall syscall) => Notify(syscall);
}
