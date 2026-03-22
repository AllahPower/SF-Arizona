# SF-Arizona

Fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), adapted for **Arizona RP** on **SA-MP 0.3.7 R3-1** with **SAMPFUNCS v5.5.0 rel.22**.

## Version

Current release: **3.1.9**

## Highlights

- NativeAOT `win-x86` build published as a single `SF.asi`
- `MinHook.NET` based hook installation with trampoline calls
- RakNet interception layer for incoming and outgoing RPC/raw packets
- Expanded native pool models for players, vehicles, objects, actors, pickups, menus, textdraws, gang zones, and labels
- Runtime module system with metadata, lifecycle tracking, telemetry, built-in module logger, and `/sfs` dashboard
- Color builder API via `SFColor` and `SFColors` for chat and dialog styling
- Local command interception via `CInput::Send`
- Background worker logger writing to `sf_arz.log`

## Architecture

### RPC Pipeline

```text
Incoming: samp.dll HandleRpcPacket
  -> IncomingRpcPacketHook
  -> SFBootstrap.EnqueueIncomingRpc
  -> main-thread batch dispatcher
  -> RpcHandlerManager / subscribers

Outgoing: samp.dll RakClient::RPC
  -> OutgoingRpcPacketHook
  -> SFBootstrap.EnqueueOutgoingRpc
  -> main-thread batch dispatcher
  -> OutgoingRpcManager / subscribers
```

### Packet Pipeline

```text
Incoming: RakClient::Receive
  -> IncomingPacketHook
  -> SFBootstrap.EnqueueIncomingPacket
  -> main-thread batch dispatcher
  -> IncomingPacketManager / subscribers

Outgoing: RakClient::Send
  -> OutgoingPacketHook
  -> SFBootstrap.EnqueueOutgoingPacket
  -> main-thread batch dispatcher
  -> OutgoingPacketManager / subscribers
```

## Module System

Modules are registered in [`src/Modules/Program.cs`](src/Modules/Program.cs) through `SFModuleContainer.RegisterModule<T>()`.

Each module should:

- declare metadata with `[SFModule("id", "DisplayName", ...)]`
- inherit `SFModuleBase`
- implement `ExecuteAsync(CancellationToken cancellationToken)`

Minimal example:

```csharp
[SFModule("example", "Example", Description = "Demo module", Order = 100)]
public sealed class ExampleModule : SFModuleBase
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Log.LogInformation("started");
        Context.SetDetail("mode", "idle");

        while (!cancellationToken.IsCancellationRequested)
        {
            using (ModuleLoopScope _ = Context.TrackLoop("tick"))
            {
                Context.Heartbeat("running");
            }

            await Task.Yield();
        }
    }
}
```

Available module features:

- `Log.Info/Warn/Error(...)` with automatic module prefix
- `Context.Heartbeat(...)` and `Context.ReportActivity(...)`
- `Context.IncrementCounter(...)` and `Context.SetDetail(...)`
- `Context.TrackLoop(...)` for lightweight load metrics
- `Context.RegisterChatCommand(...)` and `Context.RegisterDisposable(...)`
- `Context.SwitchToMainThreadAsync()` and `Context.RunBackground(...)`
- `/sfs`, `/sfs status`, `/sfs info <id>`, `/sfs start <id>`, `/sfs stop <id>`, `/sfs restart <id>`

The `/sfs` dialog shows state, uptime, load, counters, details, last activity, stop reason, and management actions for every registered module.

## Colors

Use [`src/SF/SFColor.cs`](src/SF/SFColor.cs) for SA-MP text colors instead of raw `"{RRGGBB}"` strings.

```csharp
SFColor accent = SFColors.Cyan | SFColors.Blue;
string title = accent.Apply("Network Debugger");
uint chatColor = SFColors.Green;
```

## Prerequisites

- GTA San Andreas 1.0 US
- Arizona RP client
- SA:MP 0.3.7 R3-1
- SAMPFUNCS v5.5.0 rel.22
- .NET SDK 10.0 for local builds

## Build

```bash
dotnet build src/SF.csproj -c Release
dotnet publish src/SF.csproj -c Release
# Output: src/bin/Release/net10.0/win-x86/publish/SF.asi
```

## TODO / Roadmap

- [x] Core Arizona RP port for SA:MP 0.3.7 R3-1
- [x] RPC and raw packet interception
- [x] Expanded native gameplay pool abstractions
- [x] Runtime module metadata, lifecycle, telemetry, and `/sfs` management UI
- [x] Module-scoped logger contract via `SFModuleBase`
- [x] Shared SA-MP color builder via `SFColor` and `SFColors`
- [ ] Expand high-level `SF.*` wrappers over newly mapped native classes
- [ ] Add more module examples built on the new runtime contract
- [ ] Persist module settings such as autostart and per-module options
- [ ] Harden coexistence with foreign hooks, Arizona modpacks, and SAMPFUNCS add-ons
- [ ] Document supported client builds and the offset update workflow

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) for the original project
- [SAMP.Lua](https://github.com/THE-FYP/SAMP.Lua/) as a reference source for SA:MP RPC/packet layouts and event handling
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [DarkP1xel/SAMP-API](https://github.com/DarkP1xel/SAMP-API) for reversed classes and offsets
- [RakHook](https://github.com/imring/RakHook) and [RakLua](https://github.com/Northn/RakLua) for RakNet hooking references
- [SAMPFUNCS](https://www.blast.hk/threads/17/) for inspiration and API ideas
- [blast.hk forum](https://www.blast.hk/) for GTA SA:MP reverse-engineering discussions
