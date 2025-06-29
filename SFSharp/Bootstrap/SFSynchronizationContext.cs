using System.Collections.Concurrent;

namespace SFSharp;

public class SFSynchronizationContext : SynchronizationContext
{
    private static readonly Lock _queueLock = new();

    private static Queue<(SendOrPostCallback d, object? state, ManualResetEventSlim?)> _queue = new();
    private static Queue<(SendOrPostCallback d, object? state, ManualResetEventSlim?)> _lastQueue = new();
    private static ConcurrentBag<ManualResetEventSlim> _mrePool = new();

    public override void Send(SendOrPostCallback d, object? state)
    {
        var mre = _mrePool.TryTake(out var existingMre) ? existingMre : new();
        lock (_queueLock)
        {
            _queue.Enqueue((d, state, mre));
        }
        mre.Wait();
        mre.Reset();
        _mrePool.Add(mre);
    }

    public override void Post(SendOrPostCallback d, object? state)
    {
        lock (_queueLock)
        {
            _queue.Enqueue((d, state, null));
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
            var (d, state, mre) = entry;
            try { d(state); } catch (Exception ex) { SFBootstrap.ProcessException(ex); }
            mre?.Set();
        }
    }
}