namespace SFSharp.Runtime.Bootstrap;

/// <summary>Queues work onto the game's main thread, drained once per <c>WinMainLoop</c> tick.</summary>
public sealed class MainThreadDispatcher(SFSynchronizationContext context)
{
    public SFSynchronizationContext Context => context;

    public void Post(Action action)
    {
        context.Post(static state => ((Action)state!)(), action);
    }

    /// <summary>
    /// Drains queued continuations once, for main-thread code that waits on async work whose continuations
    /// come back to the main thread (for example plugin unload waiting for cancelled modules). Main thread only.
    /// </summary>
    public void Pump()
    {
        context.ProcLoop();
    }
}
