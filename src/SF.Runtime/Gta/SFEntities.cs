using System.Numerics;

namespace SFSharp.Runtime.Gta;

/// <summary>
/// Local GTA pool entities. Network IDs are handled by SFSampEntities. Main thread only.
/// </summary>
public sealed unsafe partial class SFEntities(ISFMainThread mainThread) : ISFEntities
{
    private void RequireThread()
    {
        if (!mainThread.IsMainThread) throw new InvalidOperationException("GTA entity operations require the game thread.");
    }

    public bool Exists(SFEntityRef entity) { RequireThread(); return Resolve(entity) is not null; }

    public bool TryGetSnapshot(SFEntityRef entity, out SFEntitySnapshot snapshot)
    {
        RequireThread();
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            snapshot = default;
            return false;
        }

        GtaPhysical* physical = (GtaPhysical*)native;
        float health = 0f;
        float armour = 0f;
        SFEntityRef? vehicle = null;
        SFEntityRef? driver = null;
        switch (entity.Kind)
        {
            case SFEntityKind.Ped:
                GtaPed* ped = (GtaPed*)native;
                health = ped->Health;
                armour = ped->Armour;
                if (ped->IsInVehicle)
                {
                    vehicle = ToRef(SFEntityKind.Vehicle, GtaPools.GetVehicleHandle(ped->Vehicle));
                }

                break;
            case SFEntityKind.Vehicle:
                GtaVehicle* gameVehicle = (GtaVehicle*)native;
                health = gameVehicle->Health;
                if (gameVehicle->Driver is not null)
                {
                    driver = ToRef(SFEntityKind.Ped, GtaPools.GetPedHandle(gameVehicle->Driver));
                }

                break;
            case SFEntityKind.Object:
                health = ((GtaObject*)native)->Health;
                break;
        }

        snapshot = new SFEntitySnapshot(
            entity,
            native->ModelIndex,
            native->Interior,
            native->Placeable.Position,
            native->Placeable.HeadingDegrees,
            native->Placeable.GetMatrix(),
            physical->MoveSpeed,
            physical->TurnSpeed,
            health,
            armour,
            vehicle,
            driver);
        return true;
    }

    public IReadOnlyList<SFEntityRef> Enumerate(SFEntityKind kind)
    {
        RequireThread();
        GtaPool* pool = kind switch
        {
            SFEntityKind.Ped => GtaPools.PedPool,
            SFEntityKind.Vehicle => GtaPools.VehiclePool,
            SFEntityKind.Object => GtaPools.ObjectPool,
            _ => null,
        };

        return GtaPools.GetOccupiedHandles(pool).ConvertAll(handle => new SFEntityRef(kind, handle));
    }

    public bool Teleport(SFEntityRef entity, Vector3 position)
    {
        RequireThread();
        SFGtaWorld.Validate(position);
        GtaEntity* native = Resolve(entity);
        if (native is null || (entity.Kind == SFEntityKind.Ped && ((GtaPed*)native)->IsInVehicle))
        {
            return false;
        }

        native->Teleport(position, resetRotation: false);
        return true;
    }

    public bool SetHeading(SFEntityRef entity, float heading)
    {
        RequireThread();
        if (!float.IsFinite(heading)) throw new ArgumentOutOfRangeException(nameof(heading));
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            return false;
        }

        float radians = heading * (MathF.PI / 180f);
        if (entity.Kind == SFEntityKind.Ped)
        {
            // Mirrors script command 0173 (SET_CHAR_HEADING): rotation fields first, skipped for peds in vehicles.
            GtaPed* ped = (GtaPed*)native;
            if (ped->IsInVehicle)
            {
                return false;
            }

            ped->CurrentRotation = radians;
            ped->AimingRotation = radians;
        }

        GtaFunctions.SetHeading(&native->Placeable, radians);
        return true;
    }

    public bool SetVelocity(SFEntityRef entity, Vector3 velocity)
    {
        RequireThread();
        SFGtaWorld.Validate(velocity);
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            return false;
        }

        ((GtaPhysical*)native)->MoveSpeed = velocity;
        return true;
    }

    public bool SetTurnSpeed(SFEntityRef entity, Vector3 turnSpeed)
    {
        RequireThread();
        SFGtaWorld.Validate(turnSpeed);
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            return false;
        }

        ((GtaPhysical*)native)->TurnSpeed = turnSpeed;
        return true;
    }

    public bool SetHealth(SFEntityRef entity, float health)
    {
        RequireThread();
        if (!float.IsFinite(health)) throw new ArgumentOutOfRangeException(nameof(health));
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            return false;
        }

        switch (entity.Kind)
        {
            case SFEntityKind.Ped:
                ((GtaPed*)native)->Health = health;
                break;
            case SFEntityKind.Vehicle:
                ((GtaVehicle*)native)->Health = health;
                break;
            case SFEntityKind.Object:
                ((GtaObject*)native)->Health = health;
                break;
        }

        return true;
    }

    public bool SetArmour(SFEntityRef entity, float armour)
    {
        RequireThread();
        if (!float.IsFinite(armour)) throw new ArgumentOutOfRangeException(nameof(armour));
        GtaEntity* native = entity.Kind == SFEntityKind.Ped ? Resolve(entity) : null;
        if (native is null)
        {
            return false;
        }

        ((GtaPed*)native)->Armour = armour;
        return true;
    }

    internal static GtaEntity* Resolve(SFEntityRef entity)
    {
        GtaEntity* native = entity.Kind switch
        {
            SFEntityKind.Ped => (GtaEntity*)GtaPools.GetPed(entity.Handle),
            SFEntityKind.Vehicle => (GtaEntity*)GtaPools.GetVehicle(entity.Handle),
            SFEntityKind.Object => (GtaEntity*)GtaPools.GetObject(entity.Handle),
            _ => null,
        };

        return native is not null && (byte)native->Type == (byte)entity.Kind ? native : null;
    }

    internal static bool TryCreateRef(SFEntityKind kind, nint gamePointer, out SFEntityRef entity)
    {
        int handle = gamePointer == 0 ? -1 : kind switch
        {
            SFEntityKind.Ped => GtaPools.GetPedHandle((GtaPed*)gamePointer),
            SFEntityKind.Vehicle => GtaPools.GetVehicleHandle((GtaVehicle*)gamePointer),
            SFEntityKind.Object => GtaPools.GetObjectHandle((GtaObject*)gamePointer),
            _ => -1,
        };

        entity = new SFEntityRef(kind, handle);
        return handle >= 0 && Resolve(entity) is not null;
    }

    private static SFEntityRef? ToRef(SFEntityKind kind, int handle)
    {
        SFEntityRef entity = new(kind, handle);
        return handle >= 0 && Resolve(entity) is not null ? entity : null;
    }

}
