using System.Collections.Concurrent;

namespace SFSharp.Runtime.Networking;

// Manages thread-safe network dispatch pipeline: hook thread -> ConcurrentQueue -> main thread batched dispatch
// Uses a single unified queue to preserve global capture order across all event types.
internal sealed class NetworkDispatcher
{
    private const int MaxDispatchPerTick = 24;

    private enum EventKind : byte
    {
        IncomingRpc,
        OutgoingRpc,
        IncomingPacket,
        OutgoingPacket,
        IncomingAZVoiceControl,
        IncomingAZVoiceData,
    }

    private readonly struct NetworkEvent
    {
        public readonly EventKind Kind;
        public readonly int Id;
        public readonly byte[] Data;
        public readonly int BitParam1; // PayloadBitOffset for incoming RPC, DataBitLength for others
        public readonly int BitParam2; // PayloadBitLength for incoming RPC, unused for others

        public NetworkEvent(EventKind kind, int id, byte[] data, int bitParam1, int bitParam2 = 0)
        {
            Kind = kind;
            Id = id;
            Data = data;
            BitParam1 = bitParam1;
            BitParam2 = bitParam2;
        }
    }

    private readonly ConcurrentQueue<NetworkEvent> _pendingEvents = new();
    private int _dispatchScheduled;

    private RpcHandlerManager _incomingRpcHandlers = new();
    private OutgoingRpcManager _outgoingRpcHandlers = new();
    private IncomingPacketManager _incomingPacketHandlers = new();
    private OutgoingPacketManager _outgoingPacketHandlers = new();
    private IncomingAZVoiceControlManager _incomingAZVoiceControlHandlers = new();
    private IncomingAZVoiceDataManager _incomingAZVoiceDataHandlers = new();

    public RpcHandlerManager IncomingRpcHandlers => _incomingRpcHandlers;
    public OutgoingRpcManager OutgoingRpcHandlers => _outgoingRpcHandlers;
    public IncomingPacketManager IncomingPacketHandlers => _incomingPacketHandlers;
    public OutgoingPacketManager OutgoingPacketHandlers => _outgoingPacketHandlers;
    public IncomingAZVoiceControlManager IncomingAZVoiceControlHandlers => _incomingAZVoiceControlHandlers;
    public IncomingAZVoiceDataManager IncomingAZVoiceDataHandlers => _incomingAZVoiceDataHandlers;

    public void Reset()
    {
        _incomingRpcHandlers = new RpcHandlerManager();
        _outgoingRpcHandlers = new OutgoingRpcManager();
        _incomingPacketHandlers = new IncomingPacketManager();
        _outgoingPacketHandlers = new OutgoingPacketManager();
        _incomingAZVoiceControlHandlers = new IncomingAZVoiceControlManager();
        _incomingAZVoiceDataHandlers = new IncomingAZVoiceDataManager();
    }

    public void EnqueueIncomingRpc(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.IncomingRpc, rpcId, packet, payloadBitOffset, payloadBitLength));
        ScheduleDispatch();
    }

    public void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.OutgoingRpc, rpcId, packet, dataBitLength));
        ScheduleDispatch();
    }

    public void EnqueueIncomingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.IncomingPacket, packetId, data, dataBitLength));
        ScheduleDispatch();
    }

    public void EnqueueOutgoingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.OutgoingPacket, packetId, data, dataBitLength));
        ScheduleDispatch();
    }

    public void EnqueueIncomingAZVoiceControl(int subId, byte[] data, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.IncomingAZVoiceControl, subId, data, dataBitLength));
        ScheduleDispatch();
    }

    public void EnqueueIncomingAZVoiceData(byte[] data, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.IncomingAZVoiceData, 0, data, dataBitLength));
        ScheduleDispatch();
    }

    private void ScheduleDispatch()
    {
        if (Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessBatch);
        }
    }

    private void ProcessBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingEvents.TryDequeue(out var ev))
        {
            switch (ev.Kind)
            {
                case EventKind.IncomingRpc:
                    _incomingRpcHandlers.DispatchIncoming(ev.Id, ev.Data, ev.BitParam1, ev.BitParam2);
                    break;
                case EventKind.OutgoingRpc:
                    _outgoingRpcHandlers.Dispatch(ev.Id, ev.Data, ev.BitParam1);
                    break;
                case EventKind.IncomingPacket:
                    _incomingPacketHandlers.Dispatch(ev.Id, ev.Data, ev.BitParam1);
                    break;
                case EventKind.OutgoingPacket:
                    _outgoingPacketHandlers.Dispatch(ev.Id, ev.Data, ev.BitParam1);
                    break;
                case EventKind.IncomingAZVoiceControl:
                    _incomingAZVoiceControlHandlers.Dispatch(ev.Id, ev.Data, ev.BitParam1);
                    break;
                case EventKind.IncomingAZVoiceData:
                    _incomingAZVoiceDataHandlers.Dispatch(ev.Data, ev.BitParam1);
                    break;
            }
            processed++;
        }

        if (_pendingEvents.IsEmpty)
        {
            Interlocked.Exchange(ref _dispatchScheduled, 0);
            if (!_pendingEvents.IsEmpty && Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) == 0)
            {
                SFBootstrap.PostToMainThread(ProcessBatch);
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessBatch);
    }
}
