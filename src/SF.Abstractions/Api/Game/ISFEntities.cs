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

    /// <summary>Moves the entity through the game's own <c>Teleport</c>, which also handles peds in vehicles.</summary>
    bool Teleport(SFEntityRef entity, Vector3 position);

    /// <summary>Sets the heading in degrees; see <see cref="SFEntitySnapshot.Heading"/>.</summary>
    bool SetHeading(SFEntityRef entity, float heading);

    bool SetVelocity(SFEntityRef entity, Vector3 velocity);
    bool SetTurnSpeed(SFEntityRef entity, Vector3 turnSpeed);
    bool SetHealth(SFEntityRef entity, float health);

    /// <summary>Sets ped armour; returns false for vehicles and objects.</summary>
    bool SetArmour(SFEntityRef entity, float armour);
}
