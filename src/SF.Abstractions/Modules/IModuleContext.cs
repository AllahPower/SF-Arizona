namespace SFSharp;

/// <summary>
/// Per-run facade passed to <see cref="ISFModule.RunAsync(IModuleContext)"/>. Exposes the cancellation
/// token, telemetry helpers, storage accessors and subscription helpers for a single module run.
/// The concrete implementation is owned by the host and disposed after the module returns or faults.
/// </summary>
public interface IModuleContext : IDisposable
{
    /// <summary>Static metadata for this module run.</summary>
    ModuleDescriptor Descriptor { get; }

    /// <summary>Root SF facade for this host. Same instance every module sees.</summary>
    ISF SF { get; }

    /// <summary>Cancelled when the container shuts down or the user stops the module.</summary>
    CancellationToken CancellationToken { get; }

    /// <summary>Read/write access to the module's asset folder next to the game executable.</summary>
    IModuleStorage Assets { get; }

    /// <summary>Read/write access to the module's user data folder under My Documents.</summary>
    IModuleStorage UserData { get; }

    /// <summary>Typed JSON configuration stored inside <see cref="UserData"/>.</summary>
    IModuleConfig Config { get; }

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
    /// Starts a loop timer. Dispose the returned handle at the end of the iteration to record loop
    /// duration, duty cycle and bump the loop counter.
    /// </summary>
    IDisposable TrackLoop(string? activity = null);

    /// <summary>
    /// Takes ownership of <paramref name="disposable"/>. The object is disposed when the module run
    /// ends, even if the module faults.
    /// </summary>
    IDisposable RegisterDisposable(IDisposable disposable);

    /// <summary>Registers a chat command scoped to this module's lifetime.</summary>
    IDisposable RegisterChatCommand(string command, Action<string?> callback);

    /// <summary>Awaitable that resumes on the SAMP main thread. No-op when already there.</summary>
    Task SwitchToMainThreadAsync();

    /// <summary>Runs <paramref name="work"/> on a thread-pool thread.</summary>
    Task RunBackground(Func<Task> work);

    /// <summary>Captures a current runtime snapshot. Safe to call from any thread.</summary>
    ModuleRuntimeSnapshot GetSnapshot();
}
