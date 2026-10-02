using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace SFSharp.Runtime.Modules.PluginLoading;

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

        if (string.Equals(pluginId, PluginManifest.HostDependencyId, StringComparison.OrdinalIgnoreCase))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.ReservedPluginId,
                $"Plugin manifest '{manifestPath}' uses id '{pluginId}', which is reserved for the SF host.");
        }

        if (string.IsNullOrWhiteSpace(manifest.Version))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.MissingVersion,
                $"Plugin manifest '{manifestPath}' is missing required field 'version'.");
        }

        if (!SemanticVersion.TryParse(manifest.Version, out SemanticVersion? version))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidVersion,
                $"Plugin manifest '{manifestPath}' contains version '{manifest.Version}', which is not SemVer 2.0.");
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

        List<string> warnings = [];
        if (!TryResolveDependencies(manifest, pluginId, assemblyPath, warnings, out List<ResolvedPluginDependency> dependencies, out string? dependencyError))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidDependency,
                $"Plugin manifest '{manifestPath}' has an invalid dependency: {dependencyError}.");
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
            version!,
            dependencies,
            warnings,
            metadata,
            manifest);

        return PluginManifestResolutionResult.FromSuccess(resolved);
    }

    private static bool TryResolveDependencies(
        PluginManifest manifest,
        string pluginId,
        string assemblyPath,
        List<string> warnings,
        out List<ResolvedPluginDependency> dependencies,
        out string? error)
    {
        dependencies = [];
        error = null;
        PluginDependencyRange? hostRange = null;

        foreach ((string rawId, PluginDependencyManifest? declared) in manifest.Dependencies ?? [])
        {
            string id = rawId.Trim();
            bool isHost = string.Equals(id, PluginManifest.HostDependencyId, StringComparison.OrdinalIgnoreCase);
            if (!isHost && (!PluginManifestIdPolicy.IsValid(id) || string.Equals(id, pluginId, StringComparison.OrdinalIgnoreCase)))
            {
                error = $"'{rawId}' is not a valid plugin id or refers to the plugin itself";
                return false;
            }

            if (!PluginDependencyRange.TryParse(declared?.Min, declared?.Max, declared?.Target, out PluginDependencyRange? range, out string? rangeError))
            {
                error = $"'{id}': {rangeError}";
                return false;
            }

            if (isHost)
            {
                hostRange = range;
            }
            else
            {
                dependencies.Add(new ResolvedPluginDependency(id, range!, IsInferred: false));
            }
        }

        // Bounds the manifest left out default to the SF.Abstractions version the plugin was compiled against.
        SemanticVersion? compiled = TryReadReferencedVersion(assemblyPath, "SF.Abstractions");
        if (compiled is null)
        {
            if (hostRange is null)
            {
                warnings.Add("the plugin does not reference SF.Abstractions directly, so the host version is not checked");
                return true;
            }

            dependencies.Insert(0, new ResolvedPluginDependency(PluginManifest.HostDependencyId, hostRange, IsInferred: false));
            return true;
        }

        PluginDependencyRange effective = (hostRange ?? PluginDependencyRange.Empty).WithDefaults(compiled);
        string? effectiveError = effective.Validate();
        if (effectiveError is not null)
        {
            error = $"'{PluginManifest.HostDependencyId}' after filling defaults from SF.Abstractions {compiled}: {effectiveError}";
            return false;
        }

        dependencies.Insert(0, new ResolvedPluginDependency(PluginManifest.HostDependencyId, effective, IsInferred: hostRange is null));
        return true;
    }

    private static SemanticVersion? TryReadReferencedVersion(string assemblyPath, string referencedName)
    {
        try
        {
            using FileStream stream = File.OpenRead(assemblyPath);
            using PEReader peReader = new(stream);
            if (!peReader.HasMetadata)
            {
                return null;
            }

            MetadataReader reader = peReader.GetMetadataReader();
            foreach (AssemblyReferenceHandle handle in reader.AssemblyReferences)
            {
                AssemblyReference reference = reader.GetAssemblyReference(handle);
                if (string.Equals(reader.GetString(reference.Name), referencedName, StringComparison.OrdinalIgnoreCase))
                {
                    return SemanticVersion.FromAssemblyVersion(reference.Version);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException)
        {
            return null;
        }

        return null;
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
