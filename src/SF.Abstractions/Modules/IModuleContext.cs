using Microsoft.Extensions.Logging;

namespace SFSharp.Abstractions.Modules;

/// <summary>
/// Per-run facade passed to <see cref="ISFModule.RunAsync(IModuleContext)"/>. Exposes the cancellation
/// token, telemetry, storage accessors and subscription helpers for a single module run. The concrete
/// implementation is owned by the host and disposed after the module returns or faults.
/// </summary>
/// <remarks>
/// Member thread-safety varies. Telemetry helpers (<see cref="Telemetry"/>) and
/// command registration, <see cref="RegisterDisposable"/> and background work registration
/// are thread-safe. Everything else (storage init, <see cref="Config"/> access, subscription
/// extensions that touch native hooks) should be treated as main-thread-only unless documented
/// otherwise on the specific API.
/// </remarks>
public interface IModuleContext : IDisposable
{
    /// <summary>Static metadata for this module run.</summary>
    ModuleDescriptor Descriptor { get; }

    /// <summary>Root SF facade for this host. Same instance every module sees.</summary>
    ISF SF { get; }

    /// <summary>Cancelled when the container shuts down or the user stops the module.</summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// Logger scoped to <see cref="ModuleDescriptor.Id"/>. Messages go through the host logging
    /// pipeline and end up in the standard SF log sinks. Thread-safe.
    /// </summary>
    ILogger Log { get; }

    /// <summary>Read/write access to the module's asset folder next to the game executable.</summary>
    IModuleStorage Assets { get; }

    /// <summary>Read/write access to the module's user data folder under My Documents.</summary>
    IModuleStorage UserData { get; }

    /// <summary>Typed JSON configuration stored inside <see cref="UserData"/>.</summary>
    IModuleConfig Config { get; }

    /// <summary>Thread-safe telemetry surface for this run.</summary>
    IModuleTelemetry Telemetry { get; }

    /// <inheritdoc cref="IModuleTelemetry.Heartbeat"/>
    void Heartbeat(string? activity = null) => Telemetry.Heartbeat(activity);

    /// <inheritdoc cref="IModuleTelemetry.ReportActivity"/>
    void ReportActivity(string activity) => Telemetry.ReportActivity(activity);

    /// <inheritdoc cref="IModuleTelemetry.SetStatusText"/>
    void SetStatusText(string? value) => Telemetry.SetStatusText(value);

    /// <inheritdoc cref="IModuleTelemetry.IncrementCounter"/>
    void IncrementCounter(string counterName, long delta = 1) => Telemetry.IncrementCounter(counterName, delta);

    /// <inheritdoc cref="IModuleTelemetry.SetDetail"/>
    void SetDetail(string key, string? value) => Telemetry.SetDetail(key, value);

    /// <inheritdoc cref="IModuleTelemetry.TrackLoop"/>
    IDisposable TrackLoop(string? activity = null) => Telemetry.TrackLoop(activity);

    /// <summary>
    /// Takes ownership of <paramref name="disposable"/>. The object is disposed when the module run
    /// ends, even if the module faults. Thread-safe. Resources are released in reverse registration
    /// order on the game thread, after owned work finishes. Late registrations are disposed and rejected;
    /// a late worker registration waits for game-thread cleanup. Do not register resources from native filters.
    /// </summary>
    IDisposable RegisterDisposable(IDisposable disposable);

    /// <summary>Registers a chat command scoped to this module's lifetime. Thread-safe.</summary>
    IDisposable RegisterChatCommand(string command, Action<string?> callback);

    /// <summary>
    /// Registers a tracked asynchronous command. Its synchronous prefix runs on the game thread.
    /// At most one invocation runs at a time; further invocations while busy are ignored.
    /// Stopping the run cancels the supplied token and waits for the handler before releasing resources.
    /// </summary>
    IDisposable RegisterAsyncChatCommand(string command, Func<string?, CancellationToken, Task> callback);

    /// <summary>Executes on the game thread, cancelled when this module stops. The caller's continuation is not switched.</summary>
    Task InvokeOnMainThreadAsync(Action action, CancellationToken cancellationToken = default);
    Task<T> InvokeOnMainThreadAsync<T>(Func<T> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs <paramref name="work"/> on a thread-pool thread and tracks the returned task as an
    /// owned background task for this module run. Thread-safe.
    /// </summary>
    Task RunBackground(Func<Task> work);

    /// <summary>Starts tracked thread-pool work with this run's cancellation token. Late starts are rejected.</summary>
    Task RunBackground(Func<CancellationToken, Task> work);

    /// <inheritdoc cref="IModuleTelemetry.GetSnapshot"/>
    ModuleRuntimeSnapshot GetSnapshot() => Telemetry.GetSnapshot();
}
