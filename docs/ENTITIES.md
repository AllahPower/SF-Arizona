# World entities

`ISF.Entities` (`ISFEntities`, namespace `SFSharp.Abstractions.Game`) reads and changes GTA world
entities: peds, vehicles and objects. It works on the game engine's own pools, so it also sees
entities that have no SA-MP id, such as peds and objects created by game scripts or other mods.

Every member touches game memory. Call it only from the main game thread: a module with
`ExecutionModel = ModuleExecutionModel.MainThread`, a chat command, or an entity event handler.

## Addressing

An entity is an `SFEntityRef(Kind, Handle)`. `Handle` is the GTA pool handle, the same value GTA
scripts use: `(slot << 8) + generation`. When an entity is deleted and its slot is reused, the
generation changes, so an old reference stops resolving instead of pointing at the new entity.
`Exists` and every other member report `false` for such a reference.

Server-owned entities are resolved from their SA-MP ids and back:

```csharp
ISFEntities entities = context.SF.Entities;

if (entities.TryGetVehicle(vehicleId, out SFEntityRef vehicle)
    && entities.TryGetSnapshot(vehicle, out SFEntitySnapshot snapshot)
    && snapshot.Driver is { } driver
    && entities.TryGetPlayerId(driver, out ushort driverId))
{
    context.SF.Chat.Add($"vehicle {vehicleId} is driven by player {driverId}");
}
```

| SA-MP id to entity | Entity to SA-MP id |
|---|---|
| `TryGetLocalPlayer`, `TryGetPlayer` | `TryGetPlayerId` (local and remote players) |
| `TryGetVehicle` | `TryGetVehicleId` |
| `TryGetObject` | `TryGetObjectId` |
| `TryGetActor` | `TryGetActorId` |

`Enumerate(kind)` lists every live entity of one kind.

## Reading

`TryGetSnapshot` copies the entity state:

- model, interior, position;
- heading in degrees, 0 facing north (+Y), counter-clockwise;
- world matrix with rows right, forward, up, translation;
- velocity and turn speed in game units per physics step, 50 steps per second;
- health; armour for peds;
- the vehicle a ped sits in, and a vehicle's driver.

## Writing

`Teleport`, `SetHeading`, `SetVelocity`, `SetTurnSpeed`, `SetHealth` and `SetArmour` (peds only)
return `false` when the entity does not exist.

- `Teleport` calls the game's own virtual `Teleport`.
- `Teleport` and `SetHeading` on a ped sitting in a vehicle return `false`, because the game moves or turns
  only the ped and leaves its seat untouched. Move or turn the vehicle instead.

Writes change the local game only:

- **Remote players and their vehicles.** SA-MP sync packets overwrite their position, speed and health
  many times per second, so writes to them do not last.
- **The local player and their vehicle.** These writes are sent to the server with the next sync packet.
  A server with anti-cheat, such as Arizona RP, may treat a teleport or healing of the local player as
  cheating and punish the account.

## Events

Both events run synchronously on the main thread. The first subscription installs the hooks they
need, so subscribe from the main thread. A plugin should register the subscription with the module
context so it is released when the module stops:

```csharp
context.RegisterDisposable(context.SF.Entities.OnSampEntityChanged(e =>
{
    if (e.Type == SFSampEntityType.Player && e.Change == SFSampEntityChange.StreamedIn)
    {
        context.SF.Chat.Add($"player {e.SampId} streamed in");
    }
}));
```

### `OnSampEntityChanged`

Raised on changes in the SA-MP pools.

| Change | Raised for | When |
|---|---|---|
| `Created` | player connect, vehicle, object, actor | after the pool accepted it |
| `Deleted` | player disconnect, vehicle, object, actor | before removal; `Entity` is still alive |
| `StreamedIn` | remote player ped spawned | after the ped exists |
| `StreamedOut` | remote player ped removed | before removal |

A connecting player has no ped yet, so `Entity` is `null` for player `Created`.

### `OnDamage`

Raised before the engine applies damage. A handler may lower or raise `Amount`, or set `Cancel`.

| Target | Source in the game | Cancel |
|---|---|---|
| ped, including the local player | `CPedDamageResponseCalculator::ComputeDamageResponse` | the damage amount is set to 0 |
| vehicle, weapon hits only | `CVehicle::InflictDamage` | the hit is skipped |
| object | `CObject::ObjectDamage` | the hit is skipped |

Collision damage reaches vehicles through the `VehicleDamage` overrides and is not reported.

Cancelling damage to the local player keeps their local health, which SA-MP then syncs to the
server. Whether samp.dll still reports the hit itself, for example with its damage RPC, depends on
where it observes the hit. This has not been verified in game yet.

## Sources

Engine layouts and functions were checked in IDA against gta_sa.exe 1.0 US
(MD5 `84a781f30fac7eba030af2497da86930`). SA-MP entry points follow SAMP-API `0.3.7-R3-1`. Their
argument sizes were checked against the Arizona launcher samp.dll (MD5
`e672e4723da63c93e984912f1193a798`).
