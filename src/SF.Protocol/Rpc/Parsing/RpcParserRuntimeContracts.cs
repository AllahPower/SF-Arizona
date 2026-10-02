namespace SFSharp.Protocol.Rpc.Parsing;

public interface IIncomingRpcParser
{
    SampRpcId SampRpcId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingRpcArgs args, out RpcParseResult result);
}

public interface IOutgoingRpcParser
{
    SampRpcId SampRpcId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingRpcArgs args, out RpcParseResult result);
}
