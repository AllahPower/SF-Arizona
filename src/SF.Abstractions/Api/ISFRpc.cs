namespace SFSharp;

/// <summary>
/// Plugin-facing raw RPC transport facade. Every frame is detached from the game memory and copied
/// into managed storage so external modules can inspect it without direct access to SA-MP buffers.
/// </summary>
public interface ISFRpc
{
    IDisposable SubscribeIncoming(int rpcId, Action<IncomingRpcFrame> handler);
    IAsyncEnumerable<IncomingRpcFrame> StreamIncoming(int rpcId, CancellationToken token = default);

    IDisposable SubscribeOutgoing(int rpcId, Action<OutgoingRpcFrame> handler);
    IAsyncEnumerable<OutgoingRpcFrame> StreamOutgoing(int rpcId, CancellationToken token = default);
}
