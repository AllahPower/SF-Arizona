namespace SFSharp;

/// <summary>Typed parsed RPC facade.</summary>
public interface ISFRpcParsers
{
    IDisposable BindIncoming<TRpc>(Action<TRpc> handler, CancellationToken token = default);

    IDisposable BindOutgoing<TRpc>(Action<TRpc> handler, CancellationToken token = default);

    IAsyncEnumerable<TRpc> StreamIncoming<TRpc>(CancellationToken token = default);

    IAsyncEnumerable<TRpc> StreamOutgoing<TRpc>(CancellationToken token = default);
}
