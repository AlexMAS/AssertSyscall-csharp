namespace AssertSyscall.Tracing;

internal class SyscallTraceAnchor
{
    private const string TEST_ANCHOR_FUNC = "stat";
    private const string TEST_ANCHOR_ARG = "path";

    private readonly string _testAnchor = "/" + Guid.NewGuid().ToString();
    private bool _hasStartAnchor;
    private bool _hasStopAnchor;

    public void DropStartAnchor()
    {
        _hasStartAnchor = DropAnchor();
    }

    public void DropStopAnchor()
    {
        _hasStopAnchor = DropAnchor();
    }

    private bool DropAnchor()
    {
        if (OperatingSystem.IsLinux())
        {
            try
            {
                // It's the easiest way to make a NOP call - "stat(testId)" - to see where the test starts and stops.
                File.Exists(_testAnchor);
                return true;
            }
            catch { }
        }

        return false;
    }

    public IEnumerable<Syscall> Filter(IEnumerable<Syscall> syscalls)
    {
        if (_hasStartAnchor)
        {
            // Ignore syscalls before the test run
            syscalls = syscalls.SkipWhile(i => !i.IsFuncCall(TEST_ANCHOR_FUNC, TEST_ANCHOR_ARG, _testAnchor)).Skip(1);
        }

        if (_hasStopAnchor)
        {
            // Ignore syscalls after the test run
            syscalls = syscalls.TakeWhile(i => !i.IsFuncCall(TEST_ANCHOR_FUNC, TEST_ANCHOR_ARG, _testAnchor));
        }

        return (_hasStartAnchor || _hasStopAnchor) ? syscalls.ToList() : syscalls;
    }
}
