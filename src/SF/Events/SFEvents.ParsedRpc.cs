namespace SFSharp.Runtime.Events;

public sealed partial class SFEvents
{
    public IDisposable OnIncomingRpc<TRpc>(Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return GetOrCreateIncomingRpcChannel<TRpc>().Subscribe(handler);
    }

    public IAsyncEnumerable<TRpc> StreamIncomingRpc<TRpc>(CancellationToken token = default)
    {
        return GetOrCreateIncomingRpcChannel<TRpc>().Stream(token);
    }

    public IDisposable OnOutgoingRpc<TRpc>(Action<TRpc> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return GetOrCreateOutgoingRpcChannel<TRpc>().Subscribe(handler);
    }

    public IAsyncEnumerable<TRpc> StreamOutgoingRpc<TRpc>(CancellationToken token = default)
    {
        return GetOrCreateOutgoingRpcChannel<TRpc>().Stream(token);
    }
}
