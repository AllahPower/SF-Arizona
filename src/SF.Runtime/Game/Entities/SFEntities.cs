using System.Numerics;

namespace SFSharp.Runtime.Game.Entities;

/// <summary>
/// <see cref="ISFEntities"/> over the GTA engine pools. SA-MP ids are mapped through the SA-MP wrappers'
/// game pointers and the SA-MP pools' <c>Find</c>/<c>GetId</c>. Main thread only.
/// </summary>
public sealed unsafe partial class SFEntities : ISFEntities
{
    private static bool SampPoolsReady => CNetGame.TryGetPools(out _);

    public bool TryGetLocalPlayer(out SFEntityRef entity)
    {
        return TryCreateRef(SFEntityKind.Ped, SF.Players.Local.Ped?.GamePedPointer ?? 0, out entity);
    }

    public bool TryGetPlayer(ushort playerId, out SFEntityRef entity)
    {
        if (SampPoolsReady && playerId == CPlayerPool.Instance.LocalPlayerId)
        {
            return TryGetLocalPlayer(out entity);
        }

        nint gamePed = SF.Players.TryGetRemote(playerId, out SFRemotePlayer player) ? player.Ped?.GamePedPointer ?? 0 : 0;
        return TryCreateRef(SFEntityKind.Ped, gamePed, out entity);
    }

    public bool TryGetVehicle(ushort vehicleId, out SFEntityRef entity)
    {
        nint gameVehicle = SF.Vehicles.TryGet(vehicleId, out SFVehicle vehicle) ? vehicle.GameVehiclePointer : 0;
        return TryCreateRef(SFEntityKind.Vehicle, gameVehicle, out entity);
    }

    public bool TryGetObject(ushort objectId, out SFEntityRef entity)
    {
        nint gameObject = SF.Pools.Objects.TryGet(objectId, out SFObject obj) ? obj.GamePointer : 0;
        return TryCreateRef(SFEntityKind.Object, gameObject, out entity);
    }

    public bool TryGetActor(ushort actorId, out SFEntityRef entity)
    {
        nint gamePed = SF.Pools.Actors.TryGet(actorId, out SFActor actor) ? actor.GamePedPointer : 0;
        return TryCreateRef(SFEntityKind.Ped, gamePed, out entity);
    }

    public bool TryGetPlayerId(SFEntityRef entity, out ushort playerId)
    {
        playerId = ushort.MaxValue;
        GtaEntity* native = Resolve(entity);
        if (native is null || !SampPoolsReady)
        {
            return false;
        }

        if ((nint)native == (SF.Players.Local.Ped?.GamePedPointer ?? 0))
        {
            playerId = CPlayerPool.Instance.LocalPlayerId;
            return true;
        }

        ushort id = CPlayerPool.Instance.Find((nint)native);
        return TryAcceptId(id, SampOffsets.CPlayerPool.MaxPlayers, out playerId);
    }

    public bool TryGetVehicleId(SFEntityRef entity, out ushort vehicleId)
    {
        GtaEntity* native = Resolve(entity);
        if (native is null || entity.Kind != SFEntityKind.Vehicle || !SampPoolsReady)
        {
            vehicleId = ushort.MaxValue;
            return false;
        }

        return TryAcceptId(CVehiclePool.Instance.Find((nint)native), SampOffsets.CVehiclePool.MaxVehicles, out vehicleId);
    }

    public bool TryGetObjectId(SFEntityRef entity, out ushort objectId)
    {
        GtaEntity* native = Resolve(entity);
        if (native is null || entity.Kind != SFEntityKind.Object || !SampPoolsReady)
        {
            objectId = ushort.MaxValue;
            return false;
        }

        int id = CObjectPool.Instance.GetId((nint)native);
        return TryAcceptId(id is >= 0 and <= ushort.MaxValue ? (ushort)id : ushort.MaxValue, SampOffsets.CObjectPool.MaxObjects, out objectId);
    }

    public bool TryGetActorId(SFEntityRef entity, out ushort actorId)
    {
        GtaEntity* native = Resolve(entity);
        if (native is null || entity.Kind != SFEntityKind.Ped || !SampPoolsReady)
        {
            actorId = ushort.MaxValue;
            return false;
        }

        return TryAcceptId(CActorPool.Instance.Find((nint)native), SampOffsets.CActorPool.MaxActors, out actorId);
    }

    public bool Exists(SFEntityRef entity) => Resolve(entity) is not null;

    public bool TryGetSnapshot(SFEntityRef entity, out SFEntitySnapshot snapshot)
    {
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
        GtaEntity* native = Resolve(entity);
        if (native is null)
        {
            return false;
        }

        native->Teleport(position, resetRotation: false);
        return true;
    }

    public bool SetHeading(SFEntityRef entity, float heading)
    {
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

    private static bool TryAcceptId(ushort id, int maxExclusive, out ushort accepted)
    {
        accepted = id;
        return id < maxExclusive;
    }
}
