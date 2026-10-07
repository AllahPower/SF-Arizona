namespace SFSharp.Runtime.Modules.PluginLoading;

/// <summary>
/// Runtime record of a plugin loaded via <see cref="PluginLoader"/>. Tracks the isolated
/// <see cref="PluginLoadContext"/> and the list of module ids registered from that assembly so
/// they can be removed together on unload.
/// </summary>
internal sealed class LoadedPlugin
{
    public required ResolvedPluginManifest Manifest { get; init; }
    public required PluginLoadContext? LoadContext { get; set; }
    public required IReadOnlyList<string> RegisteredModuleIds { get; init; }
    public required WeakReference LoadContextRef { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
    public EarlyModuleHost? EarlyModules { get; set; }
    public PluginState State { get; set; } = PluginState.Loaded;
    public PluginUnloadProgress UnloadProgress { get; set; } = PluginUnloadProgress.None;
    public PluginUnloadFailureReason LastUnloadFailureReason { get; set; } = PluginUnloadFailureReason.None;
    public string? LastUnloadFailureMessage { get; set; }

    public string PluginId => Manifest.PluginId;
    public string ManifestPath => Manifest.ManifestPath;
    public string AssemblyPath => Manifest.AssemblyPath;
    public int RegisteredModuleCount => RegisteredModuleIds.Count;
    public int EarlyModuleCount => EarlyModules?.Count ?? 0;

    /// <summary>True while the plugin's modules or assemblies may still be used by other plugins.</summary>
    public bool IsActive => UnloadProgress < PluginUnloadProgress.ContextUnloaded;

    public PluginRuntimeSnapshot CreateSnapshot() => new(
        PluginId,
        Manifest.DisplayNameOrFallback,
        Manifest.Version,
        Warnings,
        State,
        RegisteredModuleCount,
        EarlyModuleCount,
        LastUnloadFailureReason,
        LastUnloadFailureMessage);
}

/// <summary>Unload steps already completed, so a retried unload resumes instead of starting over.</summary>
internal enum PluginUnloadProgress
{
    None,
    ModulesUnregistered,
    ContextUnloaded,
}
