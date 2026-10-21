namespace SFSharp.Runtime.Gta;

public sealed class SFGameClock : ISFGameClock
{
    private readonly EntityEventSource<SFGameTick> _ticks = new("GTA.Tick");
    private bool _hasFrame;
    public bool IsAvailable { get; internal set; }
    public SFGameTick Current { get; private set; }

    public IDisposable Subscribe(Action<SFGameTick> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!IsAvailable) throw new InvalidOperationException("The GTA Process call site is unavailable.");
        return _ticks.Subscribe(handler);
    }

    internal void Publish(uint frame, uint milliseconds, float step, bool paused)
    {
        if (_hasFrame && Current.Frame == frame) return;
        uint elapsed = _hasFrame ? unchecked(milliseconds - Current.GameMilliseconds) : 0;
        // A restarted game timer is a discontinuity, not weeks of elapsed simulation time.
        if (elapsed > int.MaxValue || paused) elapsed = 0;
        Current = new(frame, milliseconds, TimeSpan.FromMilliseconds(elapsed), step, paused);
        _hasFrame = true;
        _ticks.Raise(Current);
    }
}
