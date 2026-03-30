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
}
