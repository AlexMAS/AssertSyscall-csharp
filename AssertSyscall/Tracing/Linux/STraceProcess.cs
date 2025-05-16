namespace AssertSyscall.Tracing.Linux;

internal partial class STraceProcess : TraceProcess
{
    private readonly STraceOutputParser _sTraceOutputParser;
    private volatile ManagedProcess? _process;


    public STraceProcess(int tracedProcessId) : this(tracedProcessId, new STraceOutputParser())
    {
    }

    public STraceProcess(int tracedProcessId, STraceOutputParser sTraceOutputParser) : base(tracedProcessId)
    {
        _sTraceOutputParser = sTraceOutputParser;
    }


    public override bool Start()
    {
        // https://man7.org/linux/man-pages/man1/strace.1.html
        // %creds - Trace system calls that read or modify user and group identifiers or capability sets.
        // %process - Trace system calls associated with process lifecycle (creation, exec, termination).
        // %file - Trace all system calls which take a file name as an argument.
        // %desc - Trace all file descriptor related system calls.
        // %ipc - Trace all IPC related system calls.
        // %network - Trace all the network related system calls.
        // %memory - Trace all memory mapping related system calls.
        // %clock - Trace system calls that read or modify system clocks.

        _process = new ManagedProcess("strace",
            [
                "-p", TracedProcessId.ToString(),
                "--quiet",
                "--follow-forks",
                "--string-limit=32",
                "--decode-fds=path",
                "--decode-pids=comm",
                "--trace=%creds,%process,%file,%desc,%ipc,%network,%memory,%clock"
            ]);

        _process.StandardErrorLines += HandleTraceLine;

        return _process.Start();
    }

    public override void Stop()
    {
        if (_process != null)
        {
            lock (this)
            {
                if (_process != null)
                {
                    _process.Dispose();
                    _process = null;
                }
            }
        }
    }


    private void HandleTraceLine(string traceLine)
    {
        if (CanNotify())
        {
            var syscall = _sTraceOutputParser.Parse(traceLine);

            if (syscall != null)
            {
                Notify(syscall);
            }
        }
    }
}
