namespace SFSharp;

/// <summary>Copied outgoing Arizona packet frame detached from the original game memory buffer.</summary>
public readonly record struct OutgoingArizonaPacketFrame(int PacketId, int SubId, byte[] Data, int PayloadBitOffset, int PayloadBitLength);
