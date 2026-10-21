namespace SFSharp.Runtime.Events;

/// <summary>
/// Copy-on-write handler list for synchronous entity events. A throwing handler is logged and does not
/// stop the remaining handlers or the hooked game function.
/// </summary>
internal sealed class EntityEventSource<T>(string name)
{
    private readonly Lock _gate = new();
    private Action<T>[] _handlers = [];

    public bool HasHandlers => Volatile.Read(ref _handlers).Length != 0;

    public IDisposable Subscribe(Action<T> handler)
    {
        lock (_gate)
        {
            _handlers = [.. _handlers, handler];
        }

        return new Subscription(this, handler);
    }

    public void Raise(T value)
    {
        foreach (Action<T> handler in Volatile.Read(ref _handlers))
        {
            try
            {
                handler(value);
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"{name} handler {handler.Method.DeclaringType?.FullName}.{handler.Method.Name}");
            }
        }
    }

    private void Unsubscribe(Action<T> handler)
    {
        lock (_gate)
        {
            int index = Array.IndexOf(_handlers, handler);
            if (index >= 0)
            {
                _handlers = [.. _handlers[..index], .. _handlers[(index + 1)..]];
            }
        }
    }

    private sealed class Subscription(EntityEventSource<T> owner, Action<T> handler) : IDisposable
    {
        private EntityEventSource<T>? _owner = owner;
        private Action<T>? _handler = handler;

        public void Dispose()
        {
            EntityEventSource<T>? source = Interlocked.Exchange(ref _owner, null);
            Action<T>? callback = Interlocked.Exchange(ref _handler, null);
            if (source is not null && callback is not null) source.Unsubscribe(callback);
        }
    }
}
