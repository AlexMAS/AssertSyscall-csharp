namespace AssertSyscall.Tracing;

internal class SyscallTracerHostException(int errorCode, string message) : InvalidOperationException(message)
{
    public int ErrorCode { get; private set; } = errorCode;

    public static SyscallTracerHostException WrongArgs() => new(200, "The first arg must be a traced PID, the second one - a tracer class.");
    public static SyscallTracerHostException WrongTracedPIDFormat(string tracedPID) => new(201, $"The traced PID must be a positive integer: '{tracedPID}'.");
    public static SyscallTracerHostException UnknownTracerClass(string tracerClass) => new(202, $"The class cannot be found: '{tracerClass}'.");
    public static SyscallTracerHostException TracedProcessNotFound(int tracedPID) => new(203, $"The traced process not found: {tracedPID}.");
    public static SyscallTracerHostException WrongTracerClass(Type tracerClass) => new(204, $"The class is not the tracer: '{tracerClass.FullName}'.");
    public static SyscallTracerHostException NoConstructorWithPID(Type tracerClass) => new(205, $"The class must have the constructor which accept PID: '{tracerClass.FullName}'.");
    public static SyscallTracerHostException CannotCreateTracer(string reason) => new(206, $"Cannot create the tracer: '{reason}'.");
    public static SyscallTracerHostException CannotStartTracing() => new(207, $"The given process cannot be traced.");
}
