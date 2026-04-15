using Microsoft.Extensions.Logging;

namespace SFSharp;

public sealed class SFLoggerProvider : ILoggerProvider
{
    public static SFLoggerProvider Instance { get; } = new();

    public ILogger CreateLogger(string categoryName)
    {
        return new SFLogger(categoryName);
    }

    public void Dispose()
    {
    }
}

internal sealed class SFLogger(string categoryName) : ILogger
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Debug;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        string level = GetLevelTag(logLevel);
        string message = formatter(state, exception);

        string line = exception is not null
            ? $"[{timestamp}] [{level}] [{categoryName}] {message}: {exception}"
            : $"[{timestamp}] [{level}] [{categoryName}] {message}";

        SFLog.WriteFormatted(line);
    }

    private static string GetLevelTag(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "TRCE",
        LogLevel.Debug => "DBUG",
        LogLevel.Information => "INFO",
        LogLevel.Warning => "WARN",
        LogLevel.Error => "FAIL",
        LogLevel.Critical => "CRIT",
        _ => "INFO"
    };
}
