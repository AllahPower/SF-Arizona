using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace SFSharp;

public class SFSynchronizationContext : SynchronizationContext
{
    private static readonly Lock _queueLock = new();

    private static Queue<WorkItem> _queue = new();
    private static Queue<WorkItem> _lastQueue = new();
    private static ConcurrentBag<SyncWorkItem> _syncPool = new();

    private readonly int _mainThreadId = Environment.CurrentManagedThreadId;

    public override void Send(SendOrPostCallback d, object? state)
    {
        if (_mainThreadId == Environment.CurrentManagedThreadId)
        {
            d(state);
            return;
        }

        var sync = _syncPool.TryTake(out var existing) ? existing : new();
        lock (_queueLock)
        {
            _queue.Enqueue(new WorkItem(d, state, sync));
        }

        sync.Gate.Wait();
        sync.Gate.Reset();

        ExceptionDispatchInfo? fault = sync.Fault;
        sync.Fault = null;
        _syncPool.Add(sync);

        fault?.Throw();
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_queueLock)
        {
            _queue.Enqueue(new WorkItem(d, state, null));
        }
    }

    internal void ProcLoop()
    {
        lock (_queueLock)
        {
            if (_queue.Count == 0) return;
            (_queue, _lastQueue) = (_lastQueue, _queue);
        }
        while (_lastQueue.TryDequeue(out var entry))
        {
            try
            {
                entry.Callback(entry.State);
            }
            catch (Exception ex)
            {
                if (entry.Sync is not null)
                {
                    entry.Sync.Fault = ExceptionDispatchInfo.Capture(ex);
                }
                else
                {
                    SFBootstrap.ProcessException(ex);
                }
            }
            entry.Sync?.Gate.Set();
        }
    }

    private readonly record struct WorkItem(SendOrPostCallback Callback, object? State, SyncWorkItem? Sync);

    private sealed class SyncWorkItem
    {
        public readonly ManualResetEventSlim Gate = new();
        public ExceptionDispatchInfo? Fault;
    }
}
