namespace SFSharp.Runtime.Samp;

/// <summary>Installs a host-owned hook once, without keeping plugin callbacks in its native delegate.</summary>
internal sealed class SampActionEvent<T>
{
    private readonly MainThreadDispatcher _mainThread;
    private readonly EntityEventSource<T> _source;
    private readonly Lazy<IDisposable> _binding;

    public SampActionEvent(string name, MainThreadDispatcher mainThread, Func<Action<T>, IDisposable> attach)
    {
        _mainThread = mainThread;
        _source = new(name);
        // Keep native hooks resident after the last subscriber leaves. Repeated module starts must not
        // patch the same entry again or suspend game threads just to remove an unused managed callback.
        _binding = new(() => attach(Publish), LazyThreadSafetyMode.None);
    }

    public IDisposable Subscribe(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_mainThread.IsMainThread)
            throw new InvalidOperationException("SA-MP action subscriptions require the game thread.");

        _ = _binding.Value;
        return _source.Subscribe(handler);
    }

    private void Publish(T value)
    {
        if (!_source.HasHandlers) return;
        if (_mainThread.IsMainThread)
            _source.Raise(value);
        else
            Queue(value);
    }

    // Keep the captured value in the worker-only path; otherwise the compiler allocates a closure
    // even when Publish runs synchronously on the game thread.
    private void Queue(T value) => _mainThread.Post(() => _source.Raise(value));
}
