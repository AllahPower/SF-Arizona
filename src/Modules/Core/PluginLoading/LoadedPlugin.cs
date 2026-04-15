using System.Reflection;

namespace SFSharp;

/// <summary>
/// Runtime record of a plugin loaded via <see cref="PluginLoader"/>. Tracks the isolated
/// <see cref="PluginLoadContext"/>, the main assembly and the list of module ids registered
/// from that assembly so they can be removed together on unload.
/// </summary>
internal sealed class LoadedPlugin
{
    public required ResolvedPluginManifest Manifest { get; init; }
    public required PluginLoadContext LoadContext { get; init; }
    public required Assembly Assembly { get; init; }
    public required List<ModuleDescriptor> RegisteredModules { get; init; }
    public required WeakReference LoadContextRef { get; init; }
    public PluginState State { get; set; } = PluginState.Loaded;
    public PluginUnloadFailureReason LastUnloadFailureReason { get; set; } = PluginUnloadFailureReason.None;
    public string? LastUnloadFailureMessage { get; set; }

    public string PluginId => Manifest.PluginId;
    public string ManifestPath => Manifest.ManifestPath;
    public string AssemblyPath => Manifest.AssemblyPath;
    public IReadOnlyList<string> RegisteredModuleIds => RegisteredModules.Select(static descriptor => descriptor.Id).ToArray();
    public PluginRuntimeSnapshot CreateSnapshot() => new(
        PluginId,
        Manifest.DisplayNameOrFallback,
        State,
        RegisteredModules.Count,
        LastUnloadFailureReason,
        LastUnloadFailureMessage);
}
