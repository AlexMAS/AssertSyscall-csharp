using System.Text;

namespace AssertSyscall.Tracing;

internal class RemoteSyscallTracer<T> : ISyscallTracer where T : TraceProcess
{
    private static readonly TimeSpan TRACER_TERMINATION_TIMEOUT = TimeSpan.FromSeconds(30);
    private const string TRACER_DIR = "ASSERT_SYSCALL_TRACER_DIR";
    private const string TRACER_APP = "AssertSyscall.Tracer";

    private readonly ManagedProcess _traceProcess;
    private readonly SyscallTracerClient _syscallTracerClient;
    private readonly StringBuilder _stdErr;

    public RemoteSyscallTracer()
    {
        var tracer = FindTracerExec();

        _traceProcess = new ManagedProcess
        (
            tracer,
            [
                Environment.ProcessId.ToString(),
                $"\"{typeof(T).AssemblyQualifiedName!}\""
            ]
        );

        _stdErr = new();

        _traceProcess.StandardErrorLines += line => _stdErr.AppendLine(line);

        if (!_traceProcess.Start())
        {
            throw new InvalidOperationException($"Cannot start the tracer process. {_stdErr}", _traceProcess.CompletionReason);
        }

        _syscallTracerClient = new(_traceProcess.StandardOutput!, _traceProcess.StandardInput!);
    }

    private static string FindTracerExec()
    {
        var tracerDir = Environment.GetEnvironmentVariable(TRACER_DIR) ?? "";
        var tracerApp = OperatingSystem.IsLinux() ? TRACER_APP : TRACER_APP + ".exe";
        var tracer = Path.Join(tracerDir, tracerApp);

        if (File.Exists(tracer))
        {
            return tracer;
        }

        var processDir = Path.GetDirectoryName(Environment.ProcessPath);

        if (processDir == null)
        {
            return tracerApp;
        }

        tracer = Path.Join(processDir, tracerApp);

        if (File.Exists(tracer))
        {
            return tracer;
        }

        tracer = Path.Combine(processDir, "runtimes/any/native", tracerApp);

        if (File.Exists(tracer))
        {
            return tracer;
        }

        throw new FileNotFoundException("The tracer is not found.", tracerApp);
    }

    public void Initialize()
    {
        _syscallTracerClient.AwaitReadyToTrace();
    }

    public ISyscallTrace Start(string testId)
    {
        SendTraceCommand(TraceCommandType.Start, [testId], true);
        _syscallTracerClient.AwaitTraceStarted();
        return new RemoteSyscallTrace(this);
    }

    public void Dispose()
    {
        SendTraceCommand(TraceCommandType.Terminate, null, false);

        try
        {
            _traceProcess.WaitForExitAsync().Wait(TRACER_TERMINATION_TIMEOUT);
        }
        catch (Exception)
        {
            // Ignore
        }
        finally
        {
            _traceProcess.Dispose();
        }

        _stdErr.Clear();
    }

    private void SendTraceCommand(TraceCommandType command, IList<string>? args, bool throwIfTerminated)
    {
        if (!_traceProcess.IsCompleted)
        {
            _syscallTracerClient.SendTraceCommand(new TraceCommand(command, args));
        }
        else if (throwIfTerminated)
        {
            throw new InvalidOperationException($"The tracer process has been unexpectedly terminated. {_stdErr}", _traceProcess.CompletionReason);
        }
    }

    private TraceResult ReceiveTraceResult()
    {
        if (!_traceProcess.IsCompleted)
        {
            var traceResult = _syscallTracerClient.ReceiveTraceResult();

            if (traceResult != null)
            {
                return traceResult;
            }
        }

        throw new InvalidOperationException($"The tracer process has been unexpectedly terminated. {_stdErr}", _traceProcess.CompletionReason);
    }


    private class RemoteSyscallTrace : ISyscallTrace
    {
        private readonly RemoteSyscallTracer<T> _tracer;
        private readonly SyscallTraceAnchor _traceAnchor;

        public RemoteSyscallTrace(RemoteSyscallTracer<T> tracer)
        {
            _tracer = tracer;
            _traceAnchor = new();

            // Just before the test run
            _traceAnchor.DropStartAnchor();
        }


        public TraceResult Stop()
        {
            // Immediately after the test run
            _traceAnchor.DropStopAnchor();

            _tracer.SendTraceCommand(TraceCommandType.Stop, null, true);
            var traceResult = _tracer.ReceiveTraceResult();

            if (traceResult.Calls.Any())
            {
                traceResult = new TraceResult(traceResult.TestId, _traceAnchor.Filter(traceResult.Calls));
            }

            return traceResult;
        }
    }
}
