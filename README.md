# AssertSyscall

Verifies calls to the Linux API (syscalls) made by a test case.

Modern apps use a lot of external dependencies. At the same time, in most cases,
an app CI/CD pipeline verifies the app code only. Thus we can be sure that
the app code is safe, which cannot be said about used external code.

Using this library, you can declare the app/environment contract and be sure
that the app complies it. It makes your expectations transparent, eliminates
some zero-day vulnerabilities, and minimizes the possible attack surface.
And all of this is easy to inject to the existing CI/CD and tests.
All you need is to mark *existing* test cases with the `AssertSyscall` and define
your own rules - the contract between your app and the environment (OS).

## Requirements

* Linux only.
* The [`strace`](https://strace.io/) must be pre-installed.

```sh
apt update && apt install -y strace
```

## Usage example

The next example shows how to check that the tested code
writes to the `/output` directory only.

```csharp
namespace AssertSyscall.NUnit.Tests;

[TestFixture]
[AssertSyscall] // <1>
class DenyFileWriteTest
{
    [Test]
    [DenyFileWrite(excludePaths: ["/output"])] // <2>
    public void ShouldWriteToOutputDirOnly()
    {
        var dir = "/output";
        var file = Path.Join(dir, "some.txt");
        File.WriteAllText(file, "Some Content");
    }
}
```

1. The `AssertSyscall` attribute marks that the test class verifies syscalls.
2. The `DenyFileWrite` attribute defines a rule to verify syscalls made by the test case.

> **INFO**
>
> This example uses the [`AssertSyscall.NUnit`](https://www.nuget.org/packages/AssertSyscall.NUnit) package.

## Build

To build the package run:

```sh
./build.sh
```

The `bin/publish/` directory will contain nuget-packages.
