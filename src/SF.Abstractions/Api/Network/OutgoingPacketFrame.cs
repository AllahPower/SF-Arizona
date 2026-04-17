namespace SFSharp.Abstractions.Network;

/// <summary>Copied outgoing packet frame detached from the original game memory buffer.</summary>
public readonly record struct OutgoingPacketFrame(int PacketId, ReadOnlyMemory<byte> Data, int DataBitLength);
