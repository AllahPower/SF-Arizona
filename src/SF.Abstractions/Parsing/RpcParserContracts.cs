namespace SFSharp.Abstractions.Parsing;

public interface IParsedRpc
{
    ERpcId ERpcId { get; }
    string Name { get; }
    string? Detail { get; }
}

public interface IParsedIncomingRpc : IParsedRpc
{
}

public interface IParsedOutgoingRpc : IParsedRpc
{
}
