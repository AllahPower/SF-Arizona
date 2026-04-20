namespace SFSharp.Runtime.Network.RakNet.Rpc;

public static partial class RpcParserCatalog
{
    public static RpcParserRegistry CreateDefaultRegistry()
    {
        RpcParserRegistry registry = new();
        RegisterIncoming(registry);
        RegisterOutgoing(registry);
        return registry;
    }

    private static void RegisterIncoming<TPayload>(RpcParserRegistry registry, ERpcId rpcId, Func<IncomingRpcArgs, TPayload> parser, string? name = null)
    {
        string rpcName = name ?? rpcId.ToString();
        registry.Register(new DelegateIncomingRpcParser<IncomingRpc<TPayload>>(
            rpcId,
            args => new IncomingRpc<TPayload>(rpcId, rpcName, parser(args)),
            name: rpcName));
    }

    private static void RegisterOutgoing<TPayload>(RpcParserRegistry registry, ERpcId rpcId, Func<OutgoingRpcArgs, TPayload> parser, string? name = null)
    {
        string rpcName = name ?? rpcId.ToString();
        registry.Register(new DelegateOutgoingRpcParser<OutgoingRpc<TPayload>>(
            rpcId,
            args => new OutgoingRpc<TPayload>(rpcId, rpcName, parser(args)),
            name: rpcName));
    }
}
