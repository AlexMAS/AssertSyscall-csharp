using System.Text.RegularExpressions;
using System.Text;

namespace AssertSyscall.Tracing.Linux;

internal partial class STraceOutputParser
{
    private static readonly Dictionary<string, FuncDescription> KnownFunctions = [];

    private static readonly Dictionary<string, FuncArg> IgnoredFunctions = new()
    {
        // original func name - arg name - arg value
        { "mmap", new("path", "/memfd:doublemapper") },
        { "mprotect", FuncArg.ANY },
        { "munmap", FuncArg.ANY },
    };

    private int _order = -1;

    public Syscall? Parse(string traceLine)
    {
        if (string.IsNullOrWhiteSpace(traceLine))
        {
            return null;
        }

        var match = FunctionCallRegex().Match(traceLine);

        if (match == null || !match.Success)
        {
            return null;
        }

        var func = match.Groups["func"];

        if (func == null || !func.Success)
        {
            return null;
        }

        if (IgnoredFunctions.TryGetValue(func.Value, out var ignoredFuncArg) && ignoredFuncArg == FuncArg.ANY)
        {
            return null;
        }

        var pid = match.Groups["pid"];

        if (pid == null || !pid.Success || !int.TryParse(pid.Value, out var threadId))
        {
            threadId = 0;
        }

        var order = Interlocked.Increment(ref _order);

        if (!KnownFunctions.TryGetValue(func.Value, out var funcDescription))
        {
            return new(order, threadId, SyscallCategory.Undefined, SyscallType.Undefined, func.Value, null);
        }

        var args = match.Groups["args"];
        var syscall = funcDescription.CreateSyscall(order, threadId, (args == null || !args.Success) ? null : args.Value);

        if (ignoredFuncArg != null && syscall.IsFuncCall(syscall.Func, ignoredFuncArg.Arg, ignoredFuncArg.Value))
        {
            Interlocked.Decrement(ref _order);
            return null;
        }

        return syscall;
    }


    // func(agrs) = result
    // func(agrs <unfinished ...>
    // [pid NNN] func(agrs) = result
    // [pid NNN] func(agrs <unfinished ...>
    // See https://man7.org/linux/man-pages/man1/strace.1.html
    [GeneratedRegex("^(\\[pid\\s+(?<pid>\\d+).*?\\]\\s+){0,1}(?<func>.+?)\\s*\\(\\s*(?<args>.*?){0,1}\\s*((\\)\\s*=\\s*(?<result>.*?))|(\\<unfinished\\s*\\.\\.\\.\\>))$", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
    private static partial Regex FunctionCallRegex();


    private record FuncArg(string? Arg = null, string? Value = null)
    {
        public static readonly FuncArg ANY = new(null, null);
    }


    private class FuncDescription
    (
        SyscallCategory Category,
        SyscallType Type,
        string Name,
        FuncParams Params
    )
    {
        public Syscall CreateSyscall(int order, int threadId, string? args)
        {
            return new(order, threadId, Category, Type, Name, Params.BuildArgs(args));
        }
    }


    private partial class FuncParams
    {
        private readonly Dictionary<string, Func<IReadOnlyList<string>, string>> _params = [];

        public FuncParams Add(string name, Func<IReadOnlyList<string>, string> format)
        {
            _params[name] = format;
            return this;
        }

        public IReadOnlyDictionary<string, string>? BuildArgs(string? args)
        {
            if (_params.Count == 0)
            {
                return null;
            }

            var listOfArgs = ParseArguments(args);

            return _params.ToDictionary(i => i.Key, i => i.Value(listOfArgs));
        }

        private static IReadOnlyList<string> ParseArguments(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return [];
            }

            var arguments = new List<string>();
            var currentArg = new StringBuilder();
            var curlyBraces = new Stack<int>();
            var insideQuotes = false;
            var insideCurlyBraces = false;

            for (var i = 0; i < input.Length; ++i)
            {
                var ch = input[i];

                switch (ch)
                {
                    case '"':
                        insideQuotes = !insideQuotes;
                        break;
                    case ',':
                        if (!insideQuotes && !insideCurlyBraces)
                        {
                            arguments.Add(DecodeArgument(currentArg.ToString()));
                            currentArg.Clear();
                        }
                        else
                        {
                            currentArg.Append(ch);
                        }
                        break;
                    case '{':
                        if (!insideQuotes)
                        {
                            insideCurlyBraces = true;
                            curlyBraces.Push(i);
                        }
                        currentArg.Append(ch);
                        break;
                    case '}':
                        if (!insideQuotes)
                        {
                            if (curlyBraces.Count > 0)
                            {
                                curlyBraces.Pop();
                            }
                            if (curlyBraces.Count == 0)
                            {
                                insideCurlyBraces = false;
                            }
                        }
                        currentArg.Append(ch);
                        break;
                    default:
                        currentArg.Append(ch);
                        break;
                }
            }

            if (currentArg.Length > 0)
            {
                arguments.Add(DecodeArgument(currentArg.ToString()));
            }

            return arguments;
        }

        private static string DecodeArgument(string argument)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                return argument;
            }

            var desc = FileDescriptorRegex().Match(argument);

            if (desc != null && desc.Success)
            {
                var path = desc.Groups["path"];

                if (path.Success)
                {
                    return path.Value;
                }
            }

            return argument.Trim();
        }


        [GeneratedRegex("\\w+\\s*\\<\\s*(?<path>.*?)\\s*\\>", RegexOptions.Compiled | RegexOptions.CultureInvariant)]
        private static partial Regex FileDescriptorRegex();
    }
}
