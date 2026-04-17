namespace SFSharp;

/// <summary>
/// Plugin-facing raw RPC transport facade. Every frame is detached from the game memory and copied
/// into managed storage so external modules can inspect it without direct access to SA-MP buffers.
/// </summary>
/// <remarks>
/// Subscribe/Stream registration is thread-safe; handlers fire on the main game thread. Filter
/// registration is thread-safe, but filter callbacks run synchronously on the RakNet hook thread -
/// see <see cref="SFRpcFilterCallback"/>.
/// </remarks>
public interface ISFRpc
{
    IDisposable SubscribeIncoming(int rpcId, Action<IncomingRpcFrame> handler);
    IAsyncEnumerable<IncomingRpcFrame> StreamIncoming(int rpcId, CancellationToken token = default);

    IDisposable SubscribeOutgoing(int rpcId, Action<OutgoingRpcFrame> handler);
    IAsyncEnumerable<OutgoingRpcFrame> StreamOutgoing(int rpcId, CancellationToken token = default);

    /// <summary>
    /// Registers a synchronous incoming RPC filter. Return <see langword="true"/> from the
    /// callback to cancel (drop) the RPC before SA-MP processes it.
    /// </summary>
    IDisposable RegisterIncomingFilter(int rpcId, SFRpcFilterCallback filter);

    /// <summary>Registers a catch-all incoming RPC filter across all RPC ids.</summary>
    IDisposable RegisterIncomingFilter(SFRpcFilterCallback filter);

    /// <summary>
    /// Registers a synchronous outgoing RPC filter. Return <see langword="true"/> from the
    /// callback to cancel (drop) the RPC before it is sent.
    /// </summary>
    IDisposable RegisterOutgoingFilter(int rpcId, SFRpcFilterCallback filter);

    /// <summary>Registers a catch-all outgoing RPC filter across all RPC ids.</summary>
    IDisposable RegisterOutgoingFilter(SFRpcFilterCallback filter);
}
