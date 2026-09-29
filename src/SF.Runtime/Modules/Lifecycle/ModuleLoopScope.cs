using System.Diagnostics;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Scoped timer returned by <see cref="ModuleContext.TrackLoop(string?)"/>. Starts timing when
/// constructed and records the sample through the module's telemetry on <see cref="Dispose"/>.
/// Intended to be consumed with <c>using</c> so the end timestamp is captured exactly once:
/// <code>
/// using (Context.TrackLoop("keyboard-poll"))
/// {
///     // work
/// }
/// </code>
/// Feeds <see cref="ModuleRuntimeSnapshot.AverageLoopMilliseconds"/>,
/// <see cref="ModuleRuntimeSnapshot.MaxLoopMilliseconds"/> and
/// <see cref="ModuleRuntimeSnapshot.EstimatedLoadPercent"/>.
/// </summary>
public readonly struct ModuleLoopScope : IDisposable
{
    private readonly ModuleRuntimeInfo _runtime;
    private readonly long _startTicks;
    private readonly string? _activity;

    internal ModuleLoopScope(ModuleRuntimeInfo runtime, long startTicks, string? activity)
    {
        _runtime = runtime;
        _startTicks = startTicks;
        _activity = activity;
    }

    /// <summary>
    /// Records the elapsed time and the optional activity label. Safe to call on a default struct,
    /// in which case the sample is dropped.
    /// </summary>
    public void Dispose()
    {
        if (_runtime is null)
        {
            return;
        }

        _runtime.RecordLoop(_startTicks, Stopwatch.GetTimestamp(), _activity);
    }
}
