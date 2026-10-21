using System.Collections.Concurrent;
using System.Runtime.ExceptionServices;

namespace SFSharp.Runtime.Bootstrap;

public class SFSynchronizationContext(Action<Exception> reportException) : SynchronizationContext
{
    private readonly Lock _queueLock = new();

    private readonly Queue<WorkItem> _queue = new();
    private readonly ConcurrentBag<SyncWorkItem> _syncPool = new();

    private readonly int _mainThreadId = Environment.CurrentManagedThreadId;
    public bool IsMainThread => _mainThreadId == Environment.CurrentManagedThreadId;

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
        if (!IsMainThread) throw new InvalidOperationException("Only the game thread may drain its queue.");
        int remaining;
        lock (_queueLock)
        {
            remaining = _queue.Count;
        }
        // Nested pumps (plugin unload) share the same FIFO instead of swapping the active buffer.
        while (remaining-- > 0)
        {
            WorkItem entry;
            lock (_queueLock)
            {
                if (!_queue.TryDequeue(out entry)) break;
            }
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
                    try { reportException(ex); }
                    catch { /* Reporting must not strand synchronous senders behind this item. */ }
                }
            }
            finally { entry.Sync?.Gate.Set(); }
        }
    }

    private readonly record struct WorkItem(SendOrPostCallback Callback, object? State, SyncWorkItem? Sync);

    private sealed class SyncWorkItem
    {
        public readonly ManualResetEventSlim Gate = new();
        public ExceptionDispatchInfo? Fault;
    }
}
