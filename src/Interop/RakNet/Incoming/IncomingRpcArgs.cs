using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Networking;

public readonly record struct IncomingRpcArgs(int ERpcId, nint DataPtr, int DataBitOffset, int DataBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, DataBitOffset, DataBitLength);
    }
}

public class NetworkSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private int _disposed;

    internal NetworkSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public virtual void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _unsubscribe();
    }
}

public sealed class RpcSubscription : NetworkSubscription
{
    internal RpcSubscription(Action unsubscribe)
        : base(unsubscribe)
    {
    }
}
