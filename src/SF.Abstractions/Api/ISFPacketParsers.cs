namespace SFSharp;

/// <summary>Typed parsed packet facade covering plain and Arizona packet transports.</summary>
public interface ISFPacketParsers
{
    IDisposable BindIncoming<TPacket>(Action<TPacket> handler, CancellationToken token = default);

    IDisposable BindOutgoing<TPacket>(Action<TPacket> handler, CancellationToken token = default);

    IAsyncEnumerable<TPacket> StreamIncoming<TPacket>(CancellationToken token = default);

    IAsyncEnumerable<TPacket> StreamOutgoing<TPacket>(CancellationToken token = default);
}
