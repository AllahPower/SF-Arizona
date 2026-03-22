using System.Collections.Concurrent;

namespace SFSharp;

// Manages thread-safe network dispatch pipeline: hook thread -> ConcurrentQueue -> main thread batched dispatch
internal sealed class NetworkDispatcher
{
    private const int MaxDispatchPerTick = 24;

    // RPC queues
    private readonly ConcurrentQueue<(int ERpcId, byte[] Packet, int PayloadBitOffset, int PayloadBitLength)> _pendingIncomingRpc = new();
    private readonly ConcurrentQueue<(int ERpcId, byte[] Packet, int DataBitLength)> _pendingOutgoingRpc = new();
    private int _incomingRpcScheduled;
    private int _outgoingRpcScheduled;

    // Packet queues
    private readonly ConcurrentQueue<(int EPacketId, byte[] Data, int DataBitLength)> _pendingIncomingPacket = new();
    private readonly ConcurrentQueue<(int EPacketId, byte[] Data, int DataBitLength)> _pendingOutgoingPacket = new();
    private int _incomingPacketScheduled;
    private int _outgoingPacketScheduled;

    private RpcHandlerManager _incomingRpcHandlers = new();
    private OutgoingRpcManager _outgoingRpcHandlers = new();
    private IncomingPacketManager _incomingPacketHandlers = new();
    private OutgoingPacketManager _outgoingPacketHandlers = new();

    public RpcHandlerManager IncomingRpcHandlers => _incomingRpcHandlers;
    public OutgoingRpcManager OutgoingRpcHandlers => _outgoingRpcHandlers;
    public IncomingPacketManager IncomingPacketHandlers => _incomingPacketHandlers;
    public OutgoingPacketManager OutgoingPacketHandlers => _outgoingPacketHandlers;

    public void Reset()
    {
        _incomingRpcHandlers = new RpcHandlerManager();
        _outgoingRpcHandlers = new OutgoingRpcManager();
        _incomingPacketHandlers = new IncomingPacketManager();
        _outgoingPacketHandlers = new OutgoingPacketManager();
    }

    // - Incoming RPC (server -> client) -

    public void EnqueueIncomingRpc(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        _pendingIncomingRpc.Enqueue((rpcId, packet, payloadBitOffset, payloadBitLength));

        if (Interlocked.CompareExchange(ref _incomingRpcScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessIncomingRpcBatch);
        }
    }

    private void ProcessIncomingRpcBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingIncomingRpc.TryDequeue(out var item))
        {
            _incomingRpcHandlers.DispatchIncoming(item.ERpcId, item.Packet, item.PayloadBitOffset, item.PayloadBitLength);
            processed++;
        }

        if (_pendingIncomingRpc.IsEmpty)
        {
            Interlocked.Exchange(ref _incomingRpcScheduled, 0);
            if (!_pendingIncomingRpc.IsEmpty && Interlocked.CompareExchange(ref _incomingRpcScheduled, 1, 0) == 0)
            {
                SFBootstrap.PostToMainThread(ProcessIncomingRpcBatch);
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessIncomingRpcBatch);
    }

    // - Outgoing RPC (client -> server) -

    public void EnqueueOutgoingRpc(int rpcId, byte[] packet, int dataBitLength)
    {
        _pendingOutgoingRpc.Enqueue((rpcId, packet, dataBitLength));

        if (Interlocked.CompareExchange(ref _outgoingRpcScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessOutgoingRpcBatch);
        }
    }

    private void ProcessOutgoingRpcBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingOutgoingRpc.TryDequeue(out var item))
        {
            _outgoingRpcHandlers.Dispatch(item.ERpcId, item.Packet, item.DataBitLength);
            processed++;
        }

        if (_pendingOutgoingRpc.IsEmpty)
        {
            Interlocked.Exchange(ref _outgoingRpcScheduled, 0);
            if (!_pendingOutgoingRpc.IsEmpty && Interlocked.CompareExchange(ref _outgoingRpcScheduled, 1, 0) == 0)
            {
                SFBootstrap.PostToMainThread(ProcessOutgoingRpcBatch);
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessOutgoingRpcBatch);
    }

    // - Incoming Packet (server -> client) -

    public void EnqueueIncomingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _pendingIncomingPacket.Enqueue((packetId, data, dataBitLength));

        if (Interlocked.CompareExchange(ref _incomingPacketScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessIncomingPacketBatch);
        }
    }

    private void ProcessIncomingPacketBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingIncomingPacket.TryDequeue(out var item))
        {
            _incomingPacketHandlers.Dispatch(item.EPacketId, item.Data, item.DataBitLength);
            processed++;
        }

        if (_pendingIncomingPacket.IsEmpty)
        {
            Interlocked.Exchange(ref _incomingPacketScheduled, 0);
            if (!_pendingIncomingPacket.IsEmpty && Interlocked.CompareExchange(ref _incomingPacketScheduled, 1, 0) == 0)
            {
                SFBootstrap.PostToMainThread(ProcessIncomingPacketBatch);
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessIncomingPacketBatch);
    }

    // - Outgoing Packet (client -> server) -

    public void EnqueueOutgoingPacket(int packetId, byte[] data, int dataBitLength)
    {
        _pendingOutgoingPacket.Enqueue((packetId, data, dataBitLength));

        if (Interlocked.CompareExchange(ref _outgoingPacketScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessOutgoingPacketBatch);
        }
    }

    private void ProcessOutgoingPacketBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingOutgoingPacket.TryDequeue(out var item))
        {
            _outgoingPacketHandlers.Dispatch(item.EPacketId, item.Data, item.DataBitLength);
            processed++;
        }

        if (_pendingOutgoingPacket.IsEmpty)
        {
            Interlocked.Exchange(ref _outgoingPacketScheduled, 0);
            if (!_pendingOutgoingPacket.IsEmpty && Interlocked.CompareExchange(ref _outgoingPacketScheduled, 1, 0) == 0)
            {
                SFBootstrap.PostToMainThread(ProcessOutgoingPacketBatch);
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessOutgoingPacketBatch);
    }
}
