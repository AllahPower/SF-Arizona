using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Samp.Events;

internal sealed class SampRemotePlayerEvents : ISFSampRemotePlayerEvents
{
    private readonly Lazy<SampObservationHooks> _hooks;
    private readonly SampActionEvent<SFSampRemoteSyncEvent>[] _sync;
    private readonly SampActionEvent<SFSampRemoteStateEvent> _states;
    private readonly SampActionEvent<SFSampRemoteActionEvent> _actions;
    private readonly SampRemoteStateTracker _tracker = new();
    private readonly ISFSampEntities _entities;

    public SampRemotePlayerEvents(MainThreadDispatcher mainThread, ISFSampEntities entities)
    {
        _entities = entities;
        _hooks = new(() => SampObservationHooks.Create(Install));
        _sync = Enum.GetValues<SFSampSyncKind>().Select(kind => new SampActionEvent<SFSampRemoteSyncEvent>(
            $"RemoteSync.{kind}", mainThread, _ => _hooks.Value)).ToArray();
        _states = new(nameof(OnStateChanged), mainThread, _ => _hooks.Value);
        _actions = new(nameof(OnActionCompleted), mainThread, _ => _hooks.Value);
    }

    public IDisposable OnSyncProcessed(SFSampSyncKind kind, Action<SFSampRemoteSyncEvent> handler)
    {
        if ((uint)kind >= (uint)_sync.Length) throw new ArgumentOutOfRangeException(nameof(kind));
        return _sync[(int)kind].Subscribe(handler);
    }
    public IDisposable OnStateChanged(Action<SFSampRemoteStateEvent> handler) => _states.Subscribe(handler);
    public IDisposable OnActionCompleted(Action<SFSampRemoteActionEvent> handler) => _actions.Subscribe(handler);

    private void Observe(nint instance, SFSampRemoteSnapshot snapshot)
    {
        if (_states.HasSubscribers && _tracker.Observe(instance, snapshot.PlayerId, snapshot.State) is { } change)
            _states.Publish(change);
    }

    private void Install(SampObservationHooks hooks)
    {
        SFSampVersion version = SampEnvironment.RequiredProfile.Version;
        // The existing lifecycle bridge owns Spawn/Remove hooks. Reuse its managed subscription
        // instead of installing a second MinHook at the same native address.
        hooks.Own(_entities.OnSampEntityChanged(change =>
        {
            if (change.Type == SFSampEntityType.Player) _tracker.Forget(change.SampId);
        }));
        (SampObservationPoint point, SFSampSyncKind kind)[] entries = [
            (SampObservationPoint.RemoteOnFoot, SFSampSyncKind.OnFoot),
            (SampObservationPoint.RemoteVehicle, SFSampSyncKind.Vehicle),
            (SampObservationPoint.RemotePassenger, SFSampSyncKind.Passenger),
            (SampObservationPoint.RemoteAim, SFSampSyncKind.Aim),
            (SampObservationPoint.RemoteTrailer, SFSampSyncKind.Trailer)];
        foreach ((SampObservationPoint point, SFSampSyncKind kind) in entries)
        {
            SampActionEvent<SFSampRemoteSyncEvent> source = _sync[(int)kind];
            hooks.Add(point, kind is SFSampSyncKind.Aim or SFSampSyncKind.Trailer ? SampObservationAbi.Pointer : SampObservationAbi.TimedSync,
                new SampObservation<SyncCapture>(args =>
                {
                    SFSampRemoteSnapshot before = SampObservationMemory.Remote(args.Instance, version);
                    Observe(args.Instance, before);
                    return new(before, source.HasSubscribers ? SampObservationMemory.Input(args.Data, kind) : default,
                        source.HasSubscribers ? SampObservationMemory.Stored(args.Instance, version, kind) : default);
                }, (args, capture) =>
                {
                    SFSampRemoteSnapshot after = SampObservationMemory.Remote(args.Instance, version);
                    Observe(args.Instance, after);
                    if (!source.HasSubscribers) return;
                    SFSampSyncPayload stored = SampObservationMemory.Stored(args.Instance, version, kind);
                    // Passenger's timestamp is an unused ABI slot. Aim/trailer have no time argument.
                    uint? timestamp = kind is SFSampSyncKind.OnFoot or SFSampSyncKind.Vehicle ? args.Timestamp : null;
                    source.Publish(new(kind, capture.Before, after, timestamp, capture.Input, stored, !capture.Stored.ContentEquals(stored)));
                }, () => source.HasSubscribers || _states.HasSubscribers));
        }
        hooks.Add(SampObservationPoint.RemoteProcess, SampObservationAbi.Void, new SampObservation<NoRetValue>(args =>
        {
            Observe(args.Instance, SampObservationMemory.Remote(args.Instance, version));
            return default;
        }, (args, _) => Observe(args.Instance, SampObservationMemory.Remote(args.Instance, version)), () => _states.HasSubscribers));

        foreach (SFSampRemoteAction action in Enum.GetValues<SFSampRemoteAction>())
        {
            SampObservationPoint point = action switch
            {
                SFSampRemoteAction.EnterVehicle => SampObservationPoint.RemoteEnterVehicle,
                SFSampRemoteAction.ExitVehicle => SampObservationPoint.RemoteExitVehicle,
                _ => SampObservationPoint.RemoteKill,
            };
            hooks.Add(point, action == SFSampRemoteAction.EnterVehicle ? SampObservationAbi.Entry : SampObservationAbi.Void,
                new SampObservation<SFSampRemoteSnapshot>(args =>
                {
                    SFSampRemoteSnapshot before = SampObservationMemory.Remote(args.Instance, version);
                    Observe(args.Instance, before);
                    return before;
                }, (args, before) =>
                {
                    SFSampRemoteSnapshot after = SampObservationMemory.Remote(args.Instance, version);
                    Observe(args.Instance, after);
                    _actions.Publish(new(before.PlayerId, action,
                        action == SFSampRemoteAction.EnterVehicle ? (ushort)args.Value : before.VehicleId,
                        action == SFSampRemoteAction.EnterVehicle && args.Extra != 0, after));
                }, () => _actions.HasSubscribers || _states.HasSubscribers));
        }
    }

    private readonly record struct SyncCapture(SFSampRemoteSnapshot Before, SFSampSyncPayload Input, SFSampSyncPayload Stored);
}

/// <summary>Tracks observed transitions without trusting ChangeState, which is not the state writer in R3.</summary>
internal sealed class SampRemoteStateTracker
{
    private readonly Lock _gate = new();
    private readonly nint[] _instances = new nint[1004];
    private readonly byte[] _states = new byte[1004];

    internal SFSampRemoteStateEvent? Observe(nint instance, ushort id, byte state)
    {
        if (id >= _states.Length || instance == 0) return null;
        lock (_gate)
        {
            byte? previous = _instances[id] == instance ? _states[id] : null;
            if (previous == state) return null;
            _instances[id] = instance;
            _states[id] = state;
            return new(id, previous, state);
        }
    }

    internal void Forget(ushort id)
    {
        if (id >= _instances.Length) return;
        lock (_gate) _instances[id] = 0;
    }
}
