# SF-Arizona

Fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), adapted for **Arizona RP** on **SA-MP 0.3.7 R3-1** with **SAMPFUNCS v5.5.0 rel.22**.

## Version

Current release: **3.1.1**

## Highlights

- NativeAOT `win-x86` build published as a single `SF.asi`
- `MinHook.NET`-based hook installation with trampoline calls
- RakNet interception layer for incoming and outgoing RPC/raw packets
- Managed `BitStreamReader` for RakNet payload parsing with CP1251 string decoding
- Async streaming API for RPC (`SF.Rpc`) and packets (`SF.Packets`) for modules
- Structured SAMP offsets in `SampOffsets.cs`
- Live pointer resolution for `CChat`, `CDialog`, `CInput`, `CNetGame`, and other SAMP singletons
- Local command interception via `CInput::Send`
- Background worker logger writing to `sf_arz.log`
- Arizona-specific transport definitions for raw packets `220` and `221`

## Arizona Transport

Arizona RP uses custom payloads on top of raw RakNet packets:

- `PacketId.ArizonaCef = 220`
- `PacketId.ArizonaCefEx = 221`

These packets are now described in code by:

- `src/Interop/RakNet/Arizona/ArizonaPacketId.cs`
- `src/Interop/RakNet/Arizona/ArizonaPacketIdEx.cs`
- `src/Interop/RakNet/Arizona/ArizonaPacket.cs`

### Packet 220

`Packet 220` uses a `uint8` sub-id as the first byte of the payload.
It is primarily used for Arizona UI/CEF and gameplay-side custom events, for example:

- CEF display/event pipe
- chat mode/group configuration
- cursor and HUD toggles
- custom map/icon and vehicle visual state updates
- client-side interface callbacks back to the server

### Packet 221

`Packet 221` uses a `uint16` sub-id as the first 2 bytes of the payload.
It is primarily used by Arizona's bot/NPC transport, for example:

- bot stream in/out
- bot sync and movement
- bot weapon and animation control
- attached objects and chat bubbles
- outgoing bot sync/damage reports

## Architecture

### RPC Pipeline

```text
Incoming: samp.dll HandleRpcPacket (0x3A6A0)
  -> IncomingRpcPacketHook
  -> SFBootstrap.EnqueueIncomingRpc
  -> ProcessIncomingRpcBatch (main thread, batched)
  -> RpcHandlerManager.DispatchIncoming -> subscribers

Outgoing: samp.dll RakClient::RPC (0x33EE0)
  -> OutgoingRpcPacketHook
  -> SFBootstrap.EnqueueOutgoingRpc
  -> ProcessOutgoingRpcBatch (main thread, batched)
  -> OutgoingRpcManager.Dispatch -> subscribers
```

### Packet Pipeline

```text
Incoming: RakClient::Receive (vtable[8], resolved at runtime)
  -> IncomingPacketHook
  -> SFBootstrap.EnqueueIncomingPacket
  -> ProcessIncomingPacketBatch (main thread, batched)
  -> IncomingPacketManager.Dispatch -> subscribers

Outgoing: RakClient::Send (vtable[7], resolved at runtime)
  -> OutgoingPacketHook
  -> SFBootstrap.EnqueueOutgoingPacket
  -> ProcessOutgoingPacketBatch (main thread, batched)
  -> OutgoingPacketManager.Dispatch -> subscribers
```

### Module System

Modules implement `ISFModule` with `RunAsync(CancellationToken)`.
Register them in `Program.Main`; lifecycle is managed by `SFModuleContainer` and the in-game `/sfs` command.

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

- [x] **Arizona Port:** Core offsets and SA:MP class access migrated to Arizona RP 0.3.7 R3-1
- [x] **Hook Backend:** Manual trampolines replaced with `MinHook.NET`
- [x] **Command Interception:** Local commands handled through `CInput::Send`
- [x] **Dialog Isolation:** SF dialogs moved away from conflicting IDs and filtered from foreign mod dialogs
- [x] **General Logging:** Centralized runtime logging added to `sf_arz.log`
- [x] **RPC Interception:** Incoming and outgoing RPC packet hooks with managed bitstream parsing
- [x] **Packet Interception:** Raw packet hooks via RakClient send/receive interception
- [x] **RPC Abstraction:** Subscribe/Stream API for typed RPC handlers
- [x] **Packet Abstraction:** Subscribe/Stream API for raw packets
- [x] **Arizona Custom Transport:** Packet `220/221` enums and payload records added for CEF and bot traffic
- [ ] **.NET Loader:** Turn SF-Arizona into a managed loader for external .NET libraries, closer to MoonLoader for C# plugins
- [ ] **Plugin ABI:** Define a stable module/plugin ABI for hooks, commands, dialogs, packets, and lifecycle callbacks
- [ ] **Hook Hardening:** Finish resilience against chained hooks, foreign trampolines, and NativeAOT edge cases
- [ ] **API Expansion:** Expand access to pools, dialogs, textdraws, and other gameplay systems
- [ ] **Compatibility Layer:** Improve coexistence with Arizona modpacks, ASI plugins, and SAMPFUNCS-based addons
- [ ] **Portability:** Document supported client builds, offsets, and the porting workflow for future Arizona updates

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) for the original project
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [DarkP1xel/SAMP-API](https://github.com/DarkP1xel/SAMP-API) for reversed classes and offsets
- [RakHook](https://github.com/imring/RakHook) and [RakLua](https://github.com/Northn/RakLua) for RakNet hooking references
- [SAMPFUNCS](https://www.blast.hk/threads/17/) for inspiration and API ideas
- [blast.hk forum](https://www.blast.hk/) for GTA SA:MP reverse-engineering discussions
