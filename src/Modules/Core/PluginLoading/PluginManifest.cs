using System.Text.Json.Serialization;

namespace SFSharp;

public sealed class PluginManifest
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("displayName")]
    public string? DisplayName { get; set; }

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
