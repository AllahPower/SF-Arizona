using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Events;

internal static class SFEventFactory
{
    public static SFEventChannel<TEvent> FromIncomingRpc<TEvent, TRpc>(
        ERpcId rpcId,
        Func<IncomingRpcArgs, TRpc> parser,
        Func<TRpc, TEvent> map,
        string name)
    {
        return new SFEventChannel<TEvent>(publish =>
            SF.Rpc.Bind(rpcId, parser, (rpc, _) => publish(map(rpc)), name: name));
    }

    public static SFEventChannel<TEvent> FromOutgoingRpc<TEvent>(
        ERpcId rpcId,
        Func<OutgoingRpcArgs, TEvent> map)
    {
        return new SFEventChannel<TEvent>(publish =>
            SF.Rpc.SubscribeOutgoing(rpcId, args => publish(map(args))));
    }

    public static SFEventChannel<TEvent> FromOutgoingRpc<TEvent, TRpc>(
        ERpcId rpcId,
        Func<OutgoingRpcArgs, TRpc> parser,
        Func<TRpc, TEvent> map)
    {
        return new SFEventChannel<TEvent>(publish =>
            SF.Rpc.SubscribeOutgoing(rpcId, args => publish(map(parser(args)))));
    }

    public static SFEventChannel<TRpc> FromParsedIncomingRpc<TRpc>()
    {
        return new SFEventChannel<TRpc>(publish => SF.RpcParsers.BindIncoming<TRpc>(publish));
    }

    public static SFEventChannel<TRpc> FromParsedOutgoingRpc<TRpc>()
    {
        return new SFEventChannel<TRpc>(publish => SF.RpcParsers.BindOutgoing<TRpc>(publish));
    }

    public static SFEventChannel<TPacket> FromParsedIncomingPacket<TPacket>()
    {
        return new SFEventChannel<TPacket>(publish => SF.PacketParsers.BindIncoming<TPacket>(publish));
    }

    public static SFEventChannel<TPacket> FromParsedOutgoingPacket<TPacket>()
    {
        return new SFEventChannel<TPacket>(publish => SF.PacketParsers.BindOutgoing<TPacket>(publish));
    }
}
