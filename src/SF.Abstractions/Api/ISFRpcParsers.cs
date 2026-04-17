namespace SFSharp;

/// <summary>Typed parsed RPC facade.</summary>
/// <remarks>Bind/Stream registration is thread-safe; handlers fire on the main game thread.</remarks>
public interface ISFRpcParsers
{
    IDisposable BindIncoming<TRpc>(Action<TRpc> handler, CancellationToken token = default);

    IDisposable BindOutgoing<TRpc>(Action<TRpc> handler, CancellationToken token = default);

    IAsyncEnumerable<TRpc> StreamIncoming<TRpc>(CancellationToken token = default);

    IAsyncEnumerable<TRpc> StreamOutgoing<TRpc>(CancellationToken token = default);
}
