namespace SFSharp;

public sealed partial class SFEvents
{
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

            SFEventChannel<TRpc> created = SFEventFactory.FromParsedIncomingRpc<TRpc>();
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

            SFEventChannel<TRpc> created = SFEventFactory.FromParsedOutgoingRpc<TRpc>();
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

            SFEventChannel<TPacket> created = SFEventFactory.FromParsedIncomingPacket<TPacket>();
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

            SFEventChannel<TPacket> created = SFEventFactory.FromParsedOutgoingPacket<TPacket>();
            _outgoingPacketChannels[typeof(TPacket)] = created;
            return created;
        }
    }
}
