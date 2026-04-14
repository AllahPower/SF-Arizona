namespace SFSharp;

/// <summary>
/// Minimal contract implemented by every SF module. User modules typically derive from
/// <c>SFModuleBase</c> instead of implementing this directly, as the base class handles lifecycle
/// hooks, logger wiring and cancellation semantics.
/// </summary>
/// <remarks>
/// Registered types are resolved through a parameterless constructor. Keep state inside the instance,
/// the host creates a fresh instance for every restart.
/// </remarks>
public interface ISFModule
{
    /// <summary>
    /// Entry point invoked by the host after the module is constructed. The returned task represents
    /// the whole lifetime of the module, it it completes either when the module finishes naturally or
    /// when <paramref name="context"/>'s cancellation token fires.
    /// </summary>
    /// <param name="context">
    /// Per-run context with telemetry, storage and registration helpers. Valid only until the returned
    /// task completes, the host disposes it afterwards.
    /// </param>
    Task RunAsync(IModuleContext context);
}
