using System.Collections.Concurrent;

namespace SFSharp;

// Manages thread-safe RPC dispatch pipeline: hook thread -> ConcurrentQueue -> main thread batched dispatch
internal sealed class RpcDispatcher
{
    private const int MaxDispatchPerTick = 24;

    private readonly ConcurrentQueue<(int RpcId, byte[] Packet, int PayloadBitOffset, int PayloadBitLength)> _pendingIncoming = new();
    private readonly ConcurrentQueue<(int RpcId, byte[] Packet, int DataBitLength)> _pendingOutgoing = new();
    private int _incomingScheduled;
    private int _outgoingScheduled;

    private RpcHandlerManager _incomingHandlers = new();
    private OutgoingRpcManager _outgoingHandlers = new();

    public RpcHandlerManager IncomingHandlers => _incomingHandlers;
    public OutgoingRpcManager OutgoingHandlers => _outgoingHandlers;

    public void Reset()
    {
        _incomingHandlers = new RpcHandlerManager();
        _outgoingHandlers = new OutgoingRpcManager();
    }

    // - Incoming (server -> client) -

    public void EnqueueIncoming(int rpcId, byte[] packet, int payloadBitOffset, int payloadBitLength)
    {
        _pendingIncoming.Enqueue((rpcId, packet, payloadBitOffset, payloadBitLength));

        if (Interlocked.CompareExchange(ref _incomingScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessIncomingBatch);
        }
    }

    private void ProcessIncomingBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingIncoming.TryDequeue(out var item))
        {
            _incomingHandlers.DispatchIncoming(item.RpcId, item.Packet, item.PayloadBitOffset, item.PayloadBitLength);
            processed++;
        }

        if (_pendingIncoming.IsEmpty)
        {
            Interlocked.Exchange(ref _incomingScheduled, 0);
            if (!_pendingIncoming.IsEmpty)
            {
                if (Interlocked.CompareExchange(ref _incomingScheduled, 1, 0) == 0)
                {
                    SFBootstrap.PostToMainThread(ProcessIncomingBatch);
                }
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessIncomingBatch);
    }

    // - Outgoing (client -> server) -

    public void EnqueueOutgoing(int rpcId, byte[] packet, int dataBitLength)
    {
        _pendingOutgoing.Enqueue((rpcId, packet, dataBitLength));

        if (Interlocked.CompareExchange(ref _outgoingScheduled, 1, 0) == 0)
        {
            SFBootstrap.PostToMainThread(ProcessOutgoingBatch);
        }
    }

    private void ProcessOutgoingBatch()
    {
        int processed = 0;
        while (processed < MaxDispatchPerTick && _pendingOutgoing.TryDequeue(out var item))
        {
            _outgoingHandlers.Dispatch(item.RpcId, item.Packet, item.DataBitLength);
            processed++;
        }

        if (_pendingOutgoing.IsEmpty)
        {
            Interlocked.Exchange(ref _outgoingScheduled, 0);
            if (!_pendingOutgoing.IsEmpty)
            {
                if (Interlocked.CompareExchange(ref _outgoingScheduled, 1, 0) == 0)
                {
                    SFBootstrap.PostToMainThread(ProcessOutgoingBatch);
                }
            }

            return;
        }

        SFBootstrap.PostToMainThread(ProcessOutgoingBatch);
    }
}
