using NUnit.Framework;

namespace AssertSyscall.NUnit.Tests;

[TestFixture]
[AssertSyscall]
internal class DenyFileWriteTest
{
    [Test]
    [DenyFileWrite(excludePaths: ["/" + nameof(CanWriteToSpecificFilesOnly) + "/"])]
    public void CanWriteToSpecificFilesOnly()
    {
        // Given
        var dir = Path.Join(Path.GetTempPath(), nameof(CanWriteToSpecificFilesOnly));
        var file1 = Path.Join(dir, Guid.NewGuid().ToString());
        var file2 = Path.Join(dir, Guid.NewGuid().ToString());

        // When
        Directory.CreateDirectory(dir);
        File.WriteAllText(file1, "Content1");
        File.WriteAllText(file2, "Content2");
    }
}
