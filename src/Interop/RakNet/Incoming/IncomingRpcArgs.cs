using System.Runtime.CompilerServices;

namespace SFSharp;

public readonly record struct IncomingRpcArgs(int ERpcId, nint DataPtr, int DataBitOffset, int DataBitLength)
{
    public unsafe BitStreamReader CreateReader()
    {
        return new BitStreamReader((byte*)DataPtr, DataBitOffset, DataBitLength);
    }
}

public sealed class RpcSubscription : IDisposable
{
    private readonly Action _unsubscribe;
    private int _disposed;

    internal RpcSubscription(Action unsubscribe)
    {
        _unsubscribe = unsubscribe;
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _unsubscribe();
    }
}
