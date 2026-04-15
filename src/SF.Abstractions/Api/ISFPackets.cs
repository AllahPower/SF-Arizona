namespace SFSharp;

/// <summary>
/// Plugin-facing raw packet transport facade. Frames are copied out of the RakNet buffers before
/// they are surfaced to plugin code.
/// </summary>
public interface ISFPackets
{
    IDisposable SubscribeIncoming(int packetId, Action<IncomingPacketFrame> handler);
    IAsyncEnumerable<IncomingPacketFrame> StreamIncoming(int packetId, CancellationToken token = default);

    IDisposable SubscribeOutgoing(int packetId, Action<OutgoingPacketFrame> handler);
    IAsyncEnumerable<OutgoingPacketFrame> StreamOutgoing(int packetId, CancellationToken token = default);
}
