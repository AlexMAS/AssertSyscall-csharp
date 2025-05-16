using System.Diagnostics;

namespace AssertSyscall.Tracing;

internal class ManagedProcess(string command, IEnumerable<string>? arguments = null) : IDisposable
{
    private readonly string _command = command;
    private readonly IEnumerable<string>? _arguments = arguments;
    private readonly TaskCompletionSource<bool> _standardOutputClosed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> _standardErrorClosed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<int> _processResult = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private volatile Process? _process;

    public StreamWriter? StandardInput
    {
        get
        {
            var process = _process;
            return process?.StandardInput;
        }
    }

    public event Action<string>? StandardOutputLines;

    public event Action<string>? StandardErrorLines;

    public StreamReader? StandardOutput => _process?.StandardOutput;

    public StreamReader? StandardError => _process?.StandardError;

    public bool IsCompleted => _process?.HasExited == true || _processResult.Task.IsCompleted;

    public Exception? CompletionReason => _processResult.Task.Exception?.GetBaseException();


    public bool Start()
    {
        var process = new Process();
        process.StartInfo.FileName = _command;

        if (_arguments?.Count() > 0)
        {
            process.StartInfo.Arguments = string.Join(' ', _arguments);
        }

        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;

        if (StandardOutputLines != null)
        {
            process.OutputDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    _standardOutputClosed.TrySetResult(true);
                }
                else
                {
                    StandardOutputLines(e.Data);
                }
            };
        }
        else
        {
            _standardOutputClosed.TrySetResult(false);
        }

        if (StandardErrorLines != null)
        {
            process.ErrorDataReceived += (s, e) =>
            {
                if (string.IsNullOrEmpty(e.Data))
                {
                    _standardErrorClosed.TrySetResult(true);
                }
                else
                {
                    StandardErrorLines(e.Data);
                }
            };
        }
        else
        {
            _standardErrorClosed.TrySetResult(false);
        }

        try
        {
            process.Start();

            if (StandardOutputLines != null)
            {
                process.BeginOutputReadLine();
            }

            if (StandardErrorLines != null)
            {
                process.BeginErrorReadLine();
            }

            _process = process;

            return true;
        }
        catch (Exception e)
        {
            TerminateProcess(-1, killProcess: false, reason: e);
            return false;
        }
    }


    public void Stop()
    {
        TerminateProcess(-1, killProcess: true, reason: null);
    }

    private void TerminateProcess(int exitCode, bool killProcess, Exception? reason)
    {
        if (_process != null)
        {
            lock (this)
            {
                if (_process != null)
                {
                    var process = _process;

                    _process = null;

                    try
                    {
                        if (killProcess)
                        {
                            try
                            {
                                process.Kill(true);
                            }
                            catch
                            {
                                // Ignore
                            }
                        }
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }

        if (reason == null)
        {
            _processResult.TrySetResult(exitCode);
        }
        else
        {
            _processResult.TrySetException(reason);
        }
    }


    public async Task<int> WaitForExitAsync()
    {
        var process = _process;

        if (process != null)
        {
            await Task.WhenAll(
                    process.WaitForExitAsync(),
                    _standardOutputClosed.Task,
                    _standardErrorClosed.Task);

            TerminateProcess(process.ExitCode, killProcess: false, reason: null);
        }

        return await _processResult.Task;
    }


    public void Dispose()
    {
        Stop();
    }
}
