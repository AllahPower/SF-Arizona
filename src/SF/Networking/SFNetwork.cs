using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

/// <summary>
/// Delegate that receives a BitStreamWriter by ref for building packet payload.
/// </summary>
public delegate void BitStreamBuildAction(ref BitStreamWriter writer);

public sealed class SFNetwork
{
    public unsafe bool SendPacket(
        ReadOnlySpan<byte> data,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
    {
        if (data.IsEmpty || !CNetGame.TryGetRakClient(out CRakClientInterface* rakClient))
        {
            return false;
        }

        return rakClient->Send(data, priority, reliability, orderingChannel);
    }

    public bool SendPacket(
        BitStreamBuildAction build,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0)
    {
        BitStreamWriter writer = new();
        build(ref writer);
        return SendPacket(writer.AsSpan(), priority, reliability, orderingChannel);
    }

    public unsafe bool SendRpc(
        int rpcId,
        ReadOnlySpan<byte> payload,
        int payloadBitLength,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        if (!CNetGame.TryGetRakClient(out CRakClientInterface* rakClient))
        {
            return false;
        }

        return rakClient->Rpc(rpcId, payload, payloadBitLength, priority, reliability, orderingChannel, shiftTimestamp);
    }

    public bool SendRpc(
        ERpcId rpcId,
        ReadOnlySpan<byte> payload,
        int payloadBitLength,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        return SendRpc((int)rpcId, payload, payloadBitLength, priority, reliability, orderingChannel, shiftTimestamp);
    }

    public bool SendRpc(
        int rpcId,
        BitStreamBuildAction build,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        BitStreamWriter writer = new();
        build(ref writer);
        return SendRpc(rpcId, writer.AsSpan(), writer.BitLength, priority, reliability, orderingChannel, shiftTimestamp);
    }

    public bool SendRpc(
        ERpcId rpcId,
        BitStreamBuildAction build,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        return SendRpc((int)rpcId, build, priority, reliability, orderingChannel, shiftTimestamp);
    }

    public unsafe bool SendRpc(
        int rpcId,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        return SendRpc(rpcId, ReadOnlySpan<byte>.Empty, 0, priority, reliability, orderingChannel, shiftTimestamp);
    }

    public bool SendRpc(
        ERpcId rpcId,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        return SendRpc((int)rpcId, priority, reliability, orderingChannel, shiftTimestamp);
    }
}
