namespace SFSharp.Runtime.Network.RakNet.Outgoing;

public sealed class OutgoingPacketManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<OutgoingPacketArgs>>> _listeners = new();

    public bool HasSubscribers(int packetId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(packetId, out List<Action<OutgoingPacketArgs>>? list) && list.Count > 0;
        }
    }

    public bool HasSubscribers(EPacketId packetId)
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

    public NetworkSubscription Subscribe(EPacketId packetId, Action<OutgoingPacketArgs> handler)
    {
        return Subscribe((int)packetId, handler);
    }

    public NetworkSubscription Subscribe(int packetId, Action<OutgoingPacketArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(packetId, out List<Action<OutgoingPacketArgs>>? list))
            {
                list = new List<Action<OutgoingPacketArgs>>();
                _listeners[packetId] = list;
            }

            list.Add(handler);
        }

        return new NetworkSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(packetId, out List<Action<OutgoingPacketArgs>>? list))
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
        Action<OutgoingPacketArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(packetId, out List<Action<OutgoingPacketArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                OutgoingPacketArgs args = new(packetId, (nint)dataPtr, dataBitLength);
                foreach (Action<OutgoingPacketArgs> listener in snapshot)
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
