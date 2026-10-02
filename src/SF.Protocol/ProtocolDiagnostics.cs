using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SFSharp.Protocol;

/// <summary>
/// Log sink for parse failures inside SF.Protocol. The host points it at its own logger once at startup;
/// until then failures are only reported through the parse results.
/// </summary>
public static class ProtocolDiagnostics
{
    public static ILogger Logger { get; set; } = NullLogger.Instance;

    internal static void Error(string message) => Logger.Log(LogLevel.Error, default, message, null, static (text, _) => text);

    internal static void Warn(string message) => Logger.Log(LogLevel.Warning, default, message, null, static (text, _) => text);
}
