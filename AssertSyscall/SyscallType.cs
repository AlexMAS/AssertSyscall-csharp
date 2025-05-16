namespace AssertSyscall;

public enum SyscallType : int
{
    Undefined = 0,
    Stat = 1,
    Read = 2,
    Modify = 3,
    Create = 4,
    Delete = 5
}
