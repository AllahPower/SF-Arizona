using SFSharp;
using SFSharp.Interop.RakNet.Packets.Enum;

public partial class DebugModule
{
    private void OnIncomingRpc(IncomingRpcArgs args)
    {
        Interlocked.Increment(ref _totalInRpc);
        if (!_captureEnabled || !_captureIncoming || !_captureRpc) return;

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        Push(new TrafficEntry(0, TrafficDirection.Incoming, TrafficKind.Rpc, args.ERpcId, name,
            null, $"rpcId={args.ERpcId}", (args.DataBitLength + 7) / 8, Environment.TickCount64));
    }

    private void OnOutgoingRpc(OutgoingRpcArgs args)
    {
        Interlocked.Increment(ref _totalOutRpc);
        if (!_captureEnabled || !_captureOutgoing || !_captureRpc) return;

        string? name = Enum.IsDefined((ERpcId)args.ERpcId) ? ((ERpcId)args.ERpcId).ToString() : null;
        Push(new TrafficEntry(0, TrafficDirection.Outgoing, TrafficKind.Rpc, args.ERpcId, name,
            null, $"rpcId={args.ERpcId}", (args.DataBitLength + 7) / 8, Environment.TickCount64));
    }

    private void OnIncomingPacket(IncomingPacketArgs args)
    {
        Interlocked.Increment(ref _totalInPkt);
        if (!_captureEnabled || !_captureIncoming || !_capturePackets) return;

        (string? name, string? detail, string? parsed) = DecodeIncomingPacket(args);
        Push(new TrafficEntry(0, TrafficDirection.Incoming, TrafficKind.Packet, args.EPacketId, name,
            parsed, detail, args.DataByteLength, Environment.TickCount64));
    }

    private void OnOutgoingPacket(OutgoingPacketArgs args)
    {
        Interlocked.Increment(ref _totalOutPkt);
        if (!_captureEnabled || !_captureOutgoing || !_capturePackets) return;

        (string? name, string? detail, string? parsed) = DecodeOutgoingPacket(args);
        Push(new TrafficEntry(0, TrafficDirection.Outgoing, TrafficKind.Packet, args.EPacketId, name,
            parsed, detail, args.DataByteLength, Environment.TickCount64));
    }
}
