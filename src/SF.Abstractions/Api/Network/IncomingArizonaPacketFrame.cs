namespace SFSharp.Abstractions.Network;

/// <summary>Copied incoming Arizona packet frame detached from the original game memory buffer.</summary>
public readonly record struct IncomingArizonaPacketFrame(int PacketId, int SubId, ReadOnlyMemory<byte> Data, int PayloadBitOffset, int PayloadBitLength);
