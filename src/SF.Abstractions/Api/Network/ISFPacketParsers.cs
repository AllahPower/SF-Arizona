namespace SFSharp.Abstractions.Network;

/// <summary>SA-MP packet parsers with explicitly registered optional custom decoders.</summary>
/// <remarks>Bind/Stream registration is thread-safe; handlers fire on the main game thread.</remarks>
public interface ISFPacketParsers
{
    IDisposable RegisterMetadata(int packetId, Func<bool, ReadOnlyMemory<byte>, int, SFPacketMetadata?> describe);
    SFPacketMetadata? Describe(int packetId, bool incoming, ReadOnlyMemory<byte> data, int dataBitLength);
    IDisposable RegisterObservationDecoder(int packetId, Func<SFPacketObservation, SFSharp.Abstractions.Parsing.IParsedPacket?> decoder);
    bool TryParseObservation(SFPacketObservation observation, out SFSharp.Abstractions.Parsing.IParsedPacket? packet);

    /// <summary>Registers a decoder for a non-standard packet ID. Dispose before unloading its plugin.</summary>
    IDisposable RegisterIncomingDecoder(int packetId, Func<IncomingPacketFrame, SFSharp.Abstractions.Parsing.IParsedIncomingPacket?> decoder);

    /// <summary>Registers a decoder for a non-standard packet ID. Dispose before unloading its plugin.</summary>
    IDisposable RegisterOutgoingDecoder(int packetId, Func<OutgoingPacketFrame, SFSharp.Abstractions.Parsing.IParsedOutgoingPacket?> decoder);

    bool TryParseIncoming(IncomingPacketFrame frame, out SFSharp.Abstractions.Parsing.IParsedIncomingPacket? packet);
    bool TryParseOutgoing(OutgoingPacketFrame frame, out SFSharp.Abstractions.Parsing.IParsedOutgoingPacket? packet);

    IDisposable BindIncoming<TPacket>(Action<TPacket> handler, CancellationToken token = default);

    IDisposable BindOutgoing<TPacket>(Action<TPacket> handler, CancellationToken token = default);

    IAsyncEnumerable<TPacket> StreamIncoming<TPacket>(CancellationToken token = default);

    IAsyncEnumerable<TPacket> StreamOutgoing<TPacket>(CancellationToken token = default);
}
