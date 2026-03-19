namespace SFSharp;

public sealed class IncomingPacketManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<IncomingPacketArgs>>> _listeners = new();

    public bool HasSubscribers(int packetId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(packetId, out List<Action<IncomingPacketArgs>>? list) && list.Count > 0;
        }
    }

    public bool HasSubscribers(PacketId packetId)
    {
        return HasSubscribers((int)packetId);
    }

    public bool HasAnySubscribers()
    {
        lock (_sync)
        {
            foreach (var pair in _listeners)
            {
                if (pair.Value.Count > 0) return true;
            }

            return false;
        }
    }

    public RpcSubscription Subscribe(PacketId packetId, Action<IncomingPacketArgs> handler)
    {
        return Subscribe((int)packetId, handler);
    }

    public RpcSubscription Subscribe(int packetId, Action<IncomingPacketArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(packetId, out List<Action<IncomingPacketArgs>>? list))
            {
                list = new List<Action<IncomingPacketArgs>>();
                _listeners[packetId] = list;
            }

            list.Add(handler);
        }

        return new RpcSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(packetId, out List<Action<IncomingPacketArgs>>? list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    _listeners.Remove(packetId);
                }
            }
        });
    }

    internal void Dispatch(int packetId, byte[] data, int dataBitLength)
    {
        Action<IncomingPacketArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(packetId, out List<Action<IncomingPacketArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                IncomingPacketArgs args = new(packetId, (nint)dataPtr, dataBitLength);
                foreach (Action<IncomingPacketArgs> listener in snapshot)
                {
                    listener(args);
                }
            }
        }
    }

    public void Dispose()
    {
        lock (_sync)
        {
            _listeners.Clear();
        }
    }
}
