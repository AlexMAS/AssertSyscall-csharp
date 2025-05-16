namespace AssertSyscall;

public enum SyscallCategory : int
{
    Undefined = 0,
    Credentials = 1,
    Process = 2,
    File = 3,
    IPC = 4,
    Network = 5,
    Memory = 6,
    Clock = 7
}
