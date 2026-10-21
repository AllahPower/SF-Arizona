namespace SFSharp.Runtime.Networking;

internal sealed class SubscriptionGroup : IDisposable
{
    private readonly Lock _sync = new();
    private readonly List<IDisposable> _items = [];
    private bool _disposed;

    public void Add(IDisposable subscription)
    {
        ArgumentNullException.ThrowIfNull(subscription);
        lock (_sync)
        {
            if (!_disposed)
            {
                _items.Add(subscription);
                return;
            }
        }
        subscription.Dispose();
    }

    public void Link(CancellationToken token)
    {
        if (token.CanBeCanceled)
            Add(token.UnsafeRegister(static state => ((SubscriptionGroup)state!).Dispose(), this));
    }

    public void Dispose()
    {
        IDisposable[] items;
        lock (_sync)
        {
            if (_disposed) return;
            _disposed = true;
            items = [.. _items];
            _items.Clear();
        }

        for (int i = items.Length - 1; i >= 0; i--)
        {
            try { items[i].Dispose(); }
            catch (Exception ex)
            {
                try { SFLog.Error(ex, "Failed to release a parser subscription"); }
                catch { /* Release the remaining registrations even if the log sink failed. */ }
            }
        }
    }
}
