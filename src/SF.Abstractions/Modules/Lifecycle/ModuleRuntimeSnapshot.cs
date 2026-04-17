namespace SFSharp.Abstractions.Modules.Lifecycle;

/// <summary>
/// Point-in-time copy of a module's runtime state and telemetry. Produced by
/// <see cref="ModuleContext.GetSnapshot"/> and by the container when rendering the <c>/sfs</c>
/// dashboard. Immutable, safe to pass across threads and to stash in logs.
/// </summary>
/// <param name="Descriptor">Static metadata for the module, see <see cref="ModuleDescriptor"/>.</param>
/// <param name="State">Current lifecycle state, see <see cref="ModuleLifecycleState"/>.</param>
/// <param name="LastStopReason">Reason recorded by the last stop, see <see cref="ModuleStopReason"/>.</param>
/// <param name="AutoStartEnabled">Whether the next container run will auto-start the module.</param>
/// <param name="CreatedAt">When the runtime info object was first constructed.</param>
/// <param name="StartedAt">When the current lifecycle entered <see cref="ModuleLifecycleState.Starting"/>.</param>
/// <param name="StoppedAt">When the module left <see cref="ModuleLifecycleState.Running"/>, or <see langword="null"/> while running.</param>
/// <param name="LastHeartbeatAt">Timestamp of the last <see cref="ModuleContext.Heartbeat(string?)"/> call.</param>
/// <param name="LastActivityAt">Timestamp of the last recorded activity, heartbeat, loop or state change.</param>
/// <param name="LastActivity">Short label attached to the last activity update.</param>
/// <param name="StatusText">Free-form status set via <see cref="ModuleContext.SetStatusText(string?)"/>.</param>
/// <param name="StartThreadId">Managed thread id captured the first time the run loop was observed.</param>
/// <param name="LastThreadId">Managed thread id of the most recent activity.</param>
/// <param name="StartCount">Total number of starts across this runtime info object's lifetime.</param>
/// <param name="RestartCount">Number of automatic or user-triggered restarts.</param>
/// <param name="FaultCount">Number of faulted runs.</param>
/// <param name="LoopCount">Number of <see cref="ModuleContext.TrackLoop(string?)"/> scopes recorded.</param>
/// <param name="AverageLoopMilliseconds">Running average of loop durations.</param>
/// <param name="MaxLoopMilliseconds">Worst loop duration observed this run.</param>
/// <param name="EstimatedLoadPercent">Duty cycle estimate in <c>[0, 100]</c> based on loop work over loop period.</param>
/// <param name="LastMemoryBytes">Last GC memory poll in bytes, refreshed at most once every two seconds.</param>
/// <param name="LastExceptionType">Simple type name of the last unhandled exception.</param>
/// <param name="LastExceptionMessage">Message of the last unhandled exception.</param>
/// <param name="Counters">Numeric counters from <see cref="ModuleContext.IncrementCounter(string, long)"/>.</param>
/// <param name="Details">Key value strings from <see cref="ModuleContext.SetDetail(string, string?)"/>.</param>
/// <param name="OwnedDisposableCount">Number of disposables still held by the current context.</param>
public sealed record ModuleRuntimeSnapshot(
    ModuleDescriptor Descriptor,
    ModuleLifecycleState State,
    ModuleStopReason LastStopReason,
    bool AutoStartEnabled,
    DateTimeOffset? CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? StoppedAt,
    DateTimeOffset? LastHeartbeatAt,
    DateTimeOffset? LastActivityAt,
    string? LastActivity,
    string? StatusText,
    int? StartThreadId,
    int? LastThreadId,
    long StartCount,
    long RestartCount,
    long FaultCount,
    long LoopCount,
    double AverageLoopMilliseconds,
    double MaxLoopMilliseconds,
    double EstimatedLoadPercent,
    long LastMemoryBytes,
    string? LastExceptionType,
    string? LastExceptionMessage,
    IReadOnlyDictionary<string, long> Counters,
    IReadOnlyDictionary<string, string> Details,
    int OwnedDisposableCount)
{
    /// <summary>
    /// Time since <see cref="StartedAt"/>, or <see langword="null"/> if the module never started.
    /// Uses <see cref="StoppedAt"/> as the end when the module is no longer running.
    /// </summary>
    public TimeSpan? Uptime => StartedAt is null ? null : (StoppedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value;
}
