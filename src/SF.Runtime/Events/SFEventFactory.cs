namespace SFSharp.Runtime.Events;

internal sealed class SFEventFactory(SFRpc rpc, SFRpcParsers rpcParsers, SFPacketParsers packetParsers)
{
    public SFEventChannel<TEvent> FromIncomingRpc<TEvent, TRpc>(
        SampRpcId rpcId,
        Func<IncomingRpcArgs, TRpc> parser,
        Func<TRpc, TEvent> map,
        string name)
    {
        return new SFEventChannel<TEvent>(publish =>
            rpc.Bind(rpcId, parser, (rpc, _) => publish(map(rpc)), name: name));
    }

    public SFEventChannel<TEvent> FromOutgoingRpc<TEvent>(
        SampRpcId rpcId,
        Func<OutgoingRpcArgs, TEvent> map)
    {
        return new SFEventChannel<TEvent>(publish =>
            rpc.SubscribeOutgoing(rpcId, args => publish(map(args))));
    }

    public SFEventChannel<TEvent> FromOutgoingRpc<TEvent, TRpc>(
        SampRpcId rpcId,
        Func<OutgoingRpcArgs, TRpc> parser,
        Func<TRpc, TEvent> map)
    {
        return new SFEventChannel<TEvent>(publish =>
            rpc.SubscribeOutgoing(rpcId, args => publish(map(parser(args)))));
    }

    public SFEventChannel<TRpc> FromParsedIncomingRpc<TRpc>()
    {
        return new SFEventChannel<TRpc>(publish => rpcParsers.BindIncoming<TRpc>(publish));
    }

    public SFEventChannel<TRpc> FromParsedOutgoingRpc<TRpc>()
    {
        return new SFEventChannel<TRpc>(publish => rpcParsers.BindOutgoing<TRpc>(publish));
    }

    public SFEventChannel<TPacket> FromParsedIncomingPacket<TPacket>()
    {
        return new SFEventChannel<TPacket>(publish => packetParsers.BindIncoming<TPacket>(publish));
    }

    public SFEventChannel<TPacket> FromParsedOutgoingPacket<TPacket>()
    {
        return new SFEventChannel<TPacket>(publish => packetParsers.BindOutgoing<TPacket>(publish));
    }
}
