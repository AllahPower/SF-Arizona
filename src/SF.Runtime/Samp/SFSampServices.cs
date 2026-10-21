namespace SFSharp.Runtime.Samp;

internal sealed class SFSampServices(
    ISFChat chat, ISFDialog dialog, ISFPlayers players, ISFVehicles vehicles, ISFGamePools pools,
    ISFSampEntities entities, ISFSampCamera camera, ISFEvents events, ISFRpc rpc, ISFPackets packets,
    ISFPacketParsers packetParsers, ISFRpcParsers rpcParsers, ISFNetwork network) : ISFSamp
{
    public SFSampClientInfo Client => SampEnvironment.Client;
    public ISFChat Chat => chat;
    public ISFDialog Dialog => dialog;
    public ISFPlayers Players => players;
    public ISFVehicles Vehicles => vehicles;
    public ISFGamePools Pools => pools;
    public ISFSampEntities Entities => entities;
    public ISFSampCamera Camera => camera;
    public ISFEvents Events => events;
    public ISFRpc Rpc => rpc;
    public ISFPackets Packets => packets;
    public ISFPacketParsers PacketParsers => packetParsers;
    public ISFRpcParsers RpcParsers => rpcParsers;
    public ISFNetwork Network => network;
}
