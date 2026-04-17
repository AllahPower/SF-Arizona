namespace SFSharp.Abstractions.Modules;

/// <summary>
/// Telemetry surface for a single module run. Separated from <see cref="IModuleContext"/> so the
/// execution surface (storage, cancellation, logging, subscriptions) stays decoupled from
/// bookkeeping, and so future middleware can wrap telemetry around handler registration instead
/// of requiring modules to sprinkle calls manually.
/// </summary>
/// <remarks>
/// Every member is thread-safe - the backing runtime info uses a single internal lock plus
/// lock-free <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/> for
/// counters and details.
/// </remarks>
public interface IModuleTelemetry
{
    /// <summary>Records a heartbeat tick and optionally updates the activity label.</summary>
    void Heartbeat(string? activity = null);

    /// <summary>Updates only the activity label without touching the heartbeat timestamp.</summary>
    void ReportActivity(string activity);

    /// <summary>Sets the free-form status string surfaced in the runtime snapshot.</summary>
    void SetStatusText(string? value);

    /// <summary>Adds <paramref name="delta"/> to a named counter in the runtime snapshot.</summary>
    void IncrementCounter(string counterName, long delta = 1);

    /// <summary>Sets a detail key in the runtime snapshot. Blank value removes the key.</summary>
    void SetDetail(string key, string? value);

    /// <summary>
    /// Starts a loop timer. Dispose the returned handle at the end of the iteration to record
    /// loop duration, duty cycle and bump the loop counter.
    /// </summary>
    IDisposable TrackLoop(string? activity = null);

    /// <summary>Captures the current runtime snapshot. Safe to call from any thread.</summary>
    ModuleRuntimeSnapshot GetSnapshot();
}
