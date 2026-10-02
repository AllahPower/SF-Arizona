namespace SFSharp.Runtime.Game.Entities;

public sealed unsafe partial class SFEntities :
    ISubHook<PedDamageArgs, NoRetValue>,
    ISubHook<VehicleDamageArgs, NoRetValue>,
    ISubHook<ObjectDamageArgs, NoRetValue>
{
    private readonly EntityEventSource<SFSampEntityEvent> _sampEntityChanged = new(nameof(OnSampEntityChanged));
    private readonly EntityEventSource<SFDamageEvent> _damage = new(nameof(OnDamage));
    private readonly List<IDisposable> _hooks = [];
    private bool _lifecycleHooksInstalled;
    private bool _damageHooksInstalled;

    public IDisposable OnSampEntityChanged(Action<SFSampEntityEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        InstallLifecycleHooks();
        return _sampEntityChanged.Subscribe(handler);
    }

    public IDisposable OnDamage(Action<SFDamageEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        InstallDamageHooks();
        return _damage.Subscribe(handler);
    }

    private void InstallLifecycleHooks()
    {
        if (_lifecycleHooksInstalled)
        {
            return;
        }

        _lifecycleHooksInstalled = true;
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
        SFLog.Debug("Entity lifecycle hooks installed.");
    }

    private void InstallDamageHooks()
    {
        if (_damageHooksInstalled)
        {
            return;
        }

        _damageHooksInstalled = true;
        PedDamageHook ped = new();
        ped.AddSubHook(this);
        _hooks.Add(ped);
        VehicleDamageHook vehicle = new();
        vehicle.AddSubHook(this);
        _hooks.Add(vehicle);
        ObjectDamageHook obj = new();
        obj.AddSubHook(this);
        _hooks.Add(obj);
        SFLog.Debug("Entity damage hooks installed.");
    }

    private void Install<TArgs, TDelegate>(NativeHook<TArgs, bool, TDelegate> hook)
        where TArgs : ISampEntityLifecycleArgs
        where TDelegate : Delegate
    {
        hook.AddSubHook(new SampEntityLifecycleSubHook<TArgs>(RaiseSampEntityChanged));
        _hooks.Add(hook);
    }

    private void RaiseSampEntityChanged<TArgs>(TArgs args, SFSampEntityChange change)
        where TArgs : ISampEntityLifecycleArgs
    {
        if (!_sampEntityChanged.HasHandlers)
        {
            return;
        }

        ushort id = args.SampId;
        SFEntityRef entity = default;
        bool resolved = args.Type switch
        {
            SFSampEntityType.Player => TryGetPlayer(id, out entity),
            SFSampEntityType.Vehicle => TryGetVehicle(id, out entity),
            SFSampEntityType.Object => TryGetObject(id, out entity),
            SFSampEntityType.Actor => TryGetActor(id, out entity),
            _ => false,
        };

        // CRemotePlayer::Remove returns early when the player has no ped, so nothing leaves the world.
        if (change == SFSampEntityChange.StreamedOut && !resolved)
        {
            return;
        }

        _sampEntityChanged.Raise(new SFSampEntityEvent(args.Type, id, change, resolved ? entity : null));
    }

    NoRetValue ISubHook<PedDamageArgs, NoRetValue>.Process(PedDamageArgs args, Func<PedDamageArgs, NoRetValue> next)
    {
        if (_damage.HasHandlers && TryCreateRef(SFEntityKind.Ped, args.Ped, out SFEntityRef target))
        {
            byte* calculator = (byte*)args.Calculator;
            float* amount = (float*)(calculator + GtaOffsets.CPedDamageResponseCalculator.DamageFactor);
            SFDamageEvent damage = new(
                target,
                RefFromEntity(*(GtaEntity**)(calculator + GtaOffsets.CPedDamageResponseCalculator.Damager)),
                *(int*)(calculator + GtaOffsets.CPedDamageResponseCalculator.WeaponType),
                *(int*)(calculator + GtaOffsets.CPedDamageResponseCalculator.BodyPart),
                *amount);
            _damage.Raise(damage);

            // The response still has to be computed so the engine initializes it; zero damage keeps health unchanged.
            *amount = damage.Cancel ? 0f : damage.Amount;
        }

        return next(args);
    }

    NoRetValue ISubHook<VehicleDamageArgs, NoRetValue>.Process(VehicleDamageArgs args, Func<VehicleDamageArgs, NoRetValue> next)
    {
        if (!_damage.HasHandlers || !TryCreateRef(SFEntityKind.Vehicle, args.Vehicle, out SFEntityRef target))
        {
            return next(args);
        }

        SFDamageEvent damage = new(target, RefFromEntity((GtaEntity*)args.Source), args.Weapon, 0, args.Amount);
        _damage.Raise(damage);
        return damage.Cancel ? default : next(args with { Amount = damage.Amount });
    }

    NoRetValue ISubHook<ObjectDamageArgs, NoRetValue>.Process(ObjectDamageArgs args, Func<ObjectDamageArgs, NoRetValue> next)
    {
        if (!_damage.HasHandlers || !TryCreateRef(SFEntityKind.Object, args.Object, out SFEntityRef target))
        {
            return next(args);
        }

        SFDamageEvent damage = new(target, RefFromEntity((GtaEntity*)args.Source), args.Weapon, 0, args.Amount);
        _damage.Raise(damage);
        return damage.Cancel ? default : next(args with { Amount = damage.Amount });
    }

    private static SFEntityRef? RefFromEntity(GtaEntity* entity)
    {
        if (entity is null)
        {
            return null;
        }

        SFEntityKind? kind = entity->Type switch
        {
            GtaEntityType.Ped => SFEntityKind.Ped,
            GtaEntityType.Vehicle => SFEntityKind.Vehicle,
            GtaEntityType.Object => SFEntityKind.Object,
            _ => null,
        };

        return kind is { } value && TryCreateRef(value, (nint)entity, out SFEntityRef reference) ? reference : null;
    }
}
