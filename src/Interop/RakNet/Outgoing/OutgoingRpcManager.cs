using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public sealed class OutgoingRpcManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly Dictionary<int, List<Action<OutgoingRpcArgs>>> _listeners = new();

    public bool HasSubscribers(int rpcId)
    {
        lock (_sync)
        {
            return _listeners.TryGetValue(rpcId, out List<Action<OutgoingRpcArgs>>? list) && list.Count > 0;
        }
    }

    public bool HasSubscribers(ERpcId rpcId)
    {
        return HasSubscribers((int)rpcId);
    }

    public RpcSubscription Subscribe(ERpcId rpcId, Action<OutgoingRpcArgs> handler)
    {
        return Subscribe((int)rpcId, handler);
    }

    public RpcSubscription Subscribe(int rpcId, Action<OutgoingRpcArgs> handler)
    {
        lock (_sync)
        {
            if (!_listeners.TryGetValue(rpcId, out List<Action<OutgoingRpcArgs>>? list))
            {
                list = new List<Action<OutgoingRpcArgs>>();
                _listeners[rpcId] = list;
            }

            list.Add(handler);
        }

        return new RpcSubscription(() =>
        {
            lock (_sync)
            {
                if (!_listeners.TryGetValue(rpcId, out List<Action<OutgoingRpcArgs>>? list))
                {
                    return;
                }

                list.Remove(handler);
                if (list.Count == 0)
                {
                    _listeners.Remove(rpcId);
                }
            }
        });
    }

    internal void Dispatch(int rpcId, byte[] packet, int dataBitLength)
    {
        Action<OutgoingRpcArgs>[] snapshot;
        lock (_sync)
        {
            if (!_listeners.TryGetValue(rpcId, out List<Action<OutgoingRpcArgs>>? list) || list.Count == 0)
            {
                return;
            }

            snapshot = list.ToArray();
        }

        unsafe
        {
            fixed (byte* packetPtr = packet)
            {
                OutgoingRpcArgs args = new(rpcId, (nint)packetPtr, dataBitLength);
                foreach (Action<OutgoingRpcArgs> listener in snapshot)
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
