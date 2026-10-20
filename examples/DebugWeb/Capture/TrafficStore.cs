namespace SFSharp.Examples.DebugWeb.Capture;

internal sealed class TrafficStore
{
    private readonly Lock _sync = new();
    private readonly Queue<CapturedTraffic> _entries = new();
    private readonly Dictionary<long, CapturedTraffic> _index = new();
    private long _bytes;
    private long _sequence;
    private long _evicted;
    private CaptureSettings _settings;

    public TrafficStore(CaptureSettings settings) => _settings = settings;
    public string Session { get; } = Guid.NewGuid().ToString("N");

    public TrafficSummary? Add(CapturedTraffic entry)
    {
        lock (_sync)
        {
            entry = entry with { Summary = entry.Summary with { Seq = ++_sequence } };
            _entries.Enqueue(entry);
            _index.Add(entry.Summary.Seq, entry);
            _bytes += entry.Data.Length;
            Trim();
            return _index.ContainsKey(entry.Summary.Seq) ? entry.Summary : null;
        }
    }

    public void Configure(CaptureSettings settings)
    {
        lock (_sync) { _settings = settings; Trim(); }
    }

    private void Trim()
    {
        while (_entries.Count > _settings.MaxEntries || _bytes > (long)_settings.RawBudgetMiB * 1024 * 1024)
        {
            CapturedTraffic removed = _entries.Dequeue();
            _index.Remove(removed.Summary.Seq);
            _bytes -= removed.Data.Length;
            _evicted++;
        }
    }

    public CapturedTraffic? Find(long seq)
    {
        lock (_sync) return _index.GetValueOrDefault(seq);
    }

    public CapturedTraffic[] Snapshot()
    {
        lock (_sync) return _entries.ToArray();
    }

    public CaptureStats Stats(long dropped, int port)
    {
        lock (_sync) return new(Session, _sequence, _entries.Count, _bytes, _evicted, dropped, port,
            _entries.TryPeek(out var first) ? first.Summary.Seq : _sequence + 1);
    }

    public void Clear()
    {
        lock (_sync) { _entries.Clear(); _index.Clear(); _bytes = 0; _evicted = 0; }
    }
}
