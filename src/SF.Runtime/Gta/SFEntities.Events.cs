namespace SFSharp.Runtime.Gta;

public sealed unsafe partial class SFEntities :
    ISubHook<PedDamageArgs, NoRetValue>,
    ISubHook<VehicleDamageArgs, NoRetValue>,
    ISubHook<ObjectDamageArgs, NoRetValue>
{
    private readonly EntityEventSource<SFDamageEvent> _damage = new(nameof(OnDamage));
    private readonly List<IDisposable> _hooks = [];
    private bool _damageHooksInstalled;

    public IDisposable OnDamage(Action<SFDamageEvent> handler)
    {
        RequireThread();
        ArgumentNullException.ThrowIfNull(handler);
        InstallDamageHooks();
        return _damage.Subscribe(handler);
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
