using System.Numerics;

namespace SFSharp.Abstractions.Game;

/// <summary>
/// Read and write access to GTA world entities (peds, vehicles, objects), addressed by GTA pool handle
/// and resolvable from SA-MP player, vehicle, object and actor ids.
/// </summary>
/// <remarks>
/// NOT thread-safe. Every member touches game memory and must be called from the main game thread.
/// Writes act on the local game only: SA-MP sync keeps overwriting remote players and their vehicles,
/// and writes to the local player are sent to the server with the next sync packet.
/// </remarks>
public interface ISFEntities
{
    bool TryGetLocalPlayer(out SFEntityRef entity);
    bool TryGetPlayer(ushort playerId, out SFEntityRef entity);
    bool TryGetVehicle(ushort vehicleId, out SFEntityRef entity);
    bool TryGetObject(ushort objectId, out SFEntityRef entity);
    bool TryGetActor(ushort actorId, out SFEntityRef entity);

    /// <summary>SA-MP id of a remote or local player's ped.</summary>
    bool TryGetPlayerId(SFEntityRef entity, out ushort playerId);
    bool TryGetVehicleId(SFEntityRef entity, out ushort vehicleId);
    bool TryGetObjectId(SFEntityRef entity, out ushort objectId);
    bool TryGetActorId(SFEntityRef entity, out ushort actorId);

    bool Exists(SFEntityRef entity);
    bool TryGetSnapshot(SFEntityRef entity, out SFEntitySnapshot snapshot);

    /// <summary>Every live entity of one kind, including those without a SA-MP id.</summary>
    IReadOnlyList<SFEntityRef> Enumerate(SFEntityKind kind);

    /// <summary>
    /// Moves the entity through the game's own virtual <c>Teleport</c>. Returns false for a ped sitting in a
    /// vehicle: <c>CPed::Teleport</c> moves only the ped and leaves its vehicle state untouched, so teleport the vehicle.
    /// </summary>
    bool Teleport(SFEntityRef entity, Vector3 position);

    /// <summary>Sets the heading in degrees; see <see cref="SFEntitySnapshot.Heading"/>.</summary>
    bool SetHeading(SFEntityRef entity, float heading);

    bool SetVelocity(SFEntityRef entity, Vector3 velocity);
    bool SetTurnSpeed(SFEntityRef entity, Vector3 turnSpeed);
    bool SetHealth(SFEntityRef entity, float health);

    /// <summary>Sets ped armour; returns false for vehicles and objects.</summary>
    bool SetArmour(SFEntityRef entity, float armour);

    /// <summary>
    /// SA-MP pool changes of players, vehicles, objects and actors. The first subscription installs the
    /// pool hooks, so subscribe from the main thread. Register the result with
    /// <see cref="IModuleContext.RegisterDisposable"/> so it is released when the module stops.
    /// </summary>
    IDisposable OnSampEntityChanged(Action<SFSampEntityEvent> handler);

    /// <summary>
    /// Damage to peds, vehicles and objects before the engine applies it. The first subscription installs
    /// the damage hooks, so subscribe from the main thread.
    /// </summary>
    IDisposable OnDamage(Action<SFDamageEvent> handler);
}
