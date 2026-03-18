using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace SFSharp;

public static class SFLog
{
    private static readonly string _path = BuildPath();
    private static readonly ConcurrentQueue<string> _pendingLines = new();
    private static readonly AutoResetEvent _signal = new(false);
    private static readonly object _writerSync = new();
    private static readonly Thread _worker;
    private static StreamWriter? _writer;
    private static volatile bool _running = true;

    static SFLog()
    {
        ResetSessionLog();
        _worker = new Thread(WorkerLoop)
        {
            IsBackground = true,
            Name = "SFLogWriter"
        };
        _worker.Start();
        Write("INFO", "===== session start =====");
        AppDomain.CurrentDomain.ProcessExit += static (_, _) => Shutdown();
    }

    public static string Path => _path;

    public static void Info(string message) => Write("INFO", message);
    public static void Warn(string message) => Write("WARN", message);
    public static void Error(string message) => Write("ERROR", message);

    public static void Error(Exception ex, string context)
    {
        Write("ERROR", $"{context}: {ex}");
    }

    private static string BuildPath()
    {
        try
        {
            string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                string? directory = System.IO.Path.GetDirectoryName(exePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    return System.IO.Path.Combine(directory, "sf_arz.log");
                }
            }
        }
        catch
        {
        }

        return System.IO.Path.Combine(AppContext.BaseDirectory, "sf_arz.log");
    }

    private static void ResetSessionLog()
    {
        try
        {
            string? directory = System.IO.Path.GetDirectoryName(_path);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(_path, string.Empty, Encoding.UTF8);
        }
        catch
        {
        }
    }

    private static void Write(string level, string message)
    {
        try
        {
            string line = new StringBuilder(256)
                .Append('[')
                .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                .Append("] [")
                .Append(level)
                .Append("] ")
                .AppendLine(message)
                .ToString();

            _pendingLines.Enqueue(line);
            _signal.Set();
        }
        catch
        {
        }
    }

    private static void WorkerLoop()
    {
        while (_running)
        {
            _signal.WaitOne();
            DrainQueue();
        }

        DrainQueue();
        CloseWriter();
    }

    private static void DrainQueue()
    {
        try
        {
            lock (_writerSync)
            {
                StreamWriter writer = EnsureWriter();
                while (_pendingLines.TryDequeue(out string? line))
                {
                    writer.Write(line);
                }

                writer.Flush();
            }
        }
        catch
        {
        }
    }

    private static StreamWriter EnsureWriter()
    {
        if (_writer is not null)
        {
            return _writer;
        }

        FileStream stream = new(_path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
        _writer = new StreamWriter(stream, new UTF8Encoding(false))
        {
            AutoFlush = false
        };
        return _writer;
    }

    private static void Shutdown()
    {
        if (!_running)
        {
            return;
        }

        _running = false;
        _signal.Set();
        try
        {
            _worker.Join(1000);
        }
        catch
        {
        }
    }

    private static void CloseWriter()
    {
        lock (_writerSync)
        {
            if (_writer is null)
            {
                return;
            }

            try
            {
                _writer.Dispose();
            }
            catch
            {
            }
            finally
            {
                _writer = null;
            }
        }
    }
}
