namespace SFSharp.Runtime.Networking.RakNet.Filtering;

/// <summary>Synchronous drop filters consulted by the network hooks before traffic is queued.</summary>
public sealed class NetworkFilters
{
    public NetworkFilterRegistry IncomingRpc { get; } = new();
    public NetworkFilterRegistry OutgoingRpc { get; } = new();
    public NetworkFilterRegistry IncomingPacket { get; } = new();
    public NetworkFilterRegistry OutgoingPacket { get; } = new();
}
