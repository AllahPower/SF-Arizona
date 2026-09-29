using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace SFSharp.Runtime.Modules;

public sealed class PluginManifest
{
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

    [JsonPropertyName("minHostVersion")]
    public string? MinHostVersion { get; set; }

    [JsonPropertyName("enabledOnStart")]
    public bool? EnabledOnStart { get; set; }
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
    Version? MinHostVersion,
    PluginManifestMetadata Metadata,
    PluginManifest RawManifest)
{
    public string DisplayNameOrFallback => string.IsNullOrWhiteSpace(Metadata.DisplayName) ? PluginId : Metadata.DisplayName.Trim();
}

public enum PluginManifestResolutionFailureReason
{
    None,
    ManifestNotFound,
    ManifestMalformedJson,
    ManifestMissingId,
    ManifestMissingAssembly,
    InvalidPluginId,
    InvalidAssemblyPath,
    AssemblyFileNotFound,
    InvalidMinHostVersion,
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
