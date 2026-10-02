namespace SFSharp.Abstractions.Parsing;

public interface IParsedRpc
{
    SampRpcId SampRpcId { get; }
    string Name { get; }
    string? Detail { get; }
}

public interface IParsedIncomingRpc : IParsedRpc
{
}

public interface IParsedOutgoingRpc : IParsedRpc
{
}
