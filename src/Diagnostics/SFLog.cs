using System.Diagnostics;
using System.Text;

namespace SFSharp;

public static class SFLog
{
    private static readonly Lock _sync = new();
    private static readonly string _path = BuildPath();

    static SFLog()
    {
        Write("INFO", "===== session start =====");
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
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (!string.IsNullOrWhiteSpace(exePath))
            {
                var directory = System.IO.Path.GetDirectoryName(exePath);
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

    private static void Write(string level, string message)
    {
        try
        {
            lock (_sync)
            {
                var line = new StringBuilder(256)
                    .Append('[')
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"))
                    .Append("] [")
                    .Append(level)
                    .Append("] ")
                    .AppendLine(message)
                    .ToString();

                File.AppendAllText(_path, line, Encoding.UTF8);
            }
        }
        catch
        {
        }
    }
}
