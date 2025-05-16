using System.Text;

namespace AssertSyscall;

public record Syscall
(
    int Order,
    int ThreadId,
    SyscallCategory Category,
    SyscallType Type,
    string Func,
    IReadOnlyDictionary<string, string>? Args
) : IComparable<Syscall>
{
    public int CompareTo(Syscall? other)
    {
        if (other == null)
        {
            return 1;
        }

        if (Order > other.Order)
        {
            return 1;
        }

        if (Order < other.Order)
        {
            return -1;
        }

        return 0;
    }

    public override string ToString()
    {
        var result = new StringBuilder(Func).Append('(');

        if (Args != null && Args.Count > 0)
        {
            foreach (var item in Args)
            {
                result.AppendFormat("{0}=\"{1}\", ", item.Key, item.Value);
            }
            result.Remove(result.Length - 2, 2);
        }

        result.Append(')');

        return result.ToString();
    }
}
