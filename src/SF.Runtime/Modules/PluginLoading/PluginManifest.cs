using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SFSharp.Runtime.Modules.PluginLoading;

public sealed class PluginManifest
{
    public const string FileName = "manifest.json";

    /// <summary>Dependency key that refers to the SF host itself rather than to another plugin.</summary>
    public const string HostDependencyId = "sf";

    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

    [JsonPropertyName("version")]
    public string? Version { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("author")]
    public string? Author { get; set; }

    [JsonPropertyName("website")]
    public string? Website { get; set; }

    [JsonPropertyName("assembly")]
    public string? Assembly { get; set; }

    [JsonPropertyName("enabledOnStart")]
    public bool? EnabledOnStart { get; set; }

    /// <summary>Keyed by plugin id, or by <see cref="HostDependencyId"/> for the host.</summary>
    [JsonPropertyName("dependencies")]
    public Dictionary<string, PluginDependencyManifest>? Dependencies { get; set; }
}

public sealed class PluginDependencyManifest
{
    [JsonPropertyName("min")]
    public string? Min { get; set; }

    [JsonPropertyName("max")]
    public string? Max { get; set; }

    [JsonPropertyName("target")]
    public string? Target { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(PluginManifest))]
internal partial class PluginManifestJsonContext : JsonSerializerContext;

public sealed record PluginManifestMetadata(
    string? DisplayName,
    string? Version,
    string? Description,
    string? Author,
    string? Website);

public sealed record ResolvedPluginManifest(
    string PluginId,
    string ManifestPath,
    string PluginRoot,
    string AssemblyPath,
    bool? EnabledOnStartOverride,
    SemanticVersion Version,
    IReadOnlyList<ResolvedPluginDependency> Dependencies,
    IReadOnlyList<string> Warnings,
    PluginManifestMetadata Metadata,
    PluginManifest RawManifest)
{
    public string DisplayNameOrFallback => string.IsNullOrWhiteSpace(Metadata.DisplayName) ? PluginId : Metadata.DisplayName.Trim();

    public bool DependsOn(string pluginId)
        => Dependencies.Any(dependency => !dependency.IsHost && string.Equals(dependency.Id, pluginId, StringComparison.OrdinalIgnoreCase));
}

/// <param name="IsInferred">True when bounds were filled from the SF.Abstractions version the plugin was compiled against.</param>
public sealed record ResolvedPluginDependency(string Id, PluginDependencyRange Range, bool IsInferred)
{
    public bool IsHost => string.Equals(Id, PluginManifest.HostDependencyId, StringComparison.OrdinalIgnoreCase);
}

public enum PluginManifestResolutionFailureReason
{
    None,
    ManifestNotFound,
    ManifestMalformedJson,
    ManifestMissingId,
    ManifestMissingAssembly,
    InvalidPluginId,
    ReservedPluginId,
    InvalidAssemblyPath,
    AssemblyFileNotFound,
    MissingVersion,
    InvalidVersion,
    InvalidDependency,
    IoFailure,
}

public sealed record PluginManifestResolutionResult(
    bool Success,
    ResolvedPluginManifest? Manifest,
    PluginManifestResolutionFailureReason FailureReason,
    string Message)
{
    public static PluginManifestResolutionResult FromSuccess(ResolvedPluginManifest manifest)
    {
        return new(true, manifest, PluginManifestResolutionFailureReason.None, $"Resolved plugin manifest '{manifest.PluginId}'.");
    }

    public static PluginManifestResolutionResult FromFailure(PluginManifestResolutionFailureReason reason, string message)
    {
        return new(false, null, reason, message);
    }
}

internal static partial class PluginManifestIdPolicy
{
    [GeneratedRegex("^[A-Za-z0-9][A-Za-z0-9._-]{0,127}$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidPluginIdRegex();

    public static bool IsValid(string value) => ValidPluginIdRegex().IsMatch(value);
}
