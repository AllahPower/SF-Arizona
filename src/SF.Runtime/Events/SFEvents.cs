namespace SFSharp.Runtime.Events;

public sealed partial class SFEvents : ISFEvents
{
    private readonly SFEventFactory _factory;

    internal SFEvents(SFEventFactory factory)
    {
        _factory = factory;
    }

    private readonly Lock _initSync = new();
    private readonly Dictionary<Type, object> _incomingRpcChannels = new();
    private readonly Dictionary<Type, object> _outgoingRpcChannels = new();
    private readonly Dictionary<Type, object> _incomingPacketChannels = new();
    private readonly Dictionary<Type, object> _outgoingPacketChannels = new();

    private SFEventChannel<TRpc> GetOrCreateIncomingRpcChannel<TRpc>()
    {
        lock (_initSync)
        {
            if (_incomingRpcChannels.TryGetValue(typeof(TRpc), out object? channel))
            {
                return (SFEventChannel<TRpc>)channel;
            }

            SFEventChannel<TRpc> created = _factory.FromParsedIncomingRpc<TRpc>();
            _incomingRpcChannels[typeof(TRpc)] = created;
            return created;
        }
    }

    private SFEventChannel<TRpc> GetOrCreateOutgoingRpcChannel<TRpc>()
    {
        lock (_initSync)
        {
            if (_outgoingRpcChannels.TryGetValue(typeof(TRpc), out object? channel))
            {
                return (SFEventChannel<TRpc>)channel;
            }

            SFEventChannel<TRpc> created = _factory.FromParsedOutgoingRpc<TRpc>();
            _outgoingRpcChannels[typeof(TRpc)] = created;
            return created;
        }
    }

    private SFEventChannel<TPacket> GetOrCreateIncomingPacketChannel<TPacket>()
    {
        lock (_initSync)
        {
            if (_incomingPacketChannels.TryGetValue(typeof(TPacket), out object? channel))
            {
                return (SFEventChannel<TPacket>)channel;
            }

            SFEventChannel<TPacket> created = _factory.FromParsedIncomingPacket<TPacket>();
            _incomingPacketChannels[typeof(TPacket)] = created;
            return created;
        }
    }

    private SFEventChannel<TPacket> GetOrCreateOutgoingPacketChannel<TPacket>()
    {
        lock (_initSync)
        {
            if (_outgoingPacketChannels.TryGetValue(typeof(TPacket), out object? channel))
            {
                return (SFEventChannel<TPacket>)channel;
            }

            SFEventChannel<TPacket> created = _factory.FromParsedOutgoingPacket<TPacket>();
            _outgoingPacketChannels[typeof(TPacket)] = created;
            return created;
        }
    }
}
