using System.Diagnostics;

namespace SFSharp;

/// <summary>
/// Per-run facade passed to <see cref="ISFModule.RunAsync(ModuleContext)"/>. Bundles the
/// cancellation token, telemetry helpers, storage accessors and subscription helpers.
/// Created by <see cref="SFModuleContainer"/> and disposed after the module returns or faults.
/// </summary>
/// <remarks>
/// Do not cache a context outside of the run that received it. The container disposes it once
/// the task completes, which also releases every disposable registered through
/// <see cref="RegisterDisposable(IDisposable)"/> and the subscription extensions in
/// <see cref="ModuleContextEventExtensions"/>.
/// </remarks>
public sealed class ModuleContext : IModuleContext
{
    private readonly ModuleRuntimeInfo _runtime;
    private int _disposed;
    private IModuleStorage? _assets;
    private IModuleStorage? _userData;
    private IModuleConfig? _config;

    /// <summary>
    /// Constructed by the container only. External code obtains contexts through
    /// <see cref="ISFModule.RunAsync(ModuleContext)"/>.
    /// </summary>
    internal ModuleContext(ModuleDescriptor descriptor, ModuleRuntimeInfo runtime, CancellationToken cancellationToken)
    {
        Descriptor = descriptor;
        _runtime = runtime;
        CancellationToken = cancellationToken;
    }

    /// <summary>Static metadata for this module run, see <see cref="ModuleDescriptor"/>.</summary>
    public ModuleDescriptor Descriptor { get; }

    /// <summary>
    /// Cancelled when the container shuts down or the user issues <c>/sfs stop</c>. Also the token
    /// forwarded to <see cref="SFModuleBase.ExecuteAsync(CancellationToken)"/>.
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Read/write access to the module's asset folder next to <c>gta_sa.exe</c>. Rooted at
    /// <see cref="SFPaths.GetModuleAssetsDirectory(string)"/>. Resolved lazily through
    /// <see cref="SF.Modules"/>, which allows another module to override the backing store before
    /// the first access.
    /// </summary>
    public IModuleStorage Assets => _assets ??= SF.Modules.Storage.GetAssets(Descriptor);

    /// <summary>
    /// Read/write access to the module's user data folder under <c>My Documents</c>. Rooted at
    /// <see cref="SFPaths.GetModuleUserDataDirectory(string)"/>.
    /// </summary>
    public IModuleStorage UserData => _userData ??= SF.Modules.Storage.GetUserData(Descriptor);

    /// <summary>
    /// Typed configuration backed by a JSON file inside <see cref="UserData"/>. Source-generated
    /// <see cref="System.Text.Json.Serialization.Metadata.JsonTypeInfo{T}"/> is required for AOT
    /// compatibility, see <see cref="IModuleConfig"/>.
    /// </summary>
    public IModuleConfig Config => _config ??= SF.Modules.Storage.GetConfig(Descriptor);

    /// <summary>
    /// Records a heartbeat tick. Intended to be called every loop iteration or whenever the module
    /// wants to signal "still alive". Updates <see cref="ModuleRuntimeSnapshot.LastHeartbeatAt"/>
    /// and optionally <see cref="ModuleRuntimeSnapshot.LastActivity"/>.
    /// </summary>
    /// <param name="activity">Short human readable label for the current step, or <see langword="null"/>.</param>
    public void Heartbeat(string? activity = null)
    {
        _runtime.RecordHeartbeat(activity);
    }

    /// <summary>Updates only the activity label without touching the heartbeat timestamp.</summary>
    /// <param name="activity">Short label, required.</param>
    public void ReportActivity(string activity)
    {
        _runtime.RecordActivity(activity);
    }

    /// <summary>Sets the free-form status string surfaced in <see cref="ModuleRuntimeSnapshot.StatusText"/>.</summary>
    /// <param name="value">Status text, or <see langword="null"/> to clear.</param>
    public void SetStatusText(string? value)
    {
        _runtime.SetStatusText(value);
    }

    /// <summary>
    /// Adds <paramref name="delta"/> to a named counter shown in the dashboard under
    /// <see cref="ModuleRuntimeSnapshot.Counters"/>.
    /// </summary>
    /// <param name="counterName">Case-insensitive counter key. Use dotted names for grouping, for example <c>"dialogs.opened"</c>.</param>
    /// <param name="delta">Signed delta, defaults to <c>1</c>.</param>
    public void IncrementCounter(string counterName, long delta = 1)
    {
        _runtime.IncrementCounter(counterName, delta);
    }

    /// <summary>
    /// Sets a detail key shown in the dashboard under <see cref="ModuleRuntimeSnapshot.Details"/>.
    /// Passing a blank <paramref name="value"/> removes the key.
    /// </summary>
    /// <param name="key">Case-insensitive key.</param>
    /// <param name="value">Value, or <see langword="null"/>/whitespace to remove the entry.</param>
    public void SetDetail(string key, string? value)
    {
        _runtime.SetDetail(key, value);
    }

    /// <summary>
    /// Starts a loop timer. Dispose the returned <see cref="ModuleLoopScope"/> at the end of the
    /// iteration to record loop duration, duty cycle and to bump
    /// <see cref="ModuleRuntimeSnapshot.LoopCount"/>.
    /// </summary>
    /// <param name="activity">Optional label for the loop iteration.</param>
    public ModuleLoopScope TrackLoop(string? activity = null)
    {
        return new ModuleLoopScope(_runtime, Stopwatch.GetTimestamp(), activity);
    }

    IDisposable IModuleContext.TrackLoop(string? activity) => TrackLoop(activity);

    /// <summary>
    /// Takes ownership of <paramref name="disposable"/>. The object is disposed when the module
    /// run ends, even if the module faults. Returns the same instance so it can be used inline.
    /// </summary>
    /// <param name="disposable">Resource to be disposed at module teardown.</param>
    /// <exception cref="ArgumentNullException"><paramref name="disposable"/> is <see langword="null"/>.</exception>
    public IDisposable RegisterDisposable(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        _runtime.RegisterDisposable(disposable);
        return disposable;
    }

    /// <summary>
    /// Registers a chat command scoped to this module's lifetime. The callback is wrapped so that
    /// <see cref="ModuleRuntimeSnapshot.Counters"/> tracks invocations under
    /// <c>"commands.invoked"</c> and <c>"commands.registered"</c>.
    /// </summary>
    /// <param name="command">Command token without the leading slash.</param>
    /// <param name="callback">
    /// Handler invoked on the main thread. Argument is the tail of the line after the command,
    /// or <see langword="null"/> when the user typed the bare command.
    /// </param>
    /// <returns>Disposable registration, also tracked by <see cref="RegisterDisposable(IDisposable)"/>.</returns>
    public IDisposable RegisterChatCommand(string command, Action<string?> callback)
    {
        IDisposable registration = SF.Chat.RegisterChatCommand(command, args =>
        {
            _runtime.IncrementCounter("commands.invoked", 1);
            _runtime.RecordActivity($"command:{command}");
            callback(args);
        });
        _runtime.RegisterDisposable(registration);
        _runtime.IncrementCounter("commands.registered", 1);
        return registration;
    }

    /// <summary>
    /// Awaitable that resumes on the SAMP main thread. No-op when already there. Safe to call
    /// from any thread. Intended for modules using <see cref="ModuleExecutionModel.BackgroundWorker"/>
    /// or <see cref="ModuleExecutionModel.Hybrid"/>.
    /// </summary>
    public Task SwitchToMainThreadAsync()
    {
        if (SynchronizationContext.Current is SFSynchronizationContext)
        {
            _runtime.RecordActivity("main-thread");
            return Task.CompletedTask;
        }

        TaskCompletionSource tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        SFBootstrap.PostToMainThread(() =>
        {
            _runtime.RecordActivity("switch-main-thread");
            tcs.SetResult();
        });
        return tcs.Task;
    }

    /// <summary>
    /// Runs <paramref name="work"/> on a thread-pool thread. The call honours
    /// <see cref="CancellationToken"/>, so work that respects cancellation is aborted on module stop.
    /// </summary>
    /// <param name="work">Async delegate executed off the main thread.</param>
    /// <exception cref="ArgumentNullException"><paramref name="work"/> is <see langword="null"/>.</exception>
    public Task RunBackground(Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);
        return Task.Run(async () =>
        {
            _runtime.RecordActivity("background-work");
            await work();
        }, CancellationToken);
    }

    /// <summary>Captures a current <see cref="ModuleRuntimeSnapshot"/>. Safe to call from any thread.</summary>
    public ModuleRuntimeSnapshot GetSnapshot()
    {
        return _runtime.CreateSnapshot();
    }

    /// <summary>
    /// Releases every disposable registered through <see cref="RegisterDisposable(IDisposable)"/>
    /// and through the subscription helpers in <see cref="ModuleContextEventExtensions"/>.
    /// Idempotent. The container calls it exactly once per run.
    /// </summary>
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _runtime.DisposeOwnedDisposables();
    }
}
