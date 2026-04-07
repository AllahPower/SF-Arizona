namespace SFSharp;

public static class ModuleContextEventExtensions
{
    public static IDisposable RegisterEvent<TEvent>(
        this ModuleContext context,
        Action<TEvent> handler,
        Func<Action<TEvent>, IDisposable> subscribe)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(subscribe);

        return context.RegisterDisposable(subscribe(handler));
    }

    public static IDisposable RegisterIncomingRpc<TRpc>(
        this ModuleContext context,
        Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnIncomingRpc(handler));
    }

    public static IDisposable RegisterOutgoingRpc<TRpc>(
        this ModuleContext context,
        Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnOutgoingRpc(handler));
    }

    public static IDisposable RegisterIncomingPacket<TPacket>(
        this ModuleContext context,
        Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnIncomingPacket(handler));
    }

    public static IDisposable RegisterOutgoingPacket<TPacket>(
        this ModuleContext context,
        Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnOutgoingPacket(handler));
    }

    // - Filter extensions (synchronous, run on hook thread) -

    public static IDisposable RegisterOutgoingPacketFilter(
        this ModuleContext context,
        SFSharp.Interop.RakNet.Packets.Enum.EPacketId packetId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Packets.RegisterOutgoingFilter(packetId, filter));
    }

    /// <remarks>
    /// Arizona Packet 220 traffic cannot be cancelled via incoming filters because vorbisFile.dll
    /// replaces the entire RakClientInterface vtable — those packets are intercepted and processed
    /// before reaching SAMP's RakClient Receive, so our hook never sees them.
    /// </remarks>
    public static IDisposable RegisterIncomingPacketFilter(
        this ModuleContext context,
        SFSharp.Interop.RakNet.Packets.Enum.EPacketId packetId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Packets.RegisterIncomingFilter(packetId, filter));
    }

    public static IDisposable RegisterOutgoingRpcFilter(
        this ModuleContext context,
        SFSharp.Interop.RakNet.Packets.Enum.ERpcId rpcId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Rpc.RegisterOutgoingFilter(rpcId, filter));
    }

    public static IDisposable RegisterIncomingRpcFilter(
        this ModuleContext context,
        SFSharp.Interop.RakNet.Packets.Enum.ERpcId rpcId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Rpc.RegisterIncomingFilter(rpcId, filter));
    }
}
