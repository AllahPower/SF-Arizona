namespace SFSharp.Abstractions.Samp;

/// <summary>SA-MP R3 services. Entity mappings bridge server IDs to GTA pool handles.</summary>
public interface ISFSamp
{
    ISFChat Chat { get; }
    ISFDialog Dialog { get; }
    ISFPlayers Players { get; }
    ISFVehicles Vehicles { get; }
    ISFGamePools Pools { get; }
    ISFSampEntities Entities { get; }
    ISFSampCamera Camera { get; }
    ISFEvents Events { get; }
    ISFRpc Rpc { get; }
    ISFPackets Packets { get; }
    ISFPacketParsers PacketParsers { get; }
    ISFRpcParsers RpcParsers { get; }
    ISFNetwork Network { get; }
}
