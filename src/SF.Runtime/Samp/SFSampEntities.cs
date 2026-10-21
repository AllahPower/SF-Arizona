namespace SFSharp.Runtime.Samp;

/// <summary>Explicit bridge from SA-MP pool IDs to live GTA entities.</summary>
public sealed unsafe partial class SFSampEntities(SFPlayers players, SFVehicles vehicles, SFGamePools pools) : ISFSampEntities
{
    private static bool SampPoolsReady => CNetGame.TryGetPools(out _);

    public bool TryGetLocalPlayer(out SFEntityRef entity) =>
        SFEntities.TryCreateRef(SFEntityKind.Ped, SampPoolsReady ? players.Local.Ped?.GamePedPointer ?? 0 : 0, out entity);

    public bool TryGetPlayer(ushort id, out SFEntityRef entity)
    {
        if (SampPoolsReady && id == CPlayerPool.Instance.LocalPlayerId)
            return TryGetLocalPlayer(out entity);
        nint pointer = SampPoolsReady && players.TryGetRemote(id, out SFRemotePlayer player) ? player.Ped?.GamePedPointer ?? 0 : 0;
        return SFEntities.TryCreateRef(SFEntityKind.Ped, pointer, out entity);
    }

    public bool TryGetVehicle(ushort id, out SFEntityRef entity) => SFEntities.TryCreateRef(SFEntityKind.Vehicle,
        SampPoolsReady && vehicles.TryGet(id, out SFVehicle vehicle) ? vehicle.GameVehiclePointer : 0, out entity);

    public bool TryGetObject(ushort id, out SFEntityRef entity) => SFEntities.TryCreateRef(SFEntityKind.Object,
        SampPoolsReady && pools.Objects.TryGet(id, out SFObject obj) ? obj.GamePointer : 0, out entity);

    public bool TryGetActor(ushort id, out SFEntityRef entity) => SFEntities.TryCreateRef(SFEntityKind.Ped,
        SampPoolsReady && pools.Actors.TryGet(id, out SFActor actor) ? actor.GamePedPointer : 0, out entity);

    public bool TryGetPlayerId(SFEntityRef entity, out ushort id)
    {
        id = ushort.MaxValue;
        GtaEntity* native = entity.Kind == SFEntityKind.Ped ? SFEntities.Resolve(entity) : null;
        if (native is null || !SampPoolsReady) return false;
        if ((nint)native == (players.Local.Ped?.GamePedPointer ?? 0))
        {
            id = CPlayerPool.Instance.LocalPlayerId;
            return true;
        }
        return Accept(CPlayerPool.Instance.Find((nint)native), SampOffsets.CPlayerPool.MaxPlayers, out id);
    }

    public bool TryGetVehicleId(SFEntityRef entity, out ushort id)
    {
        id = ushort.MaxValue;
        GtaEntity* native = entity.Kind == SFEntityKind.Vehicle ? SFEntities.Resolve(entity) : null;
        return native is not null && SampPoolsReady && Accept(CVehiclePool.Instance.Find((nint)native), SampOffsets.CVehiclePool.MaxVehicles, out id);
    }

    public bool TryGetObjectId(SFEntityRef entity, out ushort id)
    {
        id = ushort.MaxValue;
        GtaEntity* native = entity.Kind == SFEntityKind.Object ? SFEntities.Resolve(entity) : null;
        return native is not null && SampPoolsReady && Accept(CObjectPool.Instance.GetId((nint)native), SampOffsets.CObjectPool.MaxObjects, out id);
    }

    public bool TryGetActorId(SFEntityRef entity, out ushort id)
    {
        id = ushort.MaxValue;
        GtaEntity* native = entity.Kind == SFEntityKind.Ped ? SFEntities.Resolve(entity) : null;
        return native is not null && SampPoolsReady && Accept(CActorPool.Instance.Find((nint)native), SampOffsets.CActorPool.MaxActors, out id);
    }

    internal static bool Accept(int value, int capacity, out ushort id)
    {
        bool valid = value >= 0 && value < capacity && value < ushort.MaxValue;
        id = valid ? (ushort)value : ushort.MaxValue;
        return valid;
    }
}
