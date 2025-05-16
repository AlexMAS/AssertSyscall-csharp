namespace AssertSyscall.Tracing;

internal abstract class TraceProcess(int tracedProcessId) : IDisposable
{
    public int TracedProcessId { get; } = tracedProcessId;

    public event SyscallCallback? OnSyscall;


    public abstract bool Start();

    public abstract void Stop();

    public virtual void Dispose()
    {
        Stop();
    }


    protected bool CanNotify()
    {
        return (OnSyscall != null);
    }

    protected void Notify(Syscall syscall)
    {
        OnSyscall?.Invoke(syscall);
    }


    public delegate void SyscallCallback(Syscall syscall);
}
