namespace SFSharp.Runtime.Network.RakNet.Incoming;

public readonly record struct IncomingRpcArgs(int ERpcId, nint DataPtr, int DataBitOffset, int DataBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, DataBitOffset, DataBitLength);
    }
}

public class NetworkSubscription : IDisposable
{
    private Action? _unsubscribe;

    internal NetworkSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public virtual void Dispose()
    {
        // Release the captured handler/owner even if the disposed subscription is retained.
        Interlocked.Exchange(ref _unsubscribe, null)?.Invoke();
    }
}

public sealed class RpcSubscription : NetworkSubscription
{
    internal RpcSubscription(Action unsubscribe)
        : base(unsubscribe)
    {
    }
}
