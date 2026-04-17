namespace SFSharp.Runtime.Networking;

/// <summary>
/// Thread-safe, lock-free-read filter registry for synchronous packet/RPC filtering on hook threads.
/// Uses copy-on-write: writes take a lock and rebuild the array; reads use a volatile snapshot with no lock.
/// Filters return true to cancel the packet/RPC.
/// </summary>
public sealed class NetworkFilterRegistry
{
    private volatile FilterEntry[] _filters = [];
    private readonly Lock _sync = new();

    /// <summary>
    /// Returns true if any filter wants to cancel the packet/RPC with the given id.
    /// Called on the hook thread — must be fast and lock-free.
    /// </summary>
    public unsafe bool ShouldCancel(int id, byte* dataPtr, int bitLength)
    {
        FilterEntry[] snapshot = _filters;
        if (snapshot.Length == 0)
        {
            return false;
        }

        for (int i = 0; i < snapshot.Length; i++)
        {
            FilterEntry entry = snapshot[i];
            try
            {
                if (entry.TargetId is int targetId && targetId != id)
                {
                    continue;
                }

                if (entry.Filter(id, (nint)dataPtr, bitLength))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                SFLog.Error(ex, $"NetworkFilter callback failed id={id}");
            }
        }

        return false;
    }

    public bool HasFilters => _filters.Length > 0;

    /// <summary>
    /// Register a global filter that receives all ids. Returns IDisposable to unregister.
    /// </summary>
    public IDisposable Add(Func<int, nint, int, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        FilterEntry entry = new(null, filter);
        AddEntry(entry);
        return new FilterRegistration(this, entry);
    }

    /// <summary>
    /// Register a filter for a specific id. The callback only receives (dataPtr, bitLength).
    /// </summary>
    public IDisposable Add(int targetId, Func<nint, int, bool> filter)
    {
        ArgumentNullException.ThrowIfNull(filter);
        FilterEntry entry = new(targetId, (_, dataPtr, bitLength) => filter(dataPtr, bitLength));
        AddEntry(entry);
        return new FilterRegistration(this, entry);
    }

    private void AddEntry(FilterEntry entry)
    {
        lock (_sync)
        {
            FilterEntry[] current = _filters;
            FilterEntry[] next = new FilterEntry[current.Length + 1];
            current.CopyTo(next, 0);
            next[current.Length] = entry;
            _filters = next;
        }
    }

    private void RemoveEntry(FilterEntry entry)
    {
        lock (_sync)
        {
            FilterEntry[] current = _filters;
            int index = Array.IndexOf(current, entry);
            if (index < 0)
            {
                return;
            }

            if (current.Length == 1)
            {
                _filters = [];
                return;
            }

            FilterEntry[] next = new FilterEntry[current.Length - 1];
            Array.Copy(current, 0, next, 0, index);
            Array.Copy(current, index + 1, next, index, current.Length - index - 1);
            _filters = next;
        }
    }

    private sealed record FilterEntry(int? TargetId, Func<int, nint, int, bool> Filter);

    private sealed class FilterRegistration : IDisposable
    {
        private readonly NetworkFilterRegistry _registry;
        private readonly FilterEntry _entry;
        private int _disposed;

        public FilterRegistration(NetworkFilterRegistry registry, FilterEntry entry)
        {
            _registry = registry;
            _entry = entry;
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            _registry.RemoveEntry(_entry);
        }
    }
}
