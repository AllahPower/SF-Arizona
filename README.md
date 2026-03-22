<div align="center">

# SF-Arizona

*A C# framework that brings the full SA-MP/GTA game environment into managed code - build game modules with a clean API, not raw memory hacks*

[![.NET](https://img.shields.io/badge/.NET_10-NativeAOT-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-win--x86-blue)](https://github.com/AllahPower/SF-Arizona)
[![Wiki](https://img.shields.io/badge/docs-Wiki-green?logo=github)](https://github.com/AllahPower/SF-Arizona/wiki)
[![SAMPFUNCS](https://img.shields.io/badge/SAMPFUNCS-v5.5.0-orange)](https://www.blast.hk/threads/17/)

</div>

---

## About

**SF-Arizona** is a C# framework that exposes the entire SA-MP/GTA San Andreas game environment through a high-level API abstraction layer. It targets **SA-MP 0.3.7 R3-1** on **Arizona RP** and compiles via NativeAOT into a single `SF.asi` plugin - no .NET runtime required on the target machine.

The core idea is simple: instead of writing raw memory patches and ASM hooks, you write **modules** - regular C# classes in the `src/Modules/` directory where you implement whatever game logic you need. The framework handles everything underneath: hooking into RakNet for network traffic, mapping native memory pools for players, vehicles, objects, and other game entities, providing structured events, and managing module lifecycles.

The project is a fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), rebuilt from the ground up for Arizona RP.

### Goals

- **Game environment in C#** - expose players, vehicles, objects, dialogs, chat, and all other SA-MP entities as typed, safe abstractions that module authors can use directly
- **Network layer access** - intercept and decode every RPC and raw packet in both directions, including Arizona-specific custom packets (ID 220/221), with zero-allocation `BitStreamReader` parsing
- **Module-first architecture** - everything the user builds lives in `src/Modules/`. A module is a self-contained C# class with full access to the game API, network events, native pools, chat commands, and background threading
- **Developer tooling** - built-in web traffic debugger, structured logging, telemetry, and the `/sfs` in-game dashboard for module management
- **Single-file delivery** - ship as one native DLL with no external dependencies thanks to .NET 10 NativeAOT

---

## Features

| Category | Description |
|---|---|
| **NativeAOT Build** | Compiled ahead-of-time to a single `SF.asi` - no .NET runtime installation required on the target machine |
| **RakNet Interception** | Full duplex hooking of `RakClient::RPC`, `RakClient::Send`, `RakClient::Receive`, and `HandleRpcPacket` via MinHook trampolines |
| **Arizona Packet Parsing** | Dedicated enum, model, and parser catalog for Arizona RP custom packets (Packet 220 sub-IDs, Packet 221) with `BitStreamReader` zero-allocation parsing |
| **Module System** | Attribute-based module registration with lifecycle management, telemetry, heartbeat tracking, counters, and the `/sfs` in-game dashboard |
| **Web Debugger** | Built-in ASP.NET Core Minimal API server on `localhost:7777` for live traffic inspection with filtering, stats, and color-coded packet views |
| **Native Pools** | Typed abstractions over SA-MP memory pools: players, vehicles, objects, actors, pickups, menus, textdraws, gang zones, and labels |
| **Color API** | `SFColor` and `SFColors` builder for SA-MP `{RRGGBB}` text styling with bitwise composition |
| **Command Interception** | Hook into `CInput::Send` for local command processing before packets are transmitted |
| **Structured Logging** | Background file logger writing to `sf_arz.log` with module-scoped prefixes via `Microsoft.Extensions.Logging` |

---

## Quick Start

### Prerequisites

| Requirement | Details |
|---|---|
| GTA San Andreas | Version **1.0 US** |
| Arizona RP | Client installed and configured |
| SA-MP | **0.3.7 R3-1** |
| SAMPFUNCS | **v5.5.0 rel.22** |
| .NET SDK | **10.0** (for building from source) |

### Build

```bash
# Debug build
dotnet build src/SF.csproj -c Debug

# Release build with NativeAOT publish
dotnet publish src/SF.csproj -c Release
```

Output: `src/bin/Release/net10.0/win-x86/publish/SF.asi`

### Install

Copy the published `SF.asi` into your GTA San Andreas game directory alongside `samp.dll`. The plugin is loaded automatically by SAMPFUNCS on game startup.

---

## Architecture

SF-Arizona intercepts network traffic at two levels: **RPC** (Remote Procedure Calls) and **raw packets**. Both pipelines follow the same pattern: hook the native function, enqueue the event, dispatch on the main thread.

### RPC Pipeline

```
Incoming                              Outgoing
────────                              ────────
samp.dll HandleRpcPacket              samp.dll RakClient::RPC
  → IncomingRpcPacketHook               → OutgoingRpcPacketHook
    → SFBootstrap.EnqueueIncomingRpc      → SFBootstrap.EnqueueOutgoingRpc
      → main-thread dispatcher              → main-thread dispatcher
        → RpcHandlerManager                   → OutgoingRpcManager
          → subscribers                         → subscribers
```

### Packet Pipeline

```
Incoming                              Outgoing
────────                              ────────
RakClient::Receive                    RakClient::Send
  → IncomingPacketHook                  → OutgoingPacketHook
    → SFBootstrap.EnqueueIncomingPacket   → SFBootstrap.EnqueueOutgoingPacket
      → main-thread dispatcher              → main-thread dispatcher
        → IncomingPacketManager               → OutgoingPacketManager
          → subscribers                         → subscribers
```

### Arizona Custom Packets

Arizona RP uses **Packet ID 220** as a multiplexed container. Each packet carries an inner `subId` byte that identifies the actual payload type. SF-Arizona maintains a full catalog of known sub-IDs in `EArizonaPacketId` with dedicated parsers for each in `ArizonaPacket.cs`, registered through `PacketParserCatalog`.

---

## Module System

Modules are the primary extension point. Each module is a self-contained unit with its own lifecycle, logging, telemetry, and chat command registration.

### Creating a Module

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

Register in `src/Modules/Program.cs`:

```csharp
container.RegisterModule<ExampleModule>();
```

### Module API

| Method | Purpose |
|---|---|
| `Log.LogInformation / Warn / Error` | Module-prefixed structured logging |
| `Context.Heartbeat(...)` | Report liveness to the dashboard |
| `Context.ReportActivity(...)` | Log a named activity event |
| `Context.IncrementCounter(...)` | Bump a named telemetry counter |
| `Context.SetDetail(...)` | Set a key-value detail visible in `/sfs` |
| `Context.TrackLoop(...)` | Measure loop iteration load |
| `Context.RegisterChatCommand(...)` | Bind an in-game `/command` |
| `Context.RegisterDisposable(...)` | Track disposable resources |
| `Context.SwitchToMainThreadAsync()` | Marshal execution to the game thread |
| `Context.RunBackground(...)` | Offload work to a background thread |

### Management Commands

| Command | Action |
|---|---|
| `/sfs` | Open the module dashboard dialog |
| `/sfs status` | Show all module states |
| `/sfs info <id>` | Detailed info for a specific module |
| `/sfs start <id>` | Start a stopped module |
| `/sfs stop <id>` | Stop a running module |
| `/sfs restart <id>` | Restart a module |

---

## Documentation

For detailed guides, API reference, and examples, visit the **[SF-Arizona Wiki](https://github.com/AllahPower/SF-Arizona/wiki)**.

---

## Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| [`MinHook.NET`](https://www.nuget.org/packages/MinHook.NET) | 1.1.2 | Function hooking with trampoline calls |
| [`Microsoft.Extensions.Logging.Abstractions`](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions) | 10.0.5 | Logging interfaces and abstractions |
| [`DllMain`](https://www.nuget.org/packages/DllMain) | 1.0.2 | Native DLL entry point for NativeAOT |

### Framework References

| Framework | Purpose |
|---|---|
| `Microsoft.AspNetCore.App` | Minimal API web server for the traffic debugger |

### Build Requirements

| Tool | Version |
|---|---|
| .NET SDK | 10.0+ |
| Target | `net10.0`, `win-x86`, NativeAOT |

---

## Project Structure

```
SF-Arizona/
├── src/
│   ├── SF.csproj                          # Project file (NativeAOT, win-x86)
│   ├── Bootstrap/                         # Application init and context setup
│   ├── Diagnostics/                       # Logging provider and file logger
│   ├── Interop/
│   │   ├── Classes/                       # Native SA-MP class wrappers
│   │   │   ├── CChat.cs                   #   Chat functions
│   │   │   ├── CDialog.cs                 #   Dialog system
│   │   │   ├── CInput.cs                  #   Input/command handling
│   │   │   ├── CNetGame.cs                #   Network game state
│   │   │   ├── CPlayerPool.cs             #   Player pool access
│   │   │   ├── CVehiclePool.cs            #   Vehicle pool access
│   │   │   └── ...                        #   Actors, objects, pickups, etc.
│   │   ├── Hooking/                       # MinHook installation and management
│   │   │   └── Hooks/                     #   Chat, dialog, input, scoreboard hooks
│   │   ├── Native/                        # Win32 interop (VK, MEM, AnsiString)
│   │   ├── Offsets/                       # SA-MP memory offsets (SampOffsets.cs)
│   │   └── RakNet/
│   │       ├── Arizona/                   # Arizona RP packet definitions
│   │       │   ├── EArizonaPacketId.cs    #   Sub-ID enum for Packet 220
│   │       │   └── ArizonaPacket.cs       #   Parsers for each sub-ID
│   │       ├── Incoming/                  # Incoming RPC/packet handlers
│   │       ├── Outgoing/                  # Outgoing RPC/packet handlers
│   │       ├── Packets/                   # Packet models and parser catalog
│   │       ├── Sync/                      # Sync data structures
│   │       └── Hooks/                     # RakNet function hooks
│   ├── Modules/
│   │   ├── Program.cs                     # Module registration
│   │   ├── SFModuleContainer.cs           # Container and lifecycle management
│   │   ├── SFModuleRuntime.cs             # Execution and telemetry runtime
│   │   ├── ChatViolationMonitor.cs        # Chat monitoring module
│   │   ├── DialogScraper.cs               # Dialog interception module
│   │   ├── RpcDebugger.cs                 # In-game RPC debugger
│   │   ├── DebugWeb/                      # Web-based traffic debugger
│   │   └── ...                            # Other modules
│   └── SF/                                # High-level SA-MP wrapper APIs
│       ├── SFColor.cs                     #   Color builder
│       ├── SFColors.cs                    #   Predefined color palette
│       └── ...                            #   Chat, dialog, player, vehicle APIs
└── README.md
```

---

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

---

## Acknowledgements

This project builds on the work of many talented developers and communities:

- **[TheLeftExit/SF](https://github.com/TheLeftExit/SF)** - the original project that served as the foundation
- **[SAMP.Lua](https://github.com/THE-FYP/SAMP.Lua/)** - reference source for SA-MP RPC/packet layouts and event handling
- **[SAMP-API](https://github.com/BlastHackNet/SAMP-API)** and **[DarkP1xel/SAMP-API](https://github.com/DarkP1xel/SAMP-API)** - reversed SA-MP classes and memory offsets
- **[RakHook](https://github.com/imring/RakHook)** and **[RakLua](https://github.com/Northn/RakLua)** - RakNet hooking references and implementation patterns
- **[SAMPFUNCS](https://www.blast.hk/threads/17/)** - plugin loading infrastructure and API inspiration
- **[blast.hk](https://www.blast.hk/)** - community forum for GTA SA-MP reverse engineering knowledge
