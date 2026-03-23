using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

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

public interface IIncomingRpcParser
{
    ERpcId ERpcId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingRpcArgs args, out RpcParseResult result);
}

public interface IOutgoingRpcParser
{
    ERpcId ERpcId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingRpcArgs args, out RpcParseResult result);
}
