using System.Globalization;
using System.Reflection;

namespace SFSharp.Runtime.Diagnostics;

public static class RuntimeBuildInfo
{
    public const string ProductName = "SF-Arizona";

    private static readonly Assembly RuntimeAssembly = typeof(RuntimeBuildInfo).Assembly;

    /// <summary>SemVer part of the informational version, without the source revision suffix.</summary>
    public static string Version { get; }

    /// <summary>Short source revision embedded by the SDK, or null for builds outside a git checkout.</summary>
    public static string? Commit { get; }

    public static DateTimeOffset? BuildTimestamp { get; }

    static RuntimeBuildInfo()
    {
        string informational = RuntimeAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? RuntimeAssembly.GetName().Version?.ToString()
            ?? "unknown";

        int separator = informational.IndexOf('+');
        Version = separator < 0 ? informational : informational[..separator];
        Commit = separator < 0 ? null : informational[(separator + 1)..] is { Length: > 0 } revision
            ? revision[..Math.Min(revision.Length, 7)]
            : null;

        string? timestamp = RuntimeAssembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(static attribute => attribute.Key == "BuildTimestamp")?.Value;
        BuildTimestamp = DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTimeOffset parsed)
            ? parsed
            : null;
    }

    public static string DisplayVersion => Commit is null ? Version : $"{Version} ({Commit})";
}
