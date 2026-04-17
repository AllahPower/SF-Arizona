namespace SFSharp;

/// <summary>
/// Plugin-facing RakNet transport facade. Sends packets/RPCs through the game's RakClient and
/// injects synthetic traffic into the receive pipeline.
/// </summary>
/// <remarks>
/// NOT thread-safe. Every method must be called from the main game thread because the underlying
/// RakPeer queues are single-producer. Use <see cref="IModuleContext.SwitchToMainThreadAsync"/>
/// before calling any member from a background context.
/// </remarks>
public interface ISFNetwork
{
    /// <summary>Sends a raw packet through <c>RakClient::Send</c>. Main-thread only.</summary>
    bool SendPacket(
        ReadOnlySpan<byte> data,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0);

    /// <summary>Sends a raw RPC through <c>RakClient::RPC</c>. Main-thread only.</summary>
    bool SendRpc(
        int rpcId,
        ReadOnlySpan<byte> payload,
        int payloadBitLength,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false);

    /// <summary>Sends an RPC with no payload. Main-thread only.</summary>
    bool SendRpc(
        int rpcId,
        RakNetPacketPriority priority = RakNetPacketPriority.High,
        RakNetPacketReliability reliability = RakNetPacketReliability.ReliableOrdered,
        byte orderingChannel = 0,
        bool shiftTimestamp = false);

    /// <summary>Injects a synthetic packet into RakPeer's producer queue. Main-thread only.</summary>
    bool SimulateIncomingPacket(ReadOnlySpan<byte> data);

    /// <summary>Wraps <paramref name="payload"/> in an ID_RPC container and injects it. Main-thread only.</summary>
    bool SimulateIncomingRpc(int rpcId, ReadOnlySpan<byte> payload, int payloadBitLength);

    /// <summary>Injects a synthetic RPC with no payload. Main-thread only.</summary>
    bool SimulateIncomingRpc(int rpcId);
}
