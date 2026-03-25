using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public sealed class IncomingAZVoiceDataManager : IDisposable
{
    private readonly Lock _sync = new();
    private readonly List<Action<IncomingPacketArgs>> _listeners = new();

    public bool HasSubscribers()
    {
        lock (_sync)
        {
            return _listeners.Count > 0;
        }
    }

    public NetworkSubscription Subscribe(Action<IncomingPacketArgs> handler)
    {
        lock (_sync)
        {
            _listeners.Add(handler);
        }

        return new NetworkSubscription(() =>
        {
            lock (_sync)
            {
                _listeners.Remove(handler);
            }
        });
    }

    internal void Dispatch(byte[] data, int dataBitLength)
    {
        Action<IncomingPacketArgs>[] snapshot;
        lock (_sync)
        {
            if (_listeners.Count == 0)
            {
                return;
            }

            snapshot = _listeners.ToArray();
        }

        unsafe
        {
            fixed (byte* dataPtr = data)
            {
                IncomingPacketArgs args = new((int)EPacketId.AZVoice, (nint)dataPtr, dataBitLength);
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
