using AssertSyscall.NUnit.Tests;
using AssertSyscall.Tracing;
using AssertSyscall.Tracing.Linux;

using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace AssertSyscall.NUnit;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AssertSyscallAttribute : Attribute, ITestAction
{
    private ISyscallTracer? _tracer;
    private ISyscallTrace? _trace;

    public ActionTargets Targets => ActionTargets.Suite | ActionTargets.Test;

    public void BeforeTest(ITest test)
    {
        if (test.IsSuite)
        {
            if (!OperatingSystem.IsLinux())
            {
                TestContext.Error.WriteLine($"AssertSyscall supports Linux only.");
                return;
            }

            if (!test.Tests.Any(IsSyscallConstraint))
            {
                TestContext.Out.WriteLine($"AssertSyscall did not find any syscall constraints.");
                return;
            }

            _tracer = new RemoteSyscallTracer<STraceProcess>();
            _tracer.Initialize();
        }
        else if (_tracer != null && IsSyscallConstraint(test))
        {
            _trace = _tracer.Start(test.FullName);
        }
    }

    public void AfterTest(ITest test)
    {
        if (test.IsSuite)
        {
            _tracer?.Dispose();
        }
        else if (_trace != null)
        {
            var traceResult = _trace.Stop();

            var constrains = test.Method!.GetCustomAttributes<SyscallConstraintAttribute>(true)
                .Select(i => i.CreateConstraint())
                .ToList();

            foreach (var constraint in constrains)
            {
                var violations = constraint.FindViolations(traceResult.Calls);
                Assert.That(violations, Is.Null.Or.Empty);
            }
        }
    }

    private static bool IsSyscallConstraint(ITest test)
    {
        return (test.Method != null && test.Method.IsDefined<SyscallConstraintAttribute>(true));

    }
}
