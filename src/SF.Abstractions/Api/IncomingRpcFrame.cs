namespace SFSharp;

/// <summary>Copied incoming RPC frame detached from the original game memory buffer.</summary>
public readonly record struct IncomingRpcFrame(int RpcId, byte[] Data, int DataBitOffset, int DataBitLength);
