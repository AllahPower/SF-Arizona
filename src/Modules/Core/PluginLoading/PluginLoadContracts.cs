namespace SFSharp;

public enum PluginState
{
    Loaded,
    Unloading,
    UnloadFailed,
}

public enum PluginLoadFailureReason
{
    None,
    DynamicCodeUnsupported,
    ManifestResolutionFailed,
    AlreadyLoaded,
    AssemblyLoadFailed,
    TypeEnumerationFailed,
    NoModulesFound,
    ModuleRegistrationFailed,
    MissingManagedDependency,
    MissingNativeDependency,
    Busy,
    UnexpectedError,
}

public enum PluginUnloadFailureReason
{
    None,
    PluginNotLoaded,
    PluginBusy,
    ModuleStopTimeout,
    ModuleStillRunning,
    ModuleUnregisterFailed,
    AssemblyLoadContextUnloadFailed,
    AssemblyLoadContextStillAlive,
    UnexpectedError,
}

public sealed record PluginLoadResult(
    bool Success,
    string? PluginId,
    int RegisteredModuleCount,
    PluginLoadFailureReason FailureReason,
    string Message,
    ResolvedPluginManifest? Manifest)
{
    public static PluginLoadResult FromSuccess(ResolvedPluginManifest manifest, int registeredModuleCount)
    {
        return new(true, manifest.PluginId, registeredModuleCount, PluginLoadFailureReason.None,
            $"Loaded plugin '{manifest.DisplayNameOrFallback}' with {registeredModuleCount} module(s).", manifest);
    }

    public static PluginLoadResult FromFailure(PluginLoadFailureReason reason, string message, string? pluginId = null, ResolvedPluginManifest? manifest = null)
    {
        return new(false, pluginId ?? manifest?.PluginId, 0, reason, message, manifest);
    }
}

public sealed record PluginUnloadResult(
    bool Success,
    string? PluginId,
    PluginUnloadFailureReason FailureReason,
    string Message)
{
    public static PluginUnloadResult FromSuccess(string pluginId)
    {
        return new(true, pluginId, PluginUnloadFailureReason.None, $"Unloaded plugin '{pluginId}'.");
    }

    public static PluginUnloadResult FromFailure(PluginUnloadFailureReason reason, string message, string? pluginId = null)
    {
        return new(false, pluginId, reason, message);
    }
}

public sealed record PluginReloadResult(
    bool Success,
    string? PluginId,
    string Message,
    PluginUnloadResult? UnloadResult,
    PluginLoadResult? LoadResult)
{
    public static PluginReloadResult FromSuccess(string pluginId, PluginUnloadResult unloadResult, PluginLoadResult loadResult)
    {
        return new(true, pluginId, $"Reloaded plugin '{pluginId}'.", unloadResult, loadResult);
    }

    public static PluginReloadResult FromFailure(string? pluginId, string message, PluginUnloadResult? unloadResult = null, PluginLoadResult? loadResult = null)
    {
        return new(false, pluginId, message, unloadResult, loadResult);
    }
}

public sealed record PluginRuntimeSnapshot(
    string PluginId,
    string DisplayName,
    PluginState State,
    int RegisteredModuleCount,
    PluginUnloadFailureReason LastUnloadFailureReason,
    string? LastUnloadFailureMessage);
