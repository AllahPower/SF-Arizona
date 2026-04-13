namespace SFSharp;

/// <summary>
/// Minimal contract implemented by every SF module. Most user modules should derive from
/// <see cref="SFModuleBase"/> instead of implementing this directly, as the base class handles
/// lifecycle hooks, logger wiring and cancellation semantics for you.
/// </summary>
/// <remarks>
/// Registered types are resolved through a parameterless <see langword="new"/> constructor, see
/// <see cref="SFModuleContainer.RegisterModule{T}(bool?)"/>. Keep state inside the instance,
/// the container creates a fresh instance for every restart.
/// </remarks>
public interface ISFModule
{
    /// <summary>
    /// Entry point invoked by <see cref="SFModuleContainer"/> after the module is constructed.
    /// The returned task represents the whole lifetime of the module, it is expected to complete
    /// either when the module finishes naturally or when <paramref name="context"/>'s
    /// <see cref="ModuleContext.CancellationToken"/> fires.
    /// </summary>
    /// <param name="context">
    /// Per-run context with telemetry, storage and registration helpers. Valid only until the returned
    /// task completes, the container disposes it afterwards.
    /// </param>
    Task RunAsync(ModuleContext context);
}
