namespace AssertSyscall.Tracing;

internal record TraceResult
(
    string TestId,
    IEnumerable<Syscall> Calls
);
