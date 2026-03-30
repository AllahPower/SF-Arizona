namespace SFSharp;

public sealed partial class SFEvents
{
    public IDisposable OnIncomingPacket<TPacket>(Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return GetOrCreateIncomingPacketChannel<TPacket>().Subscribe(handler);
    }

    public IAsyncEnumerable<TPacket> StreamIncomingPacket<TPacket>(CancellationToken token = default)
    {
        return GetOrCreateIncomingPacketChannel<TPacket>().Stream(token);
    }

    public IDisposable OnOutgoingPacket<TPacket>(Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return GetOrCreateOutgoingPacketChannel<TPacket>().Subscribe(handler);
    }

    public IAsyncEnumerable<TPacket> StreamOutgoingPacket<TPacket>(CancellationToken token = default)
    {
        return GetOrCreateOutgoingPacketChannel<TPacket>().Stream(token);
    }
}
