using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Network.RakNet.Rpc;

public sealed record IncomingRpc<TPayload>(ERpcId ERpcId, string Name, TPayload Payload) : IParsedIncomingRpc
{
    public string? Detail => Payload?.ToString();
}

public sealed record OutgoingRpc<TPayload>(ERpcId ERpcId, string Name, TPayload Payload) : IParsedOutgoingRpc
{
    public string? Detail => Payload?.ToString();
}

public sealed record IncomingUnknownRpc(ERpcId ERpcId, int PayloadBitLength) : IParsedIncomingRpc
{
    public string Name => "Unknown";
    public string? Detail => $"bits={PayloadBitLength}";
}

public sealed record OutgoingUnknownRpc(ERpcId ERpcId, int DataBitLength) : IParsedOutgoingRpc
{
    public string Name => "Unknown";
    public string? Detail => $"bits={DataBitLength}";
}
