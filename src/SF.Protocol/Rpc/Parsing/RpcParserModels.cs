namespace SFSharp.Protocol.Rpc.Parsing;

public sealed record IncomingRpc<TPayload>(SampRpcId SampRpcId, string Name, TPayload Payload) : IParsedIncomingRpc
{
    public string? Detail => Payload?.ToString();
}

public sealed record OutgoingRpc<TPayload>(SampRpcId SampRpcId, string Name, TPayload Payload) : IParsedOutgoingRpc
{
    public string? Detail => Payload?.ToString();
}

public sealed record IncomingUnknownRpc(SampRpcId SampRpcId, int PayloadBitLength) : IParsedIncomingRpc
{
    public string Name => "Unknown";
    public string? Detail => $"bits={PayloadBitLength}";
}

public sealed record OutgoingUnknownRpc(SampRpcId SampRpcId, int DataBitLength) : IParsedOutgoingRpc
{
    public string Name => "Unknown";
    public string? Detail => $"bits={DataBitLength}";
}
