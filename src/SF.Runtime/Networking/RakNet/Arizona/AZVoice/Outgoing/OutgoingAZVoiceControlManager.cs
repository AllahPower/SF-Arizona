namespace SFSharp.Runtime.Network.RakNet.Arizona;

// Outgoing counterpart of IncomingAZVoiceControlManager: fans AZVoice control
// messages the client emits (packet 252 sub-RPCs) out to per-subId subscribers.
// Fed by OutgoingAZVoiceRpcHook via the network dispatcher.
public sealed class OutgoingAZVoiceControlManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<OutgoingArizonaPacketArgs>>> _listeners = new();

    public bool HasSubscribers(int subId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(subId, out List<Action<OutgoingArizonaPacketArgs>>? list) && list.Count > 0;
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

    public NetworkSubscription Subscribe(int subId, Action<OutgoingArizonaPacketArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(subId, out List<Action<OutgoingArizonaPacketArgs>>? list))
            {
                list = new List<Action<OutgoingArizonaPacketArgs>>();
                _listeners[subId] = list;
            }

            list.Add(handler);
        }

        return new NetworkSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(subId, out List<Action<OutgoingArizonaPacketArgs>>? list))
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
        Action<OutgoingArizonaPacketArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(subId, out List<Action<OutgoingArizonaPacketArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                OutgoingArizonaPacketArgs args = new((int)EPacketId.AZVoice, subId, (nint)dataPtr, AZVoiceTransport.ControlPayloadBitOffset, dataBitLength - AZVoiceTransport.ControlPayloadBitOffset);
                foreach (Action<OutgoingArizonaPacketArgs> listener in snapshot)
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
