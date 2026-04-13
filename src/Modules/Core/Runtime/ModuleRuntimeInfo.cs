using System.Collections.Concurrent;
using System.Diagnostics;

namespace SFSharp;

/// <summary>
/// Mutable per-module state owned by <see cref="SFModuleContainer"/>. Holds lifecycle flags,
/// telemetry counters and the disposables registered through <see cref="ModuleContext"/>.
/// Public consumers should read state through <see cref="ModuleRuntimeSnapshot"/> (via
/// <see cref="CreateSnapshot"/>) instead of touching this type directly.
/// </summary>
/// <remarks>
/// All lifecycle methods are thread safe. Counters and details use a lock-free
/// <see cref="ConcurrentDictionary{TKey, TValue}"/>, the rest is guarded by an internal
/// <see cref="Lock"/>. The circuit breaker fires when <c>5</c> faults land inside a rolling
/// <c>60</c> second window.
/// </remarks>
public sealed class ModuleRuntimeInfo
{
    private readonly Lock _sync = new();
    private readonly List<IDisposable> _ownedDisposables = [];
    private readonly ConcurrentDictionary<string, long> _counters = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, string> _details = new(StringComparer.OrdinalIgnoreCase);
    private long _lastMemoryPollTicks;

    private DateTimeOffset? _createdAt = DateTimeOffset.UtcNow;
    private DateTimeOffset? _startedAt;
    private DateTimeOffset? _stoppedAt;
    private DateTimeOffset? _lastHeartbeatAt;
    private DateTimeOffset? _lastActivityAt;
    private string? _lastActivity;
    private string? _statusText;
    private ModuleLifecycleState _state = ModuleLifecycleState.Created;
    private ModuleStopReason _lastStopReason = ModuleStopReason.None;
    private bool _autoStartEnabled;
    private int? _startThreadId;
    private int? _lastThreadId;
    private long _startCount;
    private long _restartCount;
    private long _faultCount;
    private long _loopCount;
    private double _avgLoopMilliseconds;
    private double _maxLoopMilliseconds;
    private double _totalMeasuredLoopMilliseconds;
    private long _lastLoopStartTicks;
    private double _dutyCycleSum;
    private long _dutyCycleCount;
    private long _lastMemoryBytes;
    private string? _lastExceptionType;
    private string? _lastExceptionMessage;
    private ModuleStopReason _requestedStopReason = ModuleStopReason.None;
    private readonly Queue<long> _recentFaultTicks = new();
    private bool _circuitBroken;

    /// <summary>
    /// Called by the container during <see cref="SFModuleContainer.RegisterModule{T}(bool?)"/>.
    /// </summary>
    /// <param name="descriptor">Static metadata for the owning module.</param>
    /// <param name="autoStartEnabled">Initial value for <see cref="ModuleRuntimeSnapshot.AutoStartEnabled"/>.</param>
    internal ModuleRuntimeInfo(ModuleDescriptor descriptor, bool autoStartEnabled)
    {
        Descriptor = descriptor;
        _autoStartEnabled = autoStartEnabled;
    }

    /// <summary>Static metadata for the module this runtime belongs to.</summary>
    public ModuleDescriptor Descriptor { get; }

    private static readonly long MemoryPollIntervalTicks = Stopwatch.Frequency * 2;
    private const int CircuitBreakerFaultLimit = 5;
    private static readonly long CircuitBreakerWindowTicks = Stopwatch.Frequency * 60;

    private void PollMemoryIfDue()
    {
        long now = Stopwatch.GetTimestamp();
        if (now - _lastMemoryPollTicks >= MemoryPollIntervalTicks)
        {
            _lastMemoryPollTicks = now;
            _lastMemoryBytes = GC.GetTotalMemory(false);
        }
    }

    /// <summary>
    /// Pushes a fault timestamp into the rolling window used by the circuit breaker.
    /// Trips <see cref="IsCircuitBroken"/> when enough faults land inside the window.
    /// </summary>
    internal void RecordFaultForCircuitBreaker()
    {
        lock (_sync)
        {
            long now = Stopwatch.GetTimestamp();
            _recentFaultTicks.Enqueue(now);

            while (_recentFaultTicks.Count > 0 && now - _recentFaultTicks.Peek() > CircuitBreakerWindowTicks)
            {
                _recentFaultTicks.Dequeue();
            }

            if (_recentFaultTicks.Count >= CircuitBreakerFaultLimit)
            {
                _circuitBroken = true;
            }
        }
    }

    /// <summary>
    /// <see langword="true"/> after the container has observed too many faults in the recent
    /// window and decided to stop auto-restarting the module.
    /// </summary>
    public bool IsCircuitBroken
    {
        get
        {
            lock (_sync)
            {
                return _circuitBroken;
            }
        }
    }

    /// <summary>
    /// Resets per-run counters and timestamps. Called by the container right before it invokes
    /// the module factory. Not part of the public contract.
    /// </summary>
    /// <param name="autoStartEnabled">Current auto-start flag, mirrored into the snapshot.</param>
    internal void PrepareForStart(bool autoStartEnabled)
    {
        lock (_sync)
        {
            _autoStartEnabled = autoStartEnabled;
            _state = ModuleLifecycleState.Starting;
            _createdAt ??= DateTimeOffset.UtcNow;
            _startedAt = DateTimeOffset.UtcNow;
            _stoppedAt = null;
            _lastHeartbeatAt = null;
            _lastActivityAt = _startedAt;
            _lastActivity = "starting";
            _statusText = null;
            _startThreadId = null;
            _lastThreadId = null;
            _startCount++;
            _requestedStopReason = ModuleStopReason.None;
            _lastStopReason = ModuleStopReason.None;
            _lastExceptionType = null;
            _lastExceptionMessage = null;
            _loopCount = 0;
            _avgLoopMilliseconds = 0;
            _maxLoopMilliseconds = 0;
            _totalMeasuredLoopMilliseconds = 0;
            _lastLoopStartTicks = 0;
            _dutyCycleSum = 0;
            _dutyCycleCount = 0;
            _lastMemoryPollTicks = 0;
            _lastMemoryBytes = GC.GetTotalMemory(false);
            _counters.Clear();
            _details.Clear();
            _ownedDisposables.Clear();
        }
    }

    /// <summary>Transitions the state to <see cref="ModuleLifecycleState.Running"/> and captures the start thread id.</summary>
    internal void MarkRunning()
    {
        lock (_sync)
        {
            _state = ModuleLifecycleState.Running;
            int threadId = Environment.CurrentManagedThreadId;
            _startThreadId = threadId;
            _lastThreadId = threadId;
            _lastActivityAt = DateTimeOffset.UtcNow;
            _lastActivity = "running";
        }
    }

    /// <summary>Records a stop request without cancelling the running task, only flips the state and stop reason.</summary>
    /// <param name="reason">Why the stop was requested.</param>
    internal void RequestStop(ModuleStopReason reason)
    {
        lock (_sync)
        {
            _requestedStopReason = reason;
            if (_state is ModuleLifecycleState.Starting or ModuleLifecycleState.Running)
            {
                _state = ModuleLifecycleState.Stopping;
            }

            _lastActivityAt = DateTimeOffset.UtcNow;
            _lastActivity = $"stop:{reason}";
        }
    }

    /// <summary>Transitions to <see cref="ModuleLifecycleState.Stopped"/>. Resolves the final stop reason.</summary>
    internal void MarkCompleted()
    {
        lock (_sync)
        {
            _state = ModuleLifecycleState.Stopped;
            _stoppedAt = DateTimeOffset.UtcNow;
            _lastStopReason = _requestedStopReason == ModuleStopReason.None ? ModuleStopReason.Completed : _requestedStopReason;
            _lastActivityAt = _stoppedAt;
            _lastActivity = "completed";
        }
    }

    /// <summary>Transitions to <see cref="ModuleLifecycleState.Faulted"/> and captures exception metadata.</summary>
    /// <param name="ex">The unhandled exception observed by the container.</param>
    internal void MarkFault(Exception ex)
    {
        lock (_sync)
        {
            _state = ModuleLifecycleState.Faulted;
            _stoppedAt = DateTimeOffset.UtcNow;
            _lastStopReason = ModuleStopReason.Faulted;
            _faultCount++;
            _lastExceptionType = ex.GetType().Name;
            _lastExceptionMessage = ex.Message;
            _lastActivityAt = _stoppedAt;
            _lastActivity = $"fault:{_lastExceptionType}";
        }
    }

    /// <summary>Atomically increments <see cref="ModuleRuntimeSnapshot.RestartCount"/>.</summary>
    internal void IncrementRestartCount()
    {
        Interlocked.Increment(ref _restartCount);
    }

    /// <summary>
    /// Public telemetry hook called by <see cref="ModuleContext.Heartbeat(string?)"/>. Updates
    /// <see cref="ModuleRuntimeSnapshot.LastHeartbeatAt"/> and polls memory if due.
    /// </summary>
    /// <param name="activity">Optional free-form label that becomes <see cref="ModuleRuntimeSnapshot.LastActivity"/>.</param>
    internal void RecordHeartbeat(string? activity)
    {
        lock (_sync)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            _lastHeartbeatAt = now;
            _lastActivityAt = now;
            if (!string.IsNullOrWhiteSpace(activity))
            {
                _lastActivity = activity;
            }

            _lastThreadId = Environment.CurrentManagedThreadId;
            PollMemoryIfDue();
        }
    }

    /// <summary>Updates <see cref="ModuleRuntimeSnapshot.LastActivity"/> without touching the heartbeat timestamp.</summary>
    /// <param name="activity">Short label describing the activity.</param>
    internal void RecordActivity(string activity)
    {
        lock (_sync)
        {
            _lastActivityAt = DateTimeOffset.UtcNow;
            _lastActivity = activity;
            _lastThreadId = Environment.CurrentManagedThreadId;
        }
    }

    /// <summary>
    /// Records a loop sample produced by a <see cref="ModuleLoopScope"/>. Maintains the running
    /// average, the max, and the duty cycle estimate.
    /// </summary>
    /// <param name="startTicks"><see cref="Stopwatch.GetTimestamp"/> at scope creation.</param>
    /// <param name="endTicks"><see cref="Stopwatch.GetTimestamp"/> at scope dispose.</param>
    /// <param name="activity">Optional label, overwrites <see cref="ModuleRuntimeSnapshot.LastActivity"/>.</param>
    internal void RecordLoop(long startTicks, long endTicks, string? activity)
    {
        lock (_sync)
        {
            _loopCount++;
            double ms = Stopwatch.GetElapsedTime(startTicks, endTicks).TotalMilliseconds;
            _avgLoopMilliseconds = ((_avgLoopMilliseconds * (_loopCount - 1)) + ms) / _loopCount;
            _maxLoopMilliseconds = Math.Max(_maxLoopMilliseconds, ms);
            _totalMeasuredLoopMilliseconds += ms;

            if (_lastLoopStartTicks > 0)
            {
                double periodMs = Stopwatch.GetElapsedTime(_lastLoopStartTicks, startTicks).TotalMilliseconds;
                if (periodMs > 0)
                {
                    _dutyCycleSum += Math.Min(ms / periodMs, 1.0);
                    _dutyCycleCount++;
                }
            }

            _lastLoopStartTicks = startTicks;
            _lastThreadId = Environment.CurrentManagedThreadId;
            PollMemoryIfDue();
            DateTimeOffset now = DateTimeOffset.UtcNow;
            _lastHeartbeatAt = now;
            _lastActivityAt = now;
            if (!string.IsNullOrWhiteSpace(activity))
            {
                _lastActivity = activity;
            }
        }
    }

    /// <summary>Adds <paramref name="delta"/> to the named counter.</summary>
    /// <param name="counterName">Case-insensitive counter key.</param>
    /// <param name="delta">Signed delta. Negative values are allowed.</param>
    internal void IncrementCounter(string counterName, long delta)
    {
        _counters.AddOrUpdate(counterName, delta, (_, current) => current + delta);
    }

    /// <summary>Sets a key value pair shown in the dashboard. Passing a blank <paramref name="value"/> removes the entry.</summary>
    /// <param name="key">Case-insensitive detail key. Blank keys are ignored.</param>
    /// <param name="value">Detail value, or <see langword="null"/>/whitespace to remove the key.</param>
    internal void SetDetail(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            _details.TryRemove(key, out _);
            return;
        }

        _details[key] = value;
    }

    /// <summary>Updates <see cref="ModuleRuntimeSnapshot.StatusText"/>.</summary>
    /// <param name="value">Arbitrary status string, or <see langword="null"/> to clear.</param>
    internal void SetStatusText(string? value)
    {
        lock (_sync)
        {
            _statusText = value;
        }
    }

    /// <summary>Toggles the auto-start flag surfaced through <see cref="ModuleRuntimeSnapshot.AutoStartEnabled"/>.</summary>
    /// <param name="value">New flag value.</param>
    internal void SetAutoStartEnabled(bool value)
    {
        lock (_sync)
        {
            _autoStartEnabled = value;
        }
    }

    /// <summary>
    /// Adds a disposable to the context-owned list so it is released on module teardown.
    /// Called by <see cref="ModuleContext.RegisterDisposable(IDisposable)"/>.
    /// </summary>
    /// <param name="disposable">Resource that lives as long as the current module run.</param>
    internal void RegisterDisposable(IDisposable disposable)
    {
        lock (_sync)
        {
            _ownedDisposables.Add(disposable);
        }
    }

    /// <summary>
    /// Disposes every owned <see cref="IDisposable"/> and clears the list. Exceptions from
    /// individual disposables are logged and swallowed so teardown always finishes.
    /// </summary>
    internal void DisposeOwnedDisposables()
    {
        List<IDisposable> owned;
        lock (_sync)
        {
            owned = [.. _ownedDisposables];
            _ownedDisposables.Clear();
        }

        foreach (IDisposable disposable in owned)
        {
            try
            {
                disposable.Dispose();
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"[{Descriptor.Id}] Failed to dispose runtime resource");
            }
        }
    }

    /// <summary>Clears every counter, detail and loop statistic. Exposed through the dashboard's <c>[Telemetry]</c> action.</summary>
    internal void ClearTelemetry()
    {
        lock (_sync)
        {
            _counters.Clear();
            _details.Clear();
            _loopCount = 0;
            _avgLoopMilliseconds = 0;
            _maxLoopMilliseconds = 0;
            _totalMeasuredLoopMilliseconds = 0;
            _lastLoopStartTicks = 0;
            _dutyCycleSum = 0;
            _dutyCycleCount = 0;
            _lastExceptionType = null;
            _lastExceptionMessage = null;
            _lastMemoryPollTicks = 0;
            _lastMemoryBytes = GC.GetTotalMemory(false);
        }
    }

    /// <summary>
    /// Produces an immutable snapshot of the current state. The dictionaries are copied so the
    /// caller is free to keep the returned object across thread boundaries.
    /// </summary>
    public ModuleRuntimeSnapshot CreateSnapshot()
    {
        lock (_sync)
        {
            double estimatedLoadPercent = 0;
            if (_dutyCycleCount > 0)
            {
                estimatedLoadPercent = Math.Clamp((_dutyCycleSum / _dutyCycleCount) * 100.0, 0, 100);
            }

            return new(
                Descriptor,
                _state,
                _lastStopReason,
                _autoStartEnabled,
                _createdAt,
                _startedAt,
                _stoppedAt,
                _lastHeartbeatAt,
                _lastActivityAt,
                _lastActivity,
                _statusText,
                _startThreadId,
                _lastThreadId,
                _startCount,
                _restartCount,
                _faultCount,
                _loopCount,
                _avgLoopMilliseconds,
                _maxLoopMilliseconds,
                estimatedLoadPercent,
                _lastMemoryBytes,
                _lastExceptionType,
                _lastExceptionMessage,
                new Dictionary<string, long>(_counters, StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, string>(_details, StringComparer.OrdinalIgnoreCase),
                _ownedDisposables.Count);
        }
    }
}
