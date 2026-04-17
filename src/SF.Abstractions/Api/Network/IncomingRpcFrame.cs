namespace SFSharp.Abstractions.Network;

/// <summary>Copied incoming RPC frame detached from the original game memory buffer.</summary>
public readonly record struct IncomingRpcFrame(int RpcId, ReadOnlyMemory<byte> Data, int DataBitOffset, int DataBitLength);
