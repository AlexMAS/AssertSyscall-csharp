namespace AssertSyscall.Tracing;

internal record TraceCommand
(
    TraceCommandType Type,
    IList<string>? Args = null
);
