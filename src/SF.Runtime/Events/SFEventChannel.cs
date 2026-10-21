using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SFSharp.Runtime.Events;

internal sealed class SFEventChannel<T>
{
    private Unsubscriber[] _callbacks = [];
    private Channel<T>[] _streams = [];
    private readonly Func<Action<T>, IDisposable> _attach;
    private readonly Lock _sync = new();
    private IDisposable? _binding;
    private int _consumerCount;

    public SFEventChannel(Func<Action<T>, IDisposable> attach)
    {
        _attach = attach;
    }

    private void Publish(T value)
    {
        Unsubscriber[] snapshot;
        Channel<T>[] streams;
        lock (_sync)
        {
            snapshot = _callbacks;
            streams = _streams;
        }

        foreach (Unsubscriber cb in snapshot)
        {
            try
            {
                cb.Invoke(value);
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"SFEventChannel callback failed eventType={typeof(T).Name}");
            }
        }

        foreach (Channel<T> stream in streams)
        {
            if (!stream.Writer.TryWrite(value))
            {
                SFLog.Warn($"SFEventChannel stream write skipped eventType={typeof(T).Name}");
            }
        }
    }

    public IDisposable Subscribe(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        var subscription = new Unsubscriber(this, handler);
        lock (_sync)
        {
            EnsureBound();
            _callbacks = [.. _callbacks, subscription];
            _consumerCount++;
        }

        return subscription;
    }

    public async IAsyncEnumerable<T> Stream([EnumeratorCancellation] CancellationToken token = default)
    {
        var stream = SFChannel.CreateUnbounded<T>();

        lock (_sync)
        {
            EnsureBound();
            _streams = [.. _streams, stream];
            _consumerCount++;
        }

        try
        {
            await foreach (T item in stream.Reader.ReadAllAsync(token))
            {
                yield return item;
            }
        }
        finally
        {
            lock (_sync)
            {
                int index = Array.IndexOf(_streams, stream);
                _streams = [.. _streams[..index], .. _streams[(index + 1)..]];
                _consumerCount--;
                ReleaseBindingIfUnused();
            }

            stream.Writer.TryComplete();
        }
    }

    private void EnsureBound()
    {
        _binding ??= _attach(Publish);
    }

    private void ReleaseBindingIfUnused()
    {
        if (_consumerCount != 0 || _binding is null)
        {
            return;
        }

        _binding.Dispose();
        _binding = null;
    }

    private sealed class Unsubscriber : IDisposable
    {
        private SFEventChannel<T>? _channel;
        private Action<T>? _handler;

        public Unsubscriber(SFEventChannel<T> channel, Action<T> handler)
        {
            _channel = channel;
            _handler = handler;
        }

        public void Invoke(T value) => Volatile.Read(ref _handler)?.Invoke(value);

        public void Dispose()
        {
            SFEventChannel<T>? channel = Interlocked.Exchange(ref _channel, null);
            if (channel is null) return;
            Interlocked.Exchange(ref _handler, null);

            lock (channel._sync)
            {
                int index = Array.IndexOf(channel._callbacks, this);
                channel._callbacks = [.. channel._callbacks[..index], .. channel._callbacks[(index + 1)..]];
                channel._consumerCount--;
                channel.ReleaseBindingIfUnused();
            }
        }
    }
}
