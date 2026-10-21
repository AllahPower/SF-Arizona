namespace SFSharp.Abstractions.Network;

/// <summary>Context supplied by an optional traffic source, separate from normal RakNet receive/send.</summary>
public sealed record SFPacketObservation(string Source, bool Incoming, int PacketId, ReadOnlyMemory<byte> Data,
    int DataBitLength, int PayloadBitOffset, int PayloadBitLength, int? SubId = null, bool Reconstructed = false);

public interface ISFPacketSource : IDisposable
{
    /// <summary>Copies an observation; never sends or injects it into the game. Thread-safe.</summary>
    void Publish(bool incoming, int packetId, ReadOnlySpan<byte> data, int dataBitLength,
        int payloadBitOffset, int payloadBitLength, int? subId = null, bool reconstructed = false);
}
