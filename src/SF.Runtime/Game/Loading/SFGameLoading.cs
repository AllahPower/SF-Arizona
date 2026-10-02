using System.Diagnostics;

namespace SFSharp.Runtime.Game.Loading;

/// <summary>
/// <see cref="ISFGameLoading"/> driven by the bootstrap (<see cref="SFGameLoadStage.Startup"/>,
/// <see cref="SFGameLoadStage.NetGameReady"/>) and the CGame initialisation hooks. Handlers run synchronously
/// on the game thread; awaiting <see cref="WhenStageAsync"/> resumes through the main-thread queue, which
/// is pumped only between GTA main-loop states, so in-stage work must use <see cref="Subscribe"/>.
/// </summary>
public sealed class SFGameLoading : ISFGameLoading
{
    private static readonly TimeSpan SlowHandlerThreshold = TimeSpan.FromMilliseconds(100);

    private readonly Action<Exception> _reportException;
    private readonly Func<int> _readGameState;
    private readonly Dictionary<SFGameLoadStage, List<Subscription>> _handlers = [];
    private readonly Dictionary<SFGameLoadStage, TaskCompletionSource> _waiters = [];
    private readonly object _sync = new();
    private SFGameLoadStage _stage;

    public SFGameLoading(Action<Exception> reportException, Func<int> readGameState)
    {
        _reportException = reportException;
        _readGameState = readGameState;
    }

    public SFGameLoadStage Stage
    {
        get
        {
            lock (_sync)
            {
                return _stage;
            }
        }
    }

    public int GameState => _readGameState();

    public IDisposable Subscribe(SFGameLoadStage stage, Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Subscription subscription = new(this, stage, handler);
        lock (_sync)
        {
            if (stage <= _stage)
            {
                return subscription;
            }

            if (!_handlers.TryGetValue(stage, out List<Subscription>? list))
            {
                _handlers[stage] = list = [];
            }

            list.Add(subscription);
        }

        return subscription;
    }

    public Task WhenStageAsync(SFGameLoadStage stage)
    {
        lock (_sync)
        {
            if (stage <= _stage)
            {
                return Task.CompletedTask;
            }

            if (!_waiters.TryGetValue(stage, out TaskCompletionSource? waiter))
            {
                _waiters[stage] = waiter = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            return waiter.Task;
        }
    }

    /// <summary>Advances to <paramref name="stage"/>, skipping any stage in between, and runs its handlers.</summary>
    public void Reach(SFGameLoadStage stage)
    {
        Subscription[] handlers;
        List<TaskCompletionSource> completed = [];
        lock (_sync)
        {
            if (stage <= _stage)
            {
                SFLog.Warn($"Game load stage {stage} reported after {_stage}, ignored");
                return;
            }

            for (SFGameLoadStage skipped = _stage + 1; skipped < stage; skipped++)
            {
                int skippedHandlers = _handlers.Remove(skipped, out List<Subscription>? list) ? list.Count : 0;
                SFLog.Debug($"Game load stage {skipped} skipped, {skippedHandlers} handler(s) not called");
                if (_waiters.Remove(skipped, out TaskCompletionSource? waiter))
                {
                    completed.Add(waiter);
                }
            }

            _stage = stage;
            handlers = _handlers.Remove(stage, out List<Subscription>? current) ? [.. current] : [];
            if (_waiters.Remove(stage, out TaskCompletionSource? stageWaiter))
            {
                completed.Add(stageWaiter);
            }
        }

        SFLog.Info($"Game load stage {stage} gGameState={GameState} handlers={handlers.Length}");
        foreach (Subscription subscription in handlers)
        {
            subscription.Invoke(_reportException, stage);
        }

        foreach (TaskCompletionSource waiter in completed)
        {
            waiter.TrySetResult();
        }
    }

    private void Remove(Subscription subscription)
    {
        lock (_sync)
        {
            if (_handlers.TryGetValue(subscription.Stage, out List<Subscription>? list))
            {
                list.Remove(subscription);
            }
        }
    }

    private sealed class Subscription(SFGameLoading owner, SFGameLoadStage stage, Action handler) : IDisposable
    {
        private SFGameLoading? _owner = owner;

        public SFGameLoadStage Stage { get; } = stage;

        public void Invoke(Action<Exception> reportException, SFGameLoadStage stage)
        {
            if (_owner is null)
            {
                return;
            }

            long started = Stopwatch.GetTimestamp();
            try
            {
                handler();
            }
            catch (Exception ex)
            {
                reportException(ex);
            }

            TimeSpan elapsed = Stopwatch.GetElapsedTime(started);
            if (elapsed > SlowHandlerThreshold)
            {
                SFLog.Warn($"Game load stage {stage} handler {handler.Method.DeclaringType?.FullName}.{handler.Method.Name} took {elapsed.TotalMilliseconds:0} ms");
            }
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _owner, null)?.Remove(this);
        }
    }
}
