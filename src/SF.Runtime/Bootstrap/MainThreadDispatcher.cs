namespace SFSharp.Runtime.Bootstrap;

/// <summary>Queues work onto the game's main thread, drained once per <c>WinMainLoop</c> tick.</summary>
public sealed class MainThreadDispatcher(SFSynchronizationContext context) : ISFMainThread
{
    public SFSynchronizationContext Context => context;
    public bool IsMainThread => context.IsMainThread;

    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        return InvokeAsync(() => { action(); return true; }, cancellationToken);
    }

    public Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested) return Task.FromCanceled<T>(cancellationToken);
        if (IsMainThread)
        {
            try { return Task.FromResult(action()); }
            catch (Exception ex) { return Task.FromException<T>(ex); }
        }

        Invocation<T> invocation = new(action, cancellationToken);
        context.Post(static state => ((Invocation<T>)state!).Execute(), invocation);
        return invocation.Task;
    }

    private sealed class Invocation<T>
    {
        private readonly Lock _gate = new();
        private readonly TaskCompletionSource<T> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly CancellationToken _token;
        private readonly CancellationTokenRegistration _registration;
        private Func<T>? _action;

        public Invocation(Func<T> action, CancellationToken token)
        {
            _action = action;
            _token = token;
            _registration = token.UnsafeRegister(static state => ((Invocation<T>)state!).Cancel(), this);
            if (_completion.Task.IsCompleted) _registration.Dispose();
        }

        public Task<T> Task => _completion.Task;

        private void Cancel()
        {
            lock (_gate)
            {
                if (_action is null) return;
                _action = null;
                _completion.TrySetCanceled(_token);
            }
            _registration.Unregister();
        }

        public void Execute()
        {
            Func<T>? action;
            lock (_gate)
            {
                action = _action;
                _action = null;
            }
            _registration.Dispose();
            if (action is null) return;
            try { _completion.TrySetResult(action()); }
            catch (Exception ex) { _completion.TrySetException(ex); }
        }
    }

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
        if (!IsMainThread) throw new InvalidOperationException("Only the game thread may drain its queue.");
        context.ProcLoop();
    }
}
