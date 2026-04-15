namespace SFSharp;

/// <summary>
/// Declares a class as an SF module and carries the metadata that <see cref="SFModuleContainer"/>
/// reads at registration time. The attribute is mandatory for any type registered through
/// <see cref="SFModuleContainer.RegisterModule{T}(bool?)"/>.
/// </summary>
/// <param name="id">
/// Stable module identifier. Used as the key for <c>/sfs</c> commands, telemetry and storage roots
/// (<see cref="SFPaths.GetModuleAssetsDirectory(string)"/>, <see cref="SFPaths.GetModuleUserDataDirectory(string)"/>).
/// Must be unique per container.
/// </param>
/// <param name="displayName">Human readable name shown in chat and in the dashboard dialog.</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SFModuleAttribute(string id, string displayName) : Attribute
{
    /// <summary>Stable module identifier. See the constructor parameter for details.</summary>
    public string Id { get; } = id;

    /// <summary>Human readable module name. See the constructor parameter for details.</summary>
    public string DisplayName { get; } = displayName;

    /// <summary>Free-form grouping label for future UI. Defaults to <c>"General"</c>.</summary>
    public string Category { get; init; } = "General";

    /// <summary>
    /// One line description rendered in the module detail dialog. Keep it short, the dashboard
    /// does not wrap long text well.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// If <see langword="true"/>, <see cref="SFModuleContainer.Run(System.Threading.CancellationToken)"/>
    /// starts the module automatically. The value passed to
    /// <see cref="SFModuleContainer.RegisterModule{T}(bool?)"/> overrides this default.
    /// </summary>
    public bool DefaultEnabled { get; init; } = true;

    /// <summary>Threading model used by the container. See <see cref="ModuleExecutionModel"/>.</summary>
    public ModuleExecutionModel ExecutionModel { get; init; } = ModuleExecutionModel.MainThread;

    /// <summary>
    /// Behaviour after an unhandled exception. See <see cref="ModuleRestartPolicy"/> for the
    /// circuit breaker contract.
    /// </summary>
    public ModuleRestartPolicy RestartPolicy { get; init; } = ModuleRestartPolicy.Manual;

    /// <summary>
    /// Start order inside the container. Lower values start first once hard dependency constraints
    /// are satisfied. Ties are broken by <see cref="DisplayName"/>.
    /// </summary>
    public int Order { get; init; }

    /// <summary>
    /// Optional hard dependency list of module ids this module requires to be registered and
    /// running before it can start. The container also blocks user/plugin stop, restart and
    /// unload operations that would remove a provider while dependent modules are still running.
    /// </summary>
    public string[] Dependencies { get; init; } = [];
}
