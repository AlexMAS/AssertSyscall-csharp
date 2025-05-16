using NUnit.Framework;

namespace AssertSyscall.Tracing.Linux;

[TestFixture]
internal class STraceOutputParserTest
{
    private readonly STraceOutputParser target = new();

    [Test]
    public void ShouldParseOpenAt()
    {
        // Given
        var traceLine = "openat(AT_FDCWD</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0>, \"/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb\", O_RDONLY|O_CLOEXEC) = 171</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb>";

        // When
        var syscall = target.Parse(traceLine);

        // Then
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall!.ThreadId, Is.EqualTo(0));
        Assert.That(syscall!.Func, Is.EqualTo("open"));
        Assert.That(syscall!.Args, Is.Not.Null);
        Assert.That(syscall!.Args, Has.Count.EqualTo(1));
        Assert.That(syscall!.Args!["path"], Is.EqualTo("/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb"));
    }

    [Test]
    public void ShouldParseOpenAtWithDescriptor()
    {
        // Given
        var traceLine = "openat(12345</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0>, \"/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb\", O_RDONLY|O_CLOEXEC) = 171</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb>";

        // When
        var syscall = target.Parse(traceLine);

        // Then
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall!.ThreadId, Is.EqualTo(0));
        Assert.That(syscall!.Func, Is.EqualTo("open"));
        Assert.That(syscall!.Args, Is.Not.Null);
        Assert.That(syscall!.Args, Has.Count.EqualTo(1));
        Assert.That(syscall!.Args!["path"], Is.EqualTo("/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb"));
    }

    [Test]
    public void ShouldParseMultiThreadedOpenAt()
    {
        // Given
        var traceLine = "[pid   123<NUnit.Fw.NonPar>] openat(AT_FDCWD</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0>, \"/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb\", O_RDONLY|O_CLOEXEC) = 171</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb>";

        // When
        var syscall = target.Parse(traceLine);

        // Then
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall!.ThreadId, Is.EqualTo(123));
        Assert.That(syscall!.Func, Is.EqualTo("open"));
        Assert.That(syscall!.Args, Is.Not.Null);
        Assert.That(syscall!.Args, Has.Count.EqualTo(1));
        Assert.That(syscall!.Args!["path"], Is.EqualTo("/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb"));
    }

    [Test]
    public void ShouldParseMultiThreadedOpenAtWithDescriptor()
    {
        // Given
        var traceLine = "[pid   123<NUnit.Fw.NonPar>] openat(12345</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0>, \"/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb\", O_RDONLY|O_CLOEXEC) = 171</src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb>";

        // When
        var syscall = target.Parse(traceLine);

        // Then
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall!.ThreadId, Is.EqualTo(123));
        Assert.That(syscall!.Func, Is.EqualTo("open"));
        Assert.That(syscall!.Args, Is.Not.Null);
        Assert.That(syscall!.Args, Has.Count.EqualTo(1));
        Assert.That(syscall!.Args!["path"], Is.EqualTo("/src/AssertSyscall.NUnit.Tests/bin/Debug/net8.0/AssertSyscall.NUnit.Tests.pdb"));
    }

    [Test]
    public void ShouldParseUnknownFunction()
    {
        // Given
        var traceLine = "[pid   123<NUnit.Fw.NonPar>] some_func() = 0";

        // When
        var syscall = target.Parse(traceLine);

        // Then
        Assert.That(syscall, Is.Not.Null);
        Assert.That(syscall!.ThreadId, Is.EqualTo(123));
        Assert.That(syscall!.Func, Is.EqualTo("some_func"));
        Assert.That(syscall!.Args, Is.Null);
    }
}
