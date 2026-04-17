using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Networking;

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
