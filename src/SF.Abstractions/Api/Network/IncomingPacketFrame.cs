using SFSharp.Abstractions.Network.Protocol;

namespace SFSharp.Abstractions.Network;

/// <summary>Copied incoming packet frame detached from the original game memory buffer.</summary>
public readonly record struct IncomingPacketFrame(int PacketId, ReadOnlyMemory<byte> Data, int DataBitLength)
{
    /// <summary>
    /// Optional 32-bit RakNet timestamp from an ID_TIMESTAMP envelope, as received by the client.
    /// Zero is a present timestamp; null means no complete envelope was found.
    /// </summary>
    /// <remarks>
    /// This is a wrapping transport tick value, not UTC, local receive time or proof of the sender's
    /// original clock. Reading it does not remove the envelope from <see cref="Data"/>.
    /// </remarks>
    public uint? RakNetTimestamp => TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope) ? envelope.Timestamp : null;

    /// <summary>Inner packet ID for a complete timestamp envelope; otherwise the original <see cref="PacketId"/>.</summary>
    /// <remarks>Subscriptions and filters continue to use the original wire ID, including 40 for ID_TIMESTAMP.</remarks>
    public int EffectivePacketId => TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope) ? envelope.PacketId : PacketId;

    /// <summary>Bit offset of the inner packet ID in <see cref="Data"/>; zero for an unwrapped packet.</summary>
    public int PacketDataBitOffset => TryGetTimestampEnvelope(out _) ? RakNetTimestampEnvelope.PrefixByteLength * 8 : 0;

    /// <summary>Zero-copy view without a complete timestamp prefix, still including the packet ID.</summary>
    /// <remarks>Only <see cref="PacketDataBitLength"/> bits are valid; this does not validate the inner payload.</remarks>
    public ReadOnlyMemory<byte> PacketData => Data[(PacketDataBitOffset / 8)..];

    /// <summary>Number of valid bits in <see cref="PacketData"/>, excluding a complete timestamp prefix.</summary>
    public int PacketDataBitLength => DataBitLength - PacketDataBitOffset;

    /// <summary>Decodes the complete timestamp header once for callers needing both timestamp and inner ID.</summary>
    public bool TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope)
    {
        envelope = default;
        return PacketId == (int)RakNetPacketId.Timestamp
            && RakNetTimestampEnvelope.TryRead(Data.Span, DataBitLength, out envelope);
    }
}
