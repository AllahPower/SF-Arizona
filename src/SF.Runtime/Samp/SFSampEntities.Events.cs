namespace SFSharp.Runtime.Samp;

public sealed unsafe partial class SFSampEntities
{
    private readonly EntityEventSource<SFSampEntityEvent> _changes = new(nameof(OnSampEntityChanged));
    private readonly List<IDisposable> _hooks = [];
    private bool _installed;

    public IDisposable OnSampEntityChanged(Action<SFSampEntityEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        if (!_installed)
        {
            Install(new VehiclePoolCreateHook());
            Install(new PoolDeleteHook(SFSampEntityType.Vehicle, SampOffsets.CVehiclePool.Delete));
            Install(new ObjectPoolCreateHook());
            Install(new PoolDeleteHook(SFSampEntityType.Object, SampOffsets.CObjectPool.Delete));
            Install(new ActorPoolCreateHook());
            Install(new PoolDeleteHook(SFSampEntityType.Actor, SampOffsets.CActorPool.Delete));
            Install(new PlayerPoolCreateHook());
            Install(new PlayerPoolDeleteHook());
            Install(new RemotePlayerSpawnHook());
            Install(new RemotePlayerRemoveHook());
            _installed = true;
        }
        return _changes.Subscribe(handler);
    }

    private void Install<TArgs, TDelegate>(NativeHook<TArgs, bool, TDelegate> hook)
        where TArgs : ISampEntityLifecycleArgs where TDelegate : Delegate
    {
        hook.AddSubHook(new SampEntityLifecycleSubHook<TArgs>(Raise));
        _hooks.Add(hook);
    }

    private void Raise<TArgs>(TArgs args, SFSampEntityChange change) where TArgs : ISampEntityLifecycleArgs
    {
        if (!_changes.HasHandlers) return;
        SFEntityRef entity = default;
        bool resolved = args.Type switch
        {
            SFSampEntityType.Player => TryGetPlayer(args.SampId, out entity),
            SFSampEntityType.Vehicle => TryGetVehicle(args.SampId, out entity),
            SFSampEntityType.Object => TryGetObject(args.SampId, out entity),
            SFSampEntityType.Actor => TryGetActor(args.SampId, out entity),
            _ => false
        };
        if (change == SFSampEntityChange.StreamedOut && !resolved) return;
        _changes.Raise(new(args.Type, args.SampId, change, resolved ? entity : null));
    }
}
