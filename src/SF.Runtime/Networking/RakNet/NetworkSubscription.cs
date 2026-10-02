namespace SFSharp.Runtime.Networking.RakNet;

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
