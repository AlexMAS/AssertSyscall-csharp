using System.Diagnostics;
using System.Reflection;

namespace AssertSyscall.Tracing;

internal class SyscallTracerHost : IDisposable
{
    private readonly Func<TraceProcess> _tracerProcessFactory;
    private readonly SyscallTracerServer _syscallTracerServer;
    private volatile bool _stopped;


    public SyscallTracerHost(int tracedPID, Type tracerClass)
        : this(tracedPID, tracerClass, Console.In, Console.Out)
    {
    }

    public SyscallTracerHost(int tracedPID, Type tracerClass, TextReader requestReader, TextWriter responseWriter)
    {
        try
        {
            using (Process.GetProcessById(tracedPID)) { }
        }
        catch (ArgumentException)
        {
            throw SyscallTracerHostException.TracedProcessNotFound(tracedPID);
        }

        if (!typeof(TraceProcess).IsAssignableFrom(tracerClass))
        {
            throw SyscallTracerHostException.WrongTracerClass(tracerClass);
        }

        var tracerConstructor = tracerClass.GetConstructor([typeof(int)]);

        if (tracerConstructor == null)
        {
            throw SyscallTracerHostException.NoConstructorWithPID(tracerClass);
        }

        _tracerProcessFactory = () =>
        {
            try
            {
                return (TraceProcess)tracerConstructor.Invoke([tracedPID]);
            }
            catch (Exception e)
            {
                throw SyscallTracerHostException.CannotCreateTracer(e.Message);
            }
        };

        _syscallTracerServer = new(requestReader, responseWriter);
    }


    public static SyscallTracerHost ForConsoleApp(string[] args)
    {
        if (args.Length != 2)
        {
            throw SyscallTracerHostException.WrongArgs();
        }

        var tracedPIDArg = args[0];

        if (!int.TryParse(tracedPIDArg, out var tracedPID) || tracedPID <= 0)
        {
            throw SyscallTracerHostException.WrongTracedPIDFormat(tracedPIDArg);
        }

        var tracerClassArg = args[1];
        var tracerClass = LoadType(tracerClassArg);

        if (tracerClass == null)
        {
            throw SyscallTracerHostException.UnknownTracerClass(tracerClassArg);
        }

        return new(tracedPID, tracerClass);
    }

    private static Type? LoadType(string typeName)
    {
        try
        {
            var type = Type.GetType(typeName);

            if (type != null)
            {
                return type;
            }

            var parts = typeName.Split(',');

            if (parts.Length < 2)
            {
                return null;
            }

            var typeFullName = parts[0].Trim();
            var assemblyName = parts[1].Trim();

            var assembly = Assembly.Load(assemblyName);

            if (assembly == null)
            {
                return null;
            }

            return assembly.GetType(typeFullName);
        }
        catch
        {
            return null;
        }
    }


    public void Start()
    {
        using var traceProcess = _tracerProcessFactory();
        using var syscallTracer = new LocalSyscallTracer(traceProcess);
        syscallTracer.Initialize();

        _syscallTracerServer.RaiseReadyToTrace();

        ISyscallTrace? activeTrace = null;

        while (!_stopped)
        {
            var command = _syscallTracerServer.ReceiveTraceCommand();

            switch (command.Type)
            {
                case TraceCommandType.Start:
                    var testId = command.Args?.FirstOrDefault("") ?? "";
                    activeTrace = syscallTracer.Start(testId);
                    _syscallTracerServer.RaiseTraceStarted();
                    break;
                case TraceCommandType.Stop:
                    if (activeTrace != null)
                    {
                        try
                        {
                            var traceResult = activeTrace.Stop();
                            _syscallTracerServer.SendTraceResult(traceResult);
                        }
                        finally
                        {
                            activeTrace = null;
                        }
                    }
                    break;
                case TraceCommandType.Terminate:
                    _stopped = true;
                    break;
                default:
                    break;
            }
        }
    }

    public void Stop()
    {
        _stopped = true;
    }

    public void Dispose()
    {
        Stop();
    }


    public static int Main(string[] args)
    {
        try
        {
            using var host = ForConsoleApp(args);
            host.Start();
            return 0;
        }
        catch (SyscallTracerHostException e)
        {
            Console.Error.WriteLine(e.Message);
            return e.ErrorCode;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }
    }
}
