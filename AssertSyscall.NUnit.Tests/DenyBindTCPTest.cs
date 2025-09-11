using NUnit.Framework;
using System.Net;
using System.Net.Sockets;

namespace AssertSyscall.NUnit.Tests;

[TestFixture]
[AssertSyscall]
internal class DenyBindTCPTest
{
    [Test]
    [DenyBindTCP(excludePorts: [8080])]
    public void CanBindSpecificPortsOnly()
    {
        var server = new TcpListener(IPAddress.Any, 8080);
        server.Start();
        server.Dispose();
    }
}
