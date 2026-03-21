using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace SFSharp;

public enum ModuleExecutionModel
{
    MainThread,
    BackgroundWorker,
    Hybrid
}

public enum ModuleRestartPolicy
{
    Manual,
    OnFault
}

public enum ModuleLifecycleState
{
    Created,
    Starting,
    Running,
    Stopping,
    Stopped,
    Faulted
}

public enum ModuleStopReason
{
    None,
    Completed,
    UserRequested,
    RestartRequested,
    ContainerShutdown,
    Faulted
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class SFModuleAttribute(string id, string displayName) : Attribute
{
    public string Id { get; } = id;
    public string DisplayName { get; } = displayName;
    public string Category { get; init; } = "General";
    public string Description { get; init; } = string.Empty;
    public bool DefaultEnabled { get; init; } = true;
    public ModuleExecutionModel ExecutionModel { get; init; } = ModuleExecutionModel.MainThread;
    public ModuleRestartPolicy RestartPolicy { get; init; } = ModuleRestartPolicy.Manual;
    public int Order { get; init; }
}

public interface ISFModule
{
    Task RunAsync(ModuleContext context);
}

public abstract class SFModuleBase : ISFModule
{
    protected ModuleContext Context { get; private set; } = null!;
    protected ILogger Log { get; private set; } = null!;

    public async Task RunAsync(ModuleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        Context = context;
        Log = CreateLogger(context);

        await OnStartingAsync();
        try
        {
            await ExecuteAsync(context.CancellationToken);
            await OnCompletedAsync();
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            await OnFaultedAsync(ex);
            throw;
        }
        finally
        {
            try
            {
                await OnStoppingAsync();
            }
            finally
            {
                await OnStoppedAsync();
                Context = null!;
                Log = null!;
            }
        }
    }

    protected virtual ILogger CreateLogger(ModuleContext context)
    {
        return SFLoggerProvider.Instance.CreateLogger(context.Descriptor.Id);
    }

    protected virtual Task OnStartingAsync() => Task.CompletedTask;
    protected virtual Task OnCompletedAsync() => Task.CompletedTask;
    protected virtual Task OnFaultedAsync(Exception exception) => Task.CompletedTask;
    protected virtual Task OnStoppingAsync() => Task.CompletedTask;
    protected virtual Task OnStoppedAsync() => Task.CompletedTask;

    protected abstract Task ExecuteAsync(CancellationToken cancellationToken);
}

public sealed record ModuleDescriptor(
    string Id,
    string DisplayName,
    string Category,
    string Description,
    bool DefaultEnabled,
    ModuleExecutionModel ExecutionModel,
    ModuleRestartPolicy RestartPolicy,
    int Order,
    Type ModuleType)
{
    public static ModuleDescriptor FromType(Type moduleType)
    {
        ArgumentNullException.ThrowIfNull(moduleType);
        SFModuleAttribute? metadata = moduleType.GetCustomAttribute<SFModuleAttribute>();
        if (metadata is null)
        {
            throw new InvalidOperationException($"Module type {moduleType.FullName} is missing [SFModule].");
        }

        if (string.IsNullOrWhiteSpace(metadata.Id))
        {
            throw new InvalidOperationException($"Module type {moduleType.FullName} has an empty module id.");
        }

        return new(
            metadata.Id.Trim(),
            string.IsNullOrWhiteSpace(metadata.DisplayName) ? moduleType.Name : metadata.DisplayName.Trim(),
            string.IsNullOrWhiteSpace(metadata.Category) ? "General" : metadata.Category.Trim(),
            metadata.Description?.Trim() ?? string.Empty,
            metadata.DefaultEnabled,
            metadata.ExecutionModel,
            metadata.RestartPolicy,
            metadata.Order,
            moduleType);
    }
}

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
    public TimeSpan? Uptime => StartedAt is null ? null : (StoppedAt ?? DateTimeOffset.UtcNow) - StartedAt.Value;
}

public sealed class ModuleContext : IDisposable
{
    private readonly ModuleRuntimeInfo _runtime;
    private int _disposed;

    internal ModuleContext(ModuleDescriptor descriptor, ModuleRuntimeInfo runtime, CancellationToken cancellationToken)
    {
        Descriptor = descriptor;
        _runtime = runtime;
        CancellationToken = cancellationToken;
    }

    public ModuleDescriptor Descriptor { get; }
    public CancellationToken CancellationToken { get; }

    public void Heartbeat(string? activity = null)
    {
        _runtime.RecordHeartbeat(activity);
    }

    public void ReportActivity(string activity)
    {
        _runtime.RecordActivity(activity);
    }

    public void SetStatusText(string? value)
    {
        _runtime.SetStatusText(value);
    }

    public void IncrementCounter(string counterName, long delta = 1)
    {
        _runtime.IncrementCounter(counterName, delta);
    }

    public void SetDetail(string key, string? value)
    {
        _runtime.SetDetail(key, value);
    }

    internal ModuleLoopScope TrackLoop(string? activity = null)
    {
        return new ModuleLoopScope(_runtime, Stopwatch.GetTimestamp(), activity);
    }

    public IDisposable RegisterDisposable(IDisposable disposable)
    {
        ArgumentNullException.ThrowIfNull(disposable);
        _runtime.RegisterDisposable(disposable);
        return disposable;
    }

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

    public Task RunBackground(Func<Task> work)
    {
        ArgumentNullException.ThrowIfNull(work);
        return Task.Run(async () =>
        {
            _runtime.RecordActivity("background-work");
            await work();
        }, CancellationToken);
    }

    public ModuleRuntimeSnapshot GetSnapshot()
    {
        return _runtime.CreateSnapshot();
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _runtime.DisposeOwnedDisposables();
    }
}

internal readonly struct ModuleLoopScope(ModuleRuntimeInfo runtime, long startTicks, string? activity) : IDisposable
{
    public void Dispose()
    {
        runtime.RecordLoop(startTicks, Stopwatch.GetTimestamp(), activity);
    }
}


internal sealed class ModuleRuntimeInfo
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

    public ModuleRuntimeInfo(ModuleDescriptor descriptor, bool autoStartEnabled)
    {
        Descriptor = descriptor;
        _autoStartEnabled = autoStartEnabled;
    }

    public ModuleDescriptor Descriptor { get; }

    private static readonly long MemoryPollIntervalTicks = Stopwatch.Frequency * 2; // 2 seconds
    private const int CircuitBreakerFaultLimit = 5;
    private static readonly long CircuitBreakerWindowTicks = Stopwatch.Frequency * 60; // 60 seconds

    private void PollMemoryIfDue()
    {
        long now = Stopwatch.GetTimestamp();
        if (now - _lastMemoryPollTicks >= MemoryPollIntervalTicks)
        {
            _lastMemoryPollTicks = now;
            _lastMemoryBytes = GC.GetTotalMemory(false);
        }
    }

    public void RecordFaultForCircuitBreaker()
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

    public void PrepareForStart(bool autoStartEnabled)
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

    public void MarkRunning()
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

    public void RequestStop(ModuleStopReason reason)
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

    public void MarkCompleted()
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

    public void MarkFault(Exception ex)
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

    public void IncrementRestartCount()
    {
        Interlocked.Increment(ref _restartCount);
    }

    public void RecordHeartbeat(string? activity)
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

    public void RecordActivity(string activity)
    {
        lock (_sync)
        {
            _lastActivityAt = DateTimeOffset.UtcNow;
            _lastActivity = activity;
            _lastThreadId = Environment.CurrentManagedThreadId;
        }
    }

    public void RecordLoop(long startTicks, long endTicks, string? activity)
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

    public void IncrementCounter(string counterName, long delta)
    {
        _counters.AddOrUpdate(counterName, delta, (_, current) => current + delta);
    }

    public void SetDetail(string key, string? value)
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

    public void SetStatusText(string? value)
    {
        lock (_sync)
        {
            _statusText = value;
        }
    }

    public void SetAutoStartEnabled(bool value)
    {
        lock (_sync)
        {
            _autoStartEnabled = value;
        }
    }

    public void RegisterDisposable(IDisposable disposable)
    {
        lock (_sync)
        {
            _ownedDisposables.Add(disposable);
        }
    }

    public void DisposeOwnedDisposables()
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

    public void ClearTelemetry()
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
