namespace AssertSyscall.Tracing;

internal interface ISyscallTracer : IDisposable
{
    void Initialize();

    ISyscallTrace Start(string testId);
}
