using SFSharp.Abstractions.Gta;

namespace SFSharp.Abstractions.Samp;

/// <summary>SA-MP ID/GTA handle mappings. All operations require the game thread.</summary>
public interface ISFSampEntities
{
    bool TryGetLocalPlayer(out SFEntityRef entity);
    bool TryGetPlayer(ushort playerId, out SFEntityRef entity);
    bool TryGetVehicle(ushort vehicleId, out SFEntityRef entity);
    bool TryGetObject(ushort objectId, out SFEntityRef entity);
    bool TryGetActor(ushort actorId, out SFEntityRef entity);
    bool TryGetPlayerId(SFEntityRef entity, out ushort playerId);
    bool TryGetVehicleId(SFEntityRef entity, out ushort vehicleId);
    bool TryGetObjectId(SFEntityRef entity, out ushort objectId);
    bool TryGetActorId(SFEntityRef entity, out ushort actorId);
    /// <summary>Pool lifecycle events. Register the returned subscription with the module context.</summary>
    IDisposable OnSampEntityChanged(Action<SFSampEntityEvent> handler);
}
