namespace SFSharp.Abstractions.Modules;

/// <summary>
/// Public read-only snapshot of a module registration plus its safe runtime state.
/// Intended for cross-module discovery through <see cref="ISFModules"/>.
/// </summary>
public sealed record SFModuleInfo(
    string Id,
    string DisplayName,
    string Category,
    string Description,
    bool DefaultEnabled,
    bool AutoStartEnabled,
    ModuleExecutionModel ExecutionModel,
    ModuleRestartPolicy RestartPolicy,
    int Order,
    IReadOnlyList<string> Dependencies,
    string? PluginId,
    ModuleLifecycleState State,
    ModuleStopReason LastStopReason,
    long RestartCount,
    long FaultCount)
{
    /// <summary><see langword="true"/> when the module was registered from an external plugin package.</summary>
    public bool IsPluginModule => !string.IsNullOrWhiteSpace(PluginId);
}
