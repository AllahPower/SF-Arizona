using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Modules;

/// <summary>
/// Convenience wrappers around <see cref="SFEvents"/>, <see cref="SFPackets"/> and
/// <see cref="SFRpc"/> that tie the subscription lifetime to a <see cref="ModuleContext"/>.
/// Every call registers the returned <see cref="IDisposable"/> with the context so the subscription
/// is released automatically when the module stops.
/// </summary>
public static class ModuleContextEventExtensions
{
    /// <summary>
    /// Generic subscription helper for custom events. Calls <paramref name="subscribe"/> with
    /// <paramref name="handler"/> and registers the resulting disposable with
    /// <paramref name="context"/>. Useful when wrapping subsystem events that do not yet have a
    /// dedicated extension method here.
    /// </summary>
    /// <typeparam name="TEvent">Event payload type passed to the handler.</typeparam>
    /// <param name="context">Module context that owns the subscription.</param>
    /// <param name="handler">Callback invoked for every event instance.</param>
    /// <param name="subscribe">Factory that produces the underlying subscription.</param>
    /// <returns>The subscription, already tracked by <paramref name="context"/>.</returns>
    public static IDisposable RegisterEvent<TEvent>(
        this IModuleContext context,
        Action<TEvent> handler,
        Func<Action<TEvent>, IDisposable> subscribe)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);
        ArgumentNullException.ThrowIfNull(subscribe);

        return context.RegisterDisposable(subscribe(handler));
    }

    /// <summary>
    /// Subscribes to parsed incoming RPCs of <typeparamref name="TRpc"/>. Dispatched on the main
    /// thread after the inbound pipeline finishes parsing. See <see cref="SFEvents"/>.
    /// </summary>
    /// <typeparam name="TRpc">Parsed RPC model type.</typeparam>
    /// <param name="context">Module context that owns the subscription.</param>
    /// <param name="handler">Synchronous handler invoked on the main thread.</param>
    public static IDisposable RegisterIncomingRpc<TRpc>(
        this IModuleContext context,
        Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnIncomingRpc(handler));
    }

    /// <summary>
    /// Subscribes to parsed outgoing RPCs of <typeparamref name="TRpc"/>. Dispatched on the main
    /// thread after the outbound pipeline finishes parsing. See <see cref="SFEvents"/>.
    /// </summary>
    /// <typeparam name="TRpc">Parsed RPC model type.</typeparam>
    /// <param name="context">Module context that owns the subscription.</param>
    /// <param name="handler">Synchronous handler invoked on the main thread.</param>
    public static IDisposable RegisterOutgoingRpc<TRpc>(
        this IModuleContext context,
        Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnOutgoingRpc(handler));
    }

    /// <summary>
    /// Subscribes to parsed incoming packets of <typeparamref name="TPacket"/>.
    /// See <see cref="SFEvents"/>.
    /// </summary>
    /// <typeparam name="TPacket">Parsed packet model type.</typeparam>
    /// <param name="context">Module context that owns the subscription.</param>
    /// <param name="handler">Synchronous handler invoked on the main thread.</param>
    public static IDisposable RegisterIncomingPacket<TPacket>(
        this IModuleContext context,
        Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnIncomingPacket(handler));
    }

    /// <summary>
    /// Subscribes to parsed outgoing packets of <typeparamref name="TPacket"/>.
    /// See <see cref="SFEvents"/>.
    /// </summary>
    /// <typeparam name="TPacket">Parsed packet model type.</typeparam>
    /// <param name="context">Module context that owns the subscription.</param>
    /// <param name="handler">Synchronous handler invoked on the main thread.</param>
    public static IDisposable RegisterOutgoingPacket<TPacket>(
        this IModuleContext context,
        Action<TPacket> handler)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(handler);

        return context.RegisterDisposable(SF.Events.OnOutgoingPacket(handler));
    }

    /// <summary>
    /// Registers a synchronous filter for outgoing packets with <paramref name="packetId"/>.
    /// The filter runs on the RakNet hook thread, not on the main thread. Return <see langword="false"/>
    /// to drop the packet, <see langword="true"/> to forward it. See <see cref="SFPackets"/>.
    /// </summary>
    /// <param name="context">Module context that owns the filter registration.</param>
    /// <param name="packetId">Packet id to match.</param>
    /// <param name="filter">Predicate receiving the raw pointer and length of the packet payload.</param>
    public static IDisposable RegisterOutgoingPacketFilter(
        this IModuleContext context,
        EPacketId packetId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Packets.RegisterOutgoingFilter(packetId, filter));
    }

    /// <summary>
    /// Registers a synchronous filter for incoming packets with <paramref name="packetId"/>.
    /// Same thread and semantics as <see cref="RegisterOutgoingPacketFilter(ModuleContext, EPacketId, Func{nint, int, bool})"/>.
    /// </summary>
    /// <param name="context">Module context that owns the filter registration.</param>
    /// <param name="packetId">Packet id to match.</param>
    /// <param name="filter">Predicate receiving the raw pointer and length of the packet payload.</param>
    /// <remarks>
    /// Arizona Packet 220 traffic cannot be cancelled via incoming filters because <c>vorbisFile.dll</c>
    /// replaces the entire <c>RakClientInterface</c> vtable. Those packets are intercepted and
    /// processed before reaching SAMP's <c>RakClient::Receive</c>, so our hook never sees them.
    /// </remarks>
    public static IDisposable RegisterIncomingPacketFilter(
        this IModuleContext context,
        EPacketId packetId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Packets.RegisterIncomingFilter(packetId, filter));
    }

    /// <summary>
    /// Registers a synchronous filter for outgoing RPCs with <paramref name="rpcId"/>. Runs on the
    /// RakNet hook thread. See <see cref="SFRpc"/>.
    /// </summary>
    /// <param name="context">Module context that owns the filter registration.</param>
    /// <param name="rpcId">RPC id to match.</param>
    /// <param name="filter">Predicate receiving the raw pointer and length of the RPC payload.</param>
    public static IDisposable RegisterOutgoingRpcFilter(
        this IModuleContext context,
        ERpcId rpcId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Rpc.RegisterOutgoingFilter(rpcId, filter));
    }

    /// <summary>
    /// Registers a synchronous filter for incoming RPCs with <paramref name="rpcId"/>. Runs on the
    /// RakNet hook thread. See <see cref="SFRpc"/>.
    /// </summary>
    /// <param name="context">Module context that owns the filter registration.</param>
    /// <param name="rpcId">RPC id to match.</param>
    /// <param name="filter">Predicate receiving the raw pointer and length of the RPC payload.</param>
    public static IDisposable RegisterIncomingRpcFilter(
        this IModuleContext context,
        ERpcId rpcId,
        Func<nint, int, bool> filter)
    {
        return context.RegisterDisposable(SF.Rpc.RegisterIncomingFilter(rpcId, filter));
    }
}
