namespace SFSharp.Abstractions.Network;

/// <summary>
/// Plugin-facing raw packet transport facade. Frames are copied out of the RakNet buffers before
/// they are surfaced to plugin code.
/// </summary>
/// <remarks>
/// Subscribe/Stream registration is thread-safe; handlers fire on the main game thread. Filter
/// registration is thread-safe, but filter callbacks run synchronously on the RakNet hook thread -
/// see <see cref="SFPacketFilterCallback"/>.
/// </remarks>
public interface ISFPackets
{
    IDisposable SubscribeIncoming(int packetId, Action<IncomingPacketFrame> handler);
    IAsyncEnumerable<IncomingPacketFrame> StreamIncoming(int packetId, CancellationToken token = default);

    IDisposable SubscribeOutgoing(int packetId, Action<OutgoingPacketFrame> handler);
    IAsyncEnumerable<OutgoingPacketFrame> StreamOutgoing(int packetId, CancellationToken token = default);

    /// <summary>
    /// Registers a synchronous incoming packet filter. Return <see langword="true"/> from the
    /// callback to cancel (drop) the packet before SA-MP processes it.
    /// </summary>
    IDisposable RegisterIncomingFilter(int packetId, SFPacketFilterCallback filter);

    /// <summary>Registers a catch-all incoming packet filter across all packet ids.</summary>
    IDisposable RegisterIncomingFilter(SFPacketFilterCallback filter);

    /// <summary>
    /// Registers a synchronous outgoing packet filter. Return <see langword="true"/> from the
    /// callback to cancel (drop) the packet before it is sent.
    /// </summary>
    IDisposable RegisterOutgoingFilter(int packetId, SFPacketFilterCallback filter);

    /// <summary>Registers a catch-all outgoing packet filter across all packet ids.</summary>
    IDisposable RegisterOutgoingFilter(SFPacketFilterCallback filter);
}
