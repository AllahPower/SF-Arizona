using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace SFSharp;

internal sealed class SFEventChannel<T>
{
    private readonly List<Action<T>> _callbacks = new();
    private readonly List<Channel<T>> _streams = new();
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
        Action<T>[] snapshot;
        Channel<T>[] streams;
        lock (_sync)
        {
            snapshot = _callbacks.Count > 0 ? [.. _callbacks] : [];
            streams = _streams.Count > 0 ? [.. _streams] : [];
        }

        foreach (Action<T> cb in snapshot)
        {
            try
            {
                cb(value);
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
        lock (_sync)
        {
            EnsureBound();
            _callbacks.Add(handler);
            _consumerCount++;
        }

        return new Unsubscriber(this, handler);
    }

    public async IAsyncEnumerable<T> Stream([EnumeratorCancellation] CancellationToken token = default)
    {
        Channel<T> stream = Channel.CreateUnbounded<T>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });

        lock (_sync)
        {
            EnsureBound();
            _streams.Add(stream);
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
                _streams.Remove(stream);
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
        private readonly SFEventChannel<T> _channel;
        private readonly Action<T> _handler;
        private int _disposed;

        public Unsubscriber(SFEventChannel<T> channel, Action<T> handler)
        {
            _channel = channel;
            _handler = handler;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            lock (_channel._sync)
            {
                _channel._callbacks.Remove(_handler);
                _channel._consumerCount--;
                _channel.ReleaseBindingIfUnused();
            }
        }
    }
}
