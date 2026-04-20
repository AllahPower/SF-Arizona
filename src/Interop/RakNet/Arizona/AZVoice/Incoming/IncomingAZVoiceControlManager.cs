namespace SFSharp.Runtime.Network.RakNet.Arizona;

public sealed class IncomingAZVoiceControlManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<IncomingArizonaPacketArgs>>> _listeners = new();

    public bool HasSubscribers(int subId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(subId, out List<Action<IncomingArizonaPacketArgs>>? list) && list.Count > 0;
        }
    }

    public bool HasAnySubscribers()
    {
        lock (_sync)
        {
            foreach (var pair in _listeners)
            {
                if (pair.Value.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }
    }

    public NetworkSubscription Subscribe(int subId, Action<IncomingArizonaPacketArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(subId, out List<Action<IncomingArizonaPacketArgs>>? list))
            {
                list = new List<Action<IncomingArizonaPacketArgs>>();
                _listeners[subId] = list;
            }

            list.Add(handler);
        }

        return new NetworkSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(subId, out List<Action<IncomingArizonaPacketArgs>>? list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    _listeners.Remove(subId);
                }
            }
        });
    }

    internal void Dispatch(int subId, byte[] data, int dataBitLength)
    {
        Action<IncomingArizonaPacketArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(subId, out List<Action<IncomingArizonaPacketArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                IncomingArizonaPacketArgs args = new((int)EPacketId.AZVoice, subId, (nint)dataPtr, AZVoiceTransport.ControlPayloadBitOffset, dataBitLength - AZVoiceTransport.ControlPayloadBitOffset);
                foreach (Action<IncomingArizonaPacketArgs> listener in snapshot)
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
