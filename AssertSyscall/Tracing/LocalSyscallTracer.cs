namespace AssertSyscall.Tracing;

internal class LocalSyscallTracer : ISyscallTracer
{
    private readonly TraceProcess _traceProcess;
    private volatile LocalSyscallTrace? _lastTrace;

    public LocalSyscallTracer(TraceProcess traceProcess)
    {
        _traceProcess = traceProcess;
    }

    public void Initialize()
    {
        if (!_traceProcess.Start())
        {
            throw SyscallTracerHostException.CannotStartTracing();
        }
    }

    public ISyscallTrace Start(string testId)
    {
        var trace = new LocalSyscallTrace(testId);

        lock (this)
        {
            if (_lastTrace != null)
            {
                _traceProcess.OnSyscall -= _lastTrace.AddSyscall;
            }

            _lastTrace = trace;

            _traceProcess.OnSyscall += trace.AddSyscall;
        }

        return trace;
    }

    public void Dispose()
    {
        if (_lastTrace != null)
        {
            lock (this)
            {
                if (_lastTrace != null)
                {
                    _traceProcess.OnSyscall -= _lastTrace.AddSyscall;
                    _lastTrace = null;
                }
            }
        }
    }


    private class LocalSyscallTrace(string testId) : ISyscallTrace
    {
        private readonly LinkedList<Syscall> _testSyscalls = new();

        public void AddSyscall(Syscall syscall)
        {
            _testSyscalls.AddLast(syscall);
        }

        public TraceResult Stop()
        {
            return new(testId, _testSyscalls);
        }
    }
}
