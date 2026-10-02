using System.Collections.Concurrent;

namespace SFSharp.Runtime.Networking.RakNet;

// Manages thread-safe network dispatch pipeline: hook thread -> ConcurrentQueue -> main thread batched dispatch
// Uses a single unified queue to preserve global capture order across all event types.
public sealed class NetworkDispatcher(MainThreadDispatcher mainThread)
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
        OutgoingAZVoiceControl,
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

    private readonly RpcHandlerManager _incomingRpcHandlers = new();
    private readonly OutgoingRpcManager _outgoingRpcHandlers = new();
    private readonly IncomingPacketManager _incomingPacketHandlers = new();
    private readonly OutgoingPacketManager _outgoingPacketHandlers = new();
    private readonly IncomingAZVoiceControlManager _incomingAZVoiceControlHandlers = new();
    private readonly IncomingAZVoiceDataManager _incomingAZVoiceDataHandlers = new();
    private readonly OutgoingAZVoiceControlManager _outgoingAZVoiceControlHandlers = new();

    public RpcHandlerManager IncomingRpcHandlers => _incomingRpcHandlers;
    public OutgoingRpcManager OutgoingRpcHandlers => _outgoingRpcHandlers;
    public IncomingPacketManager IncomingPacketHandlers => _incomingPacketHandlers;
    public OutgoingPacketManager OutgoingPacketHandlers => _outgoingPacketHandlers;
    public IncomingAZVoiceControlManager IncomingAZVoiceControlHandlers => _incomingAZVoiceControlHandlers;
    public IncomingAZVoiceDataManager IncomingAZVoiceDataHandlers => _incomingAZVoiceDataHandlers;
    public OutgoingAZVoiceControlManager OutgoingAZVoiceControlHandlers => _outgoingAZVoiceControlHandlers;

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

    public void EnqueueOutgoingAZVoiceControl(int subId, byte[] data, int dataBitLength)
    {
        _pendingEvents.Enqueue(new NetworkEvent(EventKind.OutgoingAZVoiceControl, subId, data, dataBitLength));
        ScheduleDispatch();
    }

    private void ScheduleDispatch()
    {
        if (Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) == 0)
        {
            mainThread.Post(ProcessBatch);
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
                case EventKind.OutgoingAZVoiceControl:
                    _outgoingAZVoiceControlHandlers.Dispatch(ev.Id, ev.Data, ev.BitParam1);
                    break;
            }
            processed++;
        }

        if (_pendingEvents.IsEmpty)
        {
            Interlocked.Exchange(ref _dispatchScheduled, 0);
            if (!_pendingEvents.IsEmpty && Interlocked.CompareExchange(ref _dispatchScheduled, 1, 0) == 0)
            {
                mainThread.Post(ProcessBatch);
            }

            return;
        }

        mainThread.Post(ProcessBatch);
    }
}
