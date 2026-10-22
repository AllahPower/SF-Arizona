using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampSessionEvents : ISFSampSessionEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampConnectionEvent> _events;
    private readonly SampActionEvent<SFSampSessionStateEvent> _states;
    private readonly SampActionEvent<NoRetValue> _attempts;
    private readonly Func<SFSampGameState> _readState;
    private SFSampGameState? _previous;

    public SampSessionEvents(MainThreadDispatcher mainThread, ISFGameClock clock)
        : this(mainThread, clock, SampObservationMemory.SessionState) { }

    internal SampSessionEvents(MainThreadDispatcher mainThread, ISFGameClock clock, Func<SFSampGameState> readState)
    {
        _readState = readState;
        _hooks = new(() => SampObservationHooks.Create(Install));
        _events = new(nameof(OnConnectionEvent), mainThread, _ => _hooks.Value);
        _states = new(nameof(OnStateChanged), mainThread, _ => clock.Subscribe(_ => Poll()));
        _attempts = new(nameof(OnConnectAttempt), mainThread, publish => new SampVoidActionHook(
            ModuleResolver.GetProcAddress("samp.dll", SampObservationOffsets.Resolve(SampEnvironment.RequiredProfile.Version, SampObservationPoint.Connect)),
            () => publish(default)));
    }

    public IDisposable OnConnectionEvent(Action<SFSampConnectionEvent> handler) => _events.Subscribe(handler);
    public IDisposable OnStateChanged(Action<SFSampSessionStateEvent> handler) => _states.Subscribe(handler);
    public IDisposable OnConnectAttempt(Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return _attempts.Subscribe(_ => handler());
    }

    private void Poll()
    {
        if (!_states.HasSubscribers) { _previous = null; return; }
        SFSampGameState state = _readState();
        if (_previous == state) return;
        SFSampGameState? previous = _previous;
        _previous = state;
        _states.Publish(new(previous, state));
    }

    private void Install(SampObservationHooks hooks)
    {
        foreach (SFSampConnectionEventKind kind in Enum.GetValues<SFSampConnectionEventKind>())
        {
            SampObservationPoint point = kind switch
            {
                SFSampConnectionEventKind.PublicKeyMismatch => SampObservationPoint.PublicKeyMismatch,
                SFSampConnectionEventKind.Banned => SampObservationPoint.Banned,
                SFSampConnectionEventKind.RequestAccepted => SampObservationPoint.RequestAccepted,
                SFSampConnectionEventKind.ServerFull => SampObservationPoint.ServerFull,
                SFSampConnectionEventKind.Disconnected => SampObservationPoint.Disconnected,
                SFSampConnectionEventKind.InvalidPassword => SampObservationPoint.InvalidPassword,
                SFSampConnectionEventKind.AttemptFailed => SampObservationPoint.AttemptFailed,
                SFSampConnectionEventKind.ConnectionLost => SampObservationPoint.ConnectionLost,
                SFSampConnectionEventKind.JoinHandshake => SampObservationPoint.JoinHandshake,
                _ => throw new ArgumentOutOfRangeException(),
            };
            hooks.Add(point, SampObservationAbi.Pointer, new SampObservation<NoRetValue>(
                _ => default, (_, _) => _events.Publish(new(kind, _readState())), () => _events.HasSubscribers));
        }
    }
}
