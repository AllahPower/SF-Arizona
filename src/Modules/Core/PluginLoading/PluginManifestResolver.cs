using System.Text.Json;

namespace SFSharp;

internal sealed class PluginManifestResolver
{
    public PluginManifestResolutionResult Resolve(string manifestPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(manifestPath);

        if (!File.Exists(manifestPath))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ManifestNotFound,
                $"Plugin manifest not found at '{manifestPath}'.");
        }

        PluginManifest? manifest;
        try
        {
            using FileStream stream = File.OpenRead(manifestPath);
            manifest = JsonSerializer.Deserialize(stream, PluginManifestJsonContext.Default.PluginManifest);
        }
        catch (JsonException ex)
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ManifestMalformedJson,
                $"Malformed plugin manifest '{manifestPath}': {ex.Message}");
        }
        catch (IOException ex)
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.IoFailure,
                $"Cannot read plugin manifest '{manifestPath}': {ex.Message}");
        }

        if (manifest is null)
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ManifestMalformedJson,
                $"Plugin manifest '{manifestPath}' could not be parsed.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Id))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ManifestMissingId,
                $"Plugin manifest '{manifestPath}' is missing required field 'id'.");
        }

        string pluginId = manifest.Id.Trim();
        if (!PluginManifestIdPolicy.IsValid(pluginId))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidPluginId,
                $"Plugin manifest '{manifestPath}' contains invalid plugin id '{pluginId}'.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Assembly))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ManifestMissingAssembly,
                $"Plugin manifest '{manifestPath}' is missing required field 'assembly'.");
        }

        string pluginRoot = Path.GetDirectoryName(Path.GetFullPath(manifestPath))!;
        string assemblyCandidate = manifest.Assembly.Trim();
        if (Path.IsPathRooted(assemblyCandidate))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidAssemblyPath,
                $"Plugin manifest '{manifestPath}' uses rooted assembly path '{assemblyCandidate}'.");
        }

        string assemblyPath = Path.GetFullPath(Path.Combine(pluginRoot, assemblyCandidate));
        if (!assemblyPath.StartsWith(pluginRoot, StringComparison.OrdinalIgnoreCase))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidAssemblyPath,
                $"Plugin manifest '{manifestPath}' resolves assembly path outside plugin root.");
        }

        if (!File.Exists(assemblyPath))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.AssemblyFileNotFound,
                $"Plugin assembly file not found at '{assemblyPath}'.");
        }

        Version? minHostVersion = null;
        if (!string.IsNullOrWhiteSpace(manifest.MinHostVersion) &&
            !Version.TryParse(manifest.MinHostVersion.Trim(), out minHostVersion))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidMinHostVersion,
                $"Plugin manifest '{manifestPath}' contains invalid minHostVersion '{manifest.MinHostVersion}'.");
        }

        PluginManifestMetadata metadata = new(
            Normalize(manifest.DisplayName),
            Normalize(manifest.Version),
            Normalize(manifest.Description),
            Normalize(manifest.Author),
            Normalize(manifest.Website));

        ResolvedPluginManifest resolved = new(
            pluginId,
            Path.GetFullPath(manifestPath),
            pluginRoot,
            assemblyPath,
            manifest.EnabledOnStart,
            minHostVersion,
            metadata,
            manifest);

        return PluginManifestResolutionResult.FromSuccess(resolved);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
