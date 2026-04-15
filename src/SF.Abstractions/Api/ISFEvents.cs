namespace SFSharp;

/// <summary>
/// Plugin-facing typed parsed event surface. Event payload models are supplied by host-shared
/// assemblies and dispatched on the main thread after parsing completes.
/// </summary>
public interface ISFEvents
{
    IDisposable OnIncomingRpc<TRpc>(Action<TRpc> handler);
    IAsyncEnumerable<TRpc> StreamIncomingRpc<TRpc>(CancellationToken token = default);

    IDisposable OnOutgoingRpc<TRpc>(Action<TRpc> handler);
    IAsyncEnumerable<TRpc> StreamOutgoingRpc<TRpc>(CancellationToken token = default);

    IDisposable OnIncomingPacket<TPacket>(Action<TPacket> handler);
    IAsyncEnumerable<TPacket> StreamIncomingPacket<TPacket>(CancellationToken token = default);

    IDisposable OnOutgoingPacket<TPacket>(Action<TPacket> handler);
    IAsyncEnumerable<TPacket> StreamOutgoingPacket<TPacket>(CancellationToken token = default);
}
