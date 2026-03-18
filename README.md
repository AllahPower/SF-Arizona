# SF-Arizona

Fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), adapted for **Arizona RP** on **SA-MP 0.3.7 R3-1** with **SAMPFUNCS v5.5.0 rel.22**.

## Version

Current release: **3.0.0-arizona**

## Highlights

- NativeAOT `win-x86` build published as a single `SF.asi`
- `MinHook.NET`-based hook installation with trampoline calls (zero-overhead original function invocation)
- RakNet packet interception layer for both incoming and outgoing RPC
- `BitStreamReader` for managed bit-level parsing of RakNet payloads (CP1251)
- Async RPC streaming API (`SF.Rpc.Stream`, `SF.Rpc.StreamOutgoing`) for module integration
- Structured SAMP offsets (`SampOffsets.cs`) with full RakClient vtable mapping (55 entries)
- Live pointer resolution for `CChat`, `CDialog`, `CInput`, `CNetGame`, and other SAMP singletons
- Local command interception via `CInput::Send` hook with duplicate suppression
- Dialog flow isolation to reduce collisions with external mods
- Background-threaded structured logging (`SFLog`)

## Architecture

### RPC Pipeline

```
Incoming: samp.dll HandleRpcPacket (0x3A6A0)
  -> IncomingRpcPacketHook (MinHook trampoline)
  -> TryExtractSubscribedRpc (RakNet wire format parsing)
  -> SFBootstrap.EnqueueIncomingRpc (ConcurrentQueue)
  -> ProcessIncomingRpcBatch (main thread, max 24/tick)
  -> RpcHandlerManager.DispatchIncoming -> subscribers

Outgoing: samp.dll RakClient::RPC (0x33EE0)
  -> OutgoingRpcPacketHook (MinHook trampoline)
  -> SFBootstrap.EnqueueOutgoingRpc (ConcurrentQueue)
  -> ProcessOutgoingRpcBatch (main thread, max 24/tick)
  -> OutgoingRpcManager.Dispatch -> subscribers
```

### Module System

Modules implement `ISFModule` with `RunAsync(CancellationToken)`. Register in `Program.Main`, managed by `SFModuleContainer` with `/sfs` in-game command.

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

---

## TODO / Roadmap

- [x] **Arizona Port:** Core offsets and SA:MP class access migrated to Arizona RP 0.3.7 R3-1
- [x] **Hook Backend:** Old manual trampoline helper replaced with `MinHook.NET`
- [x] **Command Interception:** Local commands handled through `CInput::Send` before server dispatch
- [x] **Dialog Isolation:** SF dialogs moved away from conflicting IDs and filtered from foreign mod dialogs
- [x] **General Logging:** Centralized runtime logging added to `sf_arz.log`
- [x] **RPC Interception:** Incoming and outgoing RPC packet hooks with managed BitStream parsing
- [x] **RPC Abstraction:** Subscribe/Stream API for typed RPC handlers (`RpcHandlerManager`, `OutgoingRpcManager`)
- [x] **Offset Registry:** Structured `SampOffsets` with CNetGame, CChat, CInput, RakClient vtable, RPC routes
- [ ] **.NET Loader:** Turn SF-Arizona into a managed loader for external .NET libraries, closer to MoonLoader for C# plugins
- [ ] **Plugin ABI:** Define a stable module/plugin ABI for hooks, commands, dialogs, tasks, and lifecycle callbacks
- [ ] **Hook Hardening:** Finish resilience against chained hooks, foreign trampolines, and NativeAOT edge cases
- [ ] **API Expansion:** Expand access to pools, dialogs, textdraws, and other gameplay systems
- [ ] **Compatibility Layer:** Improve coexistence with Arizona modpacks, ASI plugins, and SAMPFUNCS-based addons
- [ ] **Portability:** Document supported client builds, offsets, and the porting workflow for future Arizona updates

---

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) for the original project
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [DarkP1xel/SAMP-API](https://github.com/DarkP1xel/SAMP-API) for reversed classes and offsets
- [RakHook](https://github.com/imring/RakHook) and [RakLua](https://github.com/Northn/RakLua) for RakNet hooking references
- [SAMPFUNCS](https://www.blast.hk/threads/17/) for inspiration and API ideas
- [blast.hk forum](https://www.blast.hk/) for GTA SA:MP reverse-engineering discussions
