# GTA And SA-MP Services

The host still requires SA-MP R3-1. Separating its services does not introduce a GTA-only loader.
All host projects target .NET 10 and stable C# 14.

## Boundaries

- `ISF.Gta`: GTA entities, camera, loaded-world collision and game clock.
- `ISF.Samp`: chat, dialogs, players, network pools, raw/parsed traffic, network camera state and entity mappings.
- Host-level `MainThread`, `Keyboard`, `Modules` and `Loading` are not network services.

GTA contracts live in `SFSharp.Abstractions.Gta`. SA-MP pool and bridge contracts live in
`SFSharp.Abstractions.Samp`; transport contracts retain `SFSharp.Abstractions.Network`.
SA-MP IDs may resolve to a GTA `SFEntityRef`; the GTA implementation never resolves a server ID.
Network-owned state can overwrite local entity writes on the next synchronization update.
All plugins must be rebuilt against the changed API; the old root facades are not aliases.

## Main Thread

```csharp
var position = await context.InvokeOnMainThreadAsync(() =>
{
    var sf = context.SF;
    return sf.Samp.Entities.TryGetLocalPlayer(out var ped)
        && sf.Gta.Entities.TryGetSnapshot(ped, out var snapshot)
        ? snapshot.Position : (System.Numerics.Vector3?)null;
}, cancellationToken);
```

Only the delegate executes on the game thread. Code after `await` is not implicitly moved there.
Module-context invocation combines caller cancellation with module lifetime. Cancellation before
execution drops the callback reference immediately; it cannot interrupt an already executing delegate.
For teardown after module cancellation, use `context.SF.MainThread.InvokeAsync(cleanup)` and await it.
The WinMainLoop message pump drains work; it is not a simulation tick.

## Tick And World Queries

`Gta.Clock.Subscribe` runs after Idle's Process call on the game thread. Register the returned
disposable with the module context. `Current` and native APIs are game-thread-only.
Ticks include paused frames; inspect `IsPaused`. Delta is based on game milliseconds, with zero
for the first frame, pause and timer reset. `EngineTimeStep` remains in native 1/50-second units.
Repeated frame counters are not published twice. A lost Process call-site reports unavailable;
the host does not repatch another extension or substitute message-loop timing.

`Gta.World.TryRaycast` returns position, normal, B-surface and an optional entity handle. Buildings
and dummies can be hit without a pool handle. Options map directly to native entity and visibility
flags. `TryFindGround` searches building collision below the given point, not arbitrary terrain
streamed from elsewhere. No hit is not evidence that unloaded space is empty.

`Gta.Camera.TryWorldToScreen` uses the current GTA camera and native near/far clipping. Returned
pixels can be outside the viewport. SFUI's render-camera snapshot remains separate: it describes
the actual D3D world pass, which can differ from simulation-time camera state.
Queries return false before the playing stage; non-finite coordinates and wrong-thread calls are rejected.

## Native Evidence And Acceptance

Read-only IDA analysis on 2026-10-21 used EXE MD5 `84a781f30fac7eba030af2497da86930`, base `0x400000`.
RVAs: Idle Process call `0x13E981`, CGame::Process `0x13BEE0`, CWorld::ProcessLineOfSight `0x16BA00`,
FindGroundZFor3DCoord `0x1696C0`, CSprite::CalcScreenCoors `0x30CE30`. Calls are x86 cdecl;
ground height returns through x87, query booleans use byte values. CColPoint is 44 bytes.

TheCamera RVA is `0x76F028`; active index `+0x59` selects one of three CCam slots at `+0x174`,
stride `0x238`. Mode is ushort at `+0x0C`, FOV float `+0xB4`, front/source/up at `+0x190/+0x19C/+0x1B4`.
Camera widescreen is byte `+0x3D`, shake float `+0x12C`, fade alpha float `+0xBFC`.
Fade status follows GetScreenFadeStatus: 0 clear, 1 transitioning, 2 black.

The loading-hook ABI was checked against instructions and callers in the same IDB on 2026-10-21.
`InitialiseCoreDataAfterRW` RVA `0x1BFA90` is cdecl with no arguments and returns a boolean in AL
(`xor al,al` on failure). `Init1/2/3` RVAs `0x1BF840/0x1BA1A0/0x1BA400` are called with a
`const char*` argument and return their success value in AL. Decompiler return types in this IDB
are inaccurate here; callers, argument pushes and return instructions were used instead.

Core hook chains publish immutable registration snapshots and reuse per-thread invocation frames.
The original is called at most once; an observer failing after `next()` preserves the completed result,
including an explicit suppression. Native callbacks contain managed exceptions, and observer failures
do not retry a completed receive/send or game operation. SA-MP chat/dialog originals use MinHook
trampolines rather than disabling a process-wide detour around each call.

Core native hooks are resident. Disabling a hook does not reclaim its trampoline or callback, and
does not authorize unloading a native provider. Replacement dialog strings stay alive until the next
hooked dialog; disabling cannot free buffers a still-visible native dialog may reference. Dynamic
native-provider reclamation and public hook-health snapshots remain outside this first reliability wave.

These are static binary/layout observations, not proof on every modified EXE. SFUI retains its
separate verified graphics profile; its existing fingerprint must not be replaced with this IDB's hash.
In-game gates: clean R3 startup, reconnect, pause, all camera modes, collision hit/miss, clipping,
tick chain ownership and repeated SFUI/module stop/start. Build and unit tests cannot replace them.

Future work, outside this change: shared input ownership, model streaming leases, local entity
creation and ped tasks/animations. Renderer/ImGui and client-specific integrations remain extensions.
