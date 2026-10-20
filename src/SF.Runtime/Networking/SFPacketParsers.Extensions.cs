namespace SFSharp.Runtime.Networking;

public sealed partial class SFPacketParsers
{
    private readonly Lock _decoderSync = new();
    private readonly Dictionary<int, Func<IncomingPacketFrame, IParsedIncomingPacket?>> _incomingDecoders = new();
    private readonly Dictionary<int, Func<OutgoingPacketFrame, IParsedOutgoingPacket?>> _outgoingDecoders = new();
    private readonly Dictionary<int, Func<bool, ReadOnlyMemory<byte>, int, SFPacketMetadata?>> _metadata = new();
    private readonly Dictionary<int, Func<SFPacketObservation, IParsedPacket?>> _observationDecoders = new();

    public IDisposable RegisterObservationDecoder(int packetId, Func<SFPacketObservation, IParsedPacket?> decoder) => RegisterDecoder(_observationDecoders, packetId, decoder);

    public bool TryParseObservation(SFPacketObservation observation, out IParsedPacket? packet)
    {
        packet = null;
        if (observation.DataBitLength < 8 || observation.DataBitLength > (long)observation.Data.Length * 8
            || observation.PayloadBitOffset < 0 || observation.PayloadBitLength < 0
            || (long)observation.PayloadBitOffset + observation.PayloadBitLength > observation.DataBitLength) return false;
        int id = observation.Incoming
            ? new IncomingPacketFrame(observation.PacketId, observation.Data, observation.DataBitLength).EffectivePacketId
            : observation.PacketId;
        Func<SFPacketObservation, IParsedPacket?>? decoder;
        lock (_decoderSync) decoder = _observationDecoders.GetValueOrDefault(id);
        if (decoder is not null) { packet = decoder(observation); return packet is not null; }
        if (observation.Incoming)
        {
            bool parsed = TryParseIncoming(new IncomingPacketFrame(observation.PacketId, observation.Data, observation.DataBitLength), out var incoming);
            packet = incoming;
            return parsed;
        }
        else
        {
            bool parsed = TryParseOutgoing(new OutgoingPacketFrame(observation.PacketId, observation.Data, observation.DataBitLength), out var outgoing);
            packet = outgoing;
            return parsed;
        }
    }

    public IDisposable RegisterMetadata(int packetId, Func<bool, ReadOnlyMemory<byte>, int, SFPacketMetadata?> describe) => RegisterDecoder(_metadata, packetId, describe);

    public SFPacketMetadata? Describe(int packetId, bool incoming, ReadOnlyMemory<byte> data, int dataBitLength)
    {
        if (dataBitLength < 8 || dataBitLength > (long)data.Length * 8) return null;
        Func<bool, ReadOnlyMemory<byte>, int, SFPacketMetadata?>? describe;
        lock (_decoderSync) describe = _metadata.GetValueOrDefault(packetId);
        return describe?.Invoke(incoming, data, dataBitLength);
    }

    public IDisposable RegisterIncomingDecoder(int packetId, Func<IncomingPacketFrame, IParsedIncomingPacket?> decoder) => RegisterDecoder(_incomingDecoders, packetId, decoder);
    public IDisposable RegisterOutgoingDecoder(int packetId, Func<OutgoingPacketFrame, IParsedOutgoingPacket?> decoder) => RegisterDecoder(_outgoingDecoders, packetId, decoder);

    private IDisposable RegisterDecoder<T>(Dictionary<int, T> decoders, int packetId, T decoder) where T : Delegate
    {
        ArgumentNullException.ThrowIfNull(decoder);
        if (packetId is < 0 or > 255) throw new ArgumentOutOfRangeException(nameof(packetId));
        if (Enum.IsDefined((RakNetPacketId)packetId)) throw new ArgumentException("Standard packet decoders cannot be replaced by plugins.", nameof(packetId));
        lock (_decoderSync) decoders.Add(packetId, decoder);
        return new NetworkSubscription(() =>
        {
            lock (_decoderSync) decoders.Remove(packetId);
        });
    }

    public unsafe bool TryParseIncoming(IncomingPacketFrame frame, out IParsedIncomingPacket? packet)
    {
        packet = null;
        if (frame.DataBitLength < 8 || frame.DataBitLength > (long)frame.Data.Length * 8) return false;
        Func<IncomingPacketFrame, IParsedIncomingPacket?>? decoder;
        lock (_decoderSync) decoder = _incomingDecoders.GetValueOrDefault(frame.EffectivePacketId);
        if (decoder is not null)
        {
            packet = decoder(frame);
            return packet is not null;
        }
        fixed (byte* data = frame.Data.Span)
        {
            int prefix = frame.PacketDataBitOffset;
            bool parsed = Registry.TryParseIncoming(new IncomingPacketArgs(frame.EffectivePacketId, (nint)(data + prefix / 8), frame.PacketDataBitLength), out var result);
            packet = result.Packet as IParsedIncomingPacket;
            return parsed && packet is not null;
        }
    }

    public unsafe bool TryParseOutgoing(OutgoingPacketFrame frame, out IParsedOutgoingPacket? packet)
    {
        packet = null;
        if (frame.DataBitLength < 8 || frame.DataBitLength > (long)frame.Data.Length * 8) return false;
        Func<OutgoingPacketFrame, IParsedOutgoingPacket?>? decoder;
        lock (_decoderSync) decoder = _outgoingDecoders.GetValueOrDefault(frame.PacketId);
        if (decoder is not null)
        {
            packet = decoder(frame);
            return packet is not null;
        }
        fixed (byte* data = frame.Data.Span)
        {
            bool parsed = Registry.TryParseOutgoing(new OutgoingPacketArgs(frame.PacketId, (nint)data, frame.DataBitLength), out var result);
            packet = result.Packet as IParsedOutgoingPacket;
            return parsed && packet is not null;
        }
    }
}
