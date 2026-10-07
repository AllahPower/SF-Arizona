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

        if (!TryResolveSharedAssemblies(manifest, pluginRoot, assemblyPath, out Dictionary<string, string> sharedAssemblyPaths, out string? sharedError))
        {
            return PluginManifestResolutionResult.FromFailure(
                PluginManifestResolutionFailureReason.InvalidSharedAssembly,
                $"Plugin manifest '{manifestPath}' has an invalid shared assembly: {sharedError}.");
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
            manifest)
        {
            SharedAssemblyPaths = sharedAssemblyPaths,
        };

        return PluginManifestResolutionResult.FromSuccess(resolved);
    }

    private static bool TryResolveSharedAssemblies(
        PluginManifest manifest,
        string pluginRoot,
        string assemblyPath,
        out Dictionary<string, string> paths,
        out string? error)
    {
        paths = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        error = null;
        string pluginAssemblyName = Path.GetFileNameWithoutExtension(assemblyPath);

        foreach (string? rawName in manifest.SharedAssemblies ?? [])
        {
            string name = rawName?.Trim() ?? string.Empty;
            if (!PluginManifestIdPolicy.IsValidAssemblyName(name))
            {
                error = $"'{rawName}' is not a simple assembly name";
                return false;
            }

            if (PluginSharedAssemblyPolicy.IsShared(name))
            {
                error = $"'{name}' is a host contract and is always shared by the host";
                return false;
            }

            // The plugin assembly itself must stay in its collectible context so the plugin can be unloaded.
            if (string.Equals(name, pluginAssemblyName, StringComparison.OrdinalIgnoreCase))
            {
                error = $"'{name}' is the plugin assembly itself";
                return false;
            }

            string path = Path.Combine(pluginRoot, name + ".dll");
            if (!File.Exists(path))
            {
                error = $"'{name}' was not found at '{path}'";
                return false;
            }

            if (!paths.TryAdd(name, path))
            {
                error = $"'{name}' is listed more than once";
                return false;
            }
        }

        // Shared assemblies live in the Default context, which cannot see the plugin folder, so such a reference
        // would otherwise fail only when another plugin first calls into it.
        Dictionary<string, string> shared = paths;
        foreach ((string name, string path) in shared)
        {
            if (!TryReadAssemblyReferences(path, out List<string> references))
            {
                error = $"'{name}' is not a readable managed assembly";
                return false;
            }

            string? local = references.FirstOrDefault(reference => !shared.ContainsKey(reference)
                && !PluginSharedAssemblyPolicy.IsShared(reference)
                && File.Exists(Path.Combine(pluginRoot, reference + ".dll")));
            if (local is not null)
            {
                error = $"'{name}' references '{local}' from the plugin folder, which is not shared; list '{local}' in sharedAssemblies too";
                return false;
            }
        }

        return true;
    }

    private static bool TryReadAssemblyReferences(string path, out List<string> references)
    {
        references = [];
        try
        {
            using FileStream stream = File.OpenRead(path);
            using PEReader peReader = new(stream);
            if (!peReader.HasMetadata)
            {
                return false;
            }

            MetadataReader reader = peReader.GetMetadataReader();
            foreach (AssemblyReferenceHandle handle in reader.AssemblyReferences)
            {
                references.Add(reader.GetString(reader.GetAssemblyReference(handle).Name));
            }

            return true;
        }
        catch (Exception ex) when (ex is IOException or BadImageFormatException or UnauthorizedAccessException)
        {
            return false;
        }
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
