namespace SFSharp;

/// <summary>Copied outgoing RPC frame detached from the original game memory buffer.</summary>
public readonly record struct OutgoingRpcFrame(int RpcId, ReadOnlyMemory<byte> Data, int DataBitLength);
