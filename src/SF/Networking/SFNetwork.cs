using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

/// <summary>
/// Builds a packet payload into a <see cref="BitStreamWriter"/> passed by reference.
/// </summary>
public delegate void BitStreamBuildAction(ref BitStreamWriter writer);

/// <summary>
/// High-level facade over the RakNet client: sends packets/RPCs and injects synthetic
/// traffic into the incoming pipeline the same way rakhook's emul_packet / emul_rpc does.
/// </summary>
/// <remarks>
/// NOT thread-safe. Every method must be called from the main game thread - the underlying
/// RakPeer queues are single-producer and native send paths are not re-entrant.
/// </remarks>
public sealed unsafe class SFNetwork : ISFNetwork
{
    private const byte IdRpc = (byte)EPacketId.Rpc;

    // Packet* __cdecl AllocPacket(unsigned int dataSize)
    private static readonly delegate* unmanaged[Cdecl]<uint, CRakNetPacket*> _allocPacket =
        (delegate* unmanaged[Cdecl]<uint, CRakNetPacket*>)
            ModuleResolver.GetProcAddress("samp.dll", (uint)SampOffsets.RakPeer.AllocPacket);

    // Packet** __thiscall WriteLock(SingleProducerConsumer* queue)
    private static readonly delegate* unmanaged[Thiscall]<nint, CRakNetPacket**> _writeLock =
        (delegate* unmanaged[Thiscall]<nint, CRakNetPacket**>)
            ModuleResolver.GetProcAddress("samp.dll", (uint)SampOffsets.RakPeer.WriteLock);

    // void __thiscall WriteUnlock(SingleProducerConsumer* queue)
    private static readonly delegate* unmanaged[Thiscall]<nint, void> _writeUnlock =
        (delegate* unmanaged[Thiscall]<nint, void>)
            ModuleResolver.GetProcAddress("samp.dll", (uint)SampOffsets.RakPeer.WriteUnlock);

    private static bool EmulationAvailable =>
        _allocPacket != null && _writeLock != null && _writeUnlock != null;

    // Outgoing: packets

    public bool SendPacket(
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
        ArgumentNullException.ThrowIfNull(build);

        BitStreamWriter writer = new();
        build(ref writer);
        return SendPacket(writer.AsSpan(), priority, reliability, orderingChannel);
    }

    // Outgoing: RPCs

    public bool SendRpc(
        int rpcId,
        ReadOnlySpan<byte> payload,
        int payloadBitLength,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        if (payloadBitLength < 0 || !CNetGame.TryGetRakClient(out CRakClientInterface* rakClient))
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
        => SendRpc((int)rpcId, payload, payloadBitLength, priority, reliability, orderingChannel, shiftTimestamp);

    public bool SendRpc(
        int rpcId,
        BitStreamBuildAction build,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
    {
        ArgumentNullException.ThrowIfNull(build);

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
        => SendRpc((int)rpcId, build, priority, reliability, orderingChannel, shiftTimestamp);

    public bool SendRpc(
        int rpcId,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
        => SendRpc(rpcId, ReadOnlySpan<byte>.Empty, 0, priority, reliability, orderingChannel, shiftTimestamp);

    public bool SendRpc(
        ERpcId rpcId,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false)
        => SendRpc((int)rpcId, priority, reliability, orderingChannel, shiftTimestamp);

    // Incoming: packet/RPC emulation (rakhook::emul_packet / emul_rpc)

    /// <summary>
    /// Injects a synthetic packet into RakPeer's producer queue. The next
    /// <c>RakClient::Receive</c> call delivers it through the full native pipeline
    /// as if it had arrived from the server.
    /// </summary>
    public bool SimulateIncomingPacket(ReadOnlySpan<byte> data)
    {
        if (data.IsEmpty)
        {
            return false;
        }

        nint rakPeer = IncomingRpcPacketHook.RakPeerInstance;
        if (rakPeer == 0 || !IncomingRpcPacketHook.HasServerPlayerId || !EmulationAvailable)
        {
            return false;
        }

        CRakNetPacket* packet = _allocPacket((uint)data.Length);
        if (packet == null)
        {
            return false;
        }

        fixed (byte* src = data)
        {
            Buffer.MemoryCopy(src, packet->Data, data.Length, data.Length);
        }

        // AllocPacket only wires Data/DeleteData - the rest must be populated or SA-MP's
        // Receive loop treats the packet as empty / non-server-originated and drops it.
        packet->Length = data.Length;
        packet->BitSize = data.Length * 8;
        packet->PlayerIndex = 0;

        RakNetPlayerId serverId = IncomingRpcPacketHook.ServerPlayerId;
        packet->PlayerId = new CRakNetPlayerId
        {
            BinaryAddress = serverId.BinaryAddress,
            Port = serverId.Port,
        };

        nint queue = rakPeer + SampOffsets.RakPeer.OffsetPackets;
        CRakNetPacket** slot = _writeLock(queue);
        *slot = packet;
        _writeUnlock(queue);

        return true;
    }

    public bool SimulateIncomingPacket(BitStreamBuildAction build)
    {
        ArgumentNullException.ThrowIfNull(build);

        BitStreamWriter writer = new();
        build(ref writer);
        return SimulateIncomingPacket(writer.AsSpan());
    }

    /// <summary>
    /// Wraps an RPC payload in an ID_RPC container and pushes it into RakPeer's receive
    /// queue, replicating the shape of a real server-originated RPC. The packet travels
    /// through the stock receive path and fires the regular HandleRpcPacket hook, so all
    /// subscribers see it exactly like a genuine RPC.
    /// </summary>
    public bool SimulateIncomingRpc(int rpcId, ReadOnlySpan<byte> payload, int payloadBitLength)
    {
        if ((uint)rpcId > byte.MaxValue || payloadBitLength < 0)
        {
            return false;
        }

        int payloadByteLength = (payloadBitLength + 7) / 8;
        if (payloadByteLength > payload.Length)
        {
            return false;
        }

        BitStreamWriter writer = new(2 + sizeof(uint) + payloadByteLength);
        writer.WriteUInt8(IdRpc);
        writer.WriteUInt8((byte)rpcId);
        writer.WriteCompressedUInt32((uint)payloadBitLength);
        if (payloadBitLength > 0)
        {
            writer.WriteBytes(payload[..payloadByteLength]);
        }

        return SimulateIncomingPacket(writer.AsSpan());
    }

    public bool SimulateIncomingRpc(ERpcId rpcId, ReadOnlySpan<byte> payload, int payloadBitLength)
        => SimulateIncomingRpc((int)rpcId, payload, payloadBitLength);

    public bool SimulateIncomingRpc(int rpcId, BitStreamBuildAction build)
    {
        ArgumentNullException.ThrowIfNull(build);

        BitStreamWriter payload = new();
        build(ref payload);
        return SimulateIncomingRpc(rpcId, payload.AsSpan(), payload.BitLength);
    }

    public bool SimulateIncomingRpc(ERpcId rpcId, BitStreamBuildAction build)
        => SimulateIncomingRpc((int)rpcId, build);

    public bool SimulateIncomingRpc(int rpcId)
        => SimulateIncomingRpc(rpcId, ReadOnlySpan<byte>.Empty, 0);

    public bool SimulateIncomingRpc(ERpcId rpcId)
        => SimulateIncomingRpc((int)rpcId);
}
