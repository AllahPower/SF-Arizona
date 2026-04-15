using System.Reflection;

namespace SFSharp;

/// <summary>
/// Runtime record of a plugin loaded via <see cref="PluginLoader"/>. Tracks the isolated
/// <see cref="PluginLoadContext"/>, the main assembly and the list of module ids registered
/// from that assembly so they can be removed together on unload.
/// </summary>
internal sealed class LoadedPlugin
{
    public required string PluginId { get; init; }
    public required string ManifestPath { get; init; }
    public required string AssemblyPath { get; init; }
    public required PluginLoadContext LoadContext { get; init; }
    public required Assembly Assembly { get; init; }
    public required List<string> RegisteredModuleIds { get; init; }
    public required WeakReference LoadContextRef { get; init; }
}
