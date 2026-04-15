namespace SFSharp;

/// <summary>Copied outgoing packet frame detached from the original game memory buffer.</summary>
public readonly record struct OutgoingPacketFrame(int PacketId, byte[] Data, int DataBitLength);
