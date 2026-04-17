namespace SFSharp;

/// <summary>Copied incoming packet frame detached from the original game memory buffer.</summary>
public readonly record struct IncomingPacketFrame(int PacketId, ReadOnlyMemory<byte> Data, int DataBitLength);
