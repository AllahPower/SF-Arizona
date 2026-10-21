<div align="center">

# SF-Arizona

*A C# framework that brings the full SA-MP/GTA game environment into managed code - build game modules with a clean API, not raw memory hacks*

[![.NET](https://img.shields.io/badge/.NET_10-managed_runtime-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![Platform](https://img.shields.io/badge/platform-win--x86-blue)](https://github.com/AllahPower/SF-Arizona)
[![Wiki](https://img.shields.io/badge/docs-Wiki-green?logo=github)](https://github.com/AllahPower/SF-Arizona/wiki)

</div>

---

## About

**SF-Arizona** is a C# framework that exposes GTA San Andreas and **SA-MP 0.3.7 R3-1** through a high-level API. The core does not require Arizona RP, SAMPFUNCS or other client modifications. A native win-x86 `SF.asi` loader hosts managed `SF.Runtime.dll` through hostfxr; the target machine requires **ASP.NET Core Runtime .NET 10 x86**.

The core idea is simple: instead of writing raw memory patches and ASM hooks, you write **modules** against `SF.Abstractions`. Built-in modules live in `src/SF.Runtime/Modules/BuiltIn/`; independent plugin samples live in `examples/`. The framework handles RakNet interception, native memory pools, typed events and module lifecycles.

The project is a fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF). Client-specific integrations are optional external libraries, not core dependencies.

**Maintained branch:** [`main`](https://github.com/AllahPower/SF-Arizona/tree/main), promoted from the managed-runtime experiment. The former `master` implementation is preserved in [`legacy/master-outdated`](https://github.com/AllahPower/SF-Arizona/tree/legacy/master-outdated) for historical reference only.

### Goals

- **Game environment in C#** - expose players, vehicles, objects, dialogs, chat, and all other SA-MP entities as typed, safe abstractions that module authors can use directly
- **Network layer access** - observe standard SA-MP RPCs and packets in both directions; unknown numeric IDs remain available as raw frames
- **Module-first architecture** - built-in and external modules use typed game/network contracts, chat commands and explicit lifecycle ownership
- **Developer tooling** - optional DebugWeb plugin, structured logging, telemetry, and the `/sfs` in-game dashboard for module management
- **Managed plugin delivery** - ship a native loader and managed dependencies in an installable win-x86 archive

---

## Features

| Category | Description |
|---|---|
| **Native + Managed Host** | Native `SF.asi` loads `SF.Runtime.dll` through hostfxr; requires ASP.NET Core Runtime .NET 10 x86 |
| **RakNet Interception** | Full duplex hooking of `RakClient::RPC`, `RakClient::Send`, `RakClient::Receive`, and `HandleRpcPacket` via MinHook trampolines |
| **Extension API** | Disposable registrations for custom packet decoders, metadata and separately identified observation sources; no custom client formats in the core |
| **Module System** | Attribute-based module registration with lifecycle management, telemetry, heartbeat tracking, counters, and the `/sfs` in-game dashboard |
| **Web Debugger** | Optional DebugWeb plugin on `localhost:7777` with structured trees, raw hex/offset inspection, dynamic extension categories and capture export |
| **Native Pools** | Typed abstractions over SA-MP memory pools: players, vehicles, objects, actors, pickups, menus, textdraws, gang zones, and labels |
| **Color API** | `SFColor` and `SFColors` builder for SA-MP `{RRGGBB}` text styling with bitwise composition |
| **Parsed Event Surface** | Unified `SF.Events` API over parsed RPC and packet payloads, plus `ModuleContext` helpers for module-friendly subscriptions |
| **Command Interception** | Hook into `CInput::Send` for local command processing before packets are transmitted |
| **Structured Logging** | Background file logger writing to `sf_arz.log` with module-scoped prefixes via `Microsoft.Extensions.Logging` |

---

## Quick Start

### Prerequisites

| Requirement | Details |
|---|---|
| GTA San Andreas | Version **1.0 US** |
| SA-MP | **0.3.7 R3-1** |
| .NET SDK | Stable **10.0**, selected by `global.json` (for building) |
| Native build tools | Visual Studio 2026 C++ x86/x64 tools, v145 |
| PowerShell | **7** for build/package scripts |
| Target runtime | **ASP.NET Core Runtime .NET 10 x86**; x64 alone is insufficient |

### Build

```powershell
# Managed compilation
dotnet build src/SF.Runtime/SF.Runtime.csproj -c Release

# Source boundaries and release-rule tests
./scripts/verify.ps1

# Build native loader, runtime, abstractions and example plugins; publish Runtime
./scripts/build.ps1

# Package an existing build and verify deployment
./scripts/package.ps1 -NoBuild
./scripts/verify.ps1 -Archive artifacts/releases/SF-Arizona-4.1.0-win-x86.zip
```

Open `SF-Arizona.sln` in Visual Studio. Native output: `artifacts/native/Release/win-x86/`; managed deployment: `artifacts/publish/Release/win-x86/SF/`; archives/checksums: `artifacts/releases/`. For offline checks, `build.ps1 -RestoreSource` accepts absolute local NuGet-cache paths.

`build.ps1` builds all examples, including DebugWeb, and requires Node.js 24.12+ for its
frontend. Example deployment folders are under `artifacts/examples/Release/`, including
`debug-web/`. DebugWeb is packaged separately with `./scripts/package-debug-web.ps1 -NoBuild`.

To build and create an archive in one command, run `./scripts/package.ps1`. It invokes
`build.ps1`, including native compilation and Runtime publishing. Visual Studio's Build command
only compiles the solution; it does not populate the publish directory. Use `-NoBuild` only
after `build.ps1` has prepared deployment files for the requested version and current commit.
`package.ps1` also forwards `-RestoreSource` and `-SkipRestore` to `build.ps1`.

### Install

Install ASP.NET Core Runtime .NET 10 **x86** and extract the archive beside `gta_sa.exe` and `samp.dll`. Keep `SF.asi`, `nethost.dll` and the `SF/` dependency/assets directory together. See [installation instructions](docs/INSTALL.md).

---

## Architecture

```text
src/
  SF.Abstractions/  # plugin-visible contracts and managed bitstreams
  SF.Protocol/      # standard SA-MP packet and RPC models and parsers, no native code
  SF.Runtime/
    Bootstrap/     # initialization, composition and main-thread dispatch
    Ui/            # chat, dialogs, keyboard and UI facades
    Game/          # entities, players, pools and world facades
    Events/        # typed event streams
    Networking/    # managed dispatch, filters and handler registration
    Modules/       # Hosting, Lifecycle, PluginLoading and BuiltIn
    Storage/       # module config/storage implementations
    Diagnostics/   # logging and runtime build information
    Interop/       # native wrappers, offsets, hooks and RakNet ABI adapters
  SF.Native/       # C++ loader/hostfxr integration
examples/          # independent plugins referencing Abstractions and optionally Protocol
scripts/           # reproducible build, package and verification tooling
docs/              # deployment instructions
artifacts/         # ignored generated output
```

Runtime depends on Abstractions, never the reverse. The native loader bootstraps Runtime without a managed project reference. Public assembly/type identities are unchanged. This is a modular layered runtime; existing native-facing facades and raw callback bridges remain compatibility boundaries rather than a claim of complete assembly-enforced Clean Architecture.

### DebugWeb Plugin

DebugWeb is distributed separately from the core. Extract `SF-DebugWeb-1.0.0-win-x86.zip`
beside the game executable and start `debug-web` in `/sfs`. Open `http://localhost:7777`
and enable Capture. World inspection is not part of this plugin.

```powershell
# Build the plugin, its Preact frontend and the separate installable archive
./scripts/package-debug-web.ps1
dotnet test tests/SF.DebugWeb.Tests/SF.DebugWeb.Tests.csproj -c Release
```

Node.js 24.12+ is required to build DebugWeb, but not to run it in the game. The main
`SF-Arizona.sln` does not require Node.js. Open `examples/DebugWeb/DebugWeb.slnx` to work
on the plugin and its tests. See [the DebugWeb guide](docs/DEBUGWEB.md).

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

### Custom Client Extensions

Custom packet IDs have no built-in client-specific interpretation. Optional plugins register
decoders and metadata through `ISF.Samp.PacketParsers`; separate native integrations can publish
copied observations through `ISF.Samp.Packets.RegisterSource`. Registrations must be disposed
before the exporting plugin unloads. Normal raw send/receive APIs accept numeric IDs.

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

Register built-in modules in `src/SF.Runtime/Bootstrap/Program.cs`:

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
| `Context.RegisterIncomingRpc<T>(...)` | Subscribe a module to a parsed incoming RPC |
| `Context.RegisterOutgoingRpc<T>(...)` | Subscribe a module to a parsed outgoing RPC |
| `Context.RegisterIncomingPacket<T>(...)` | Subscribe a module to a parsed incoming packet |
| `Context.RegisterOutgoingPacket<T>(...)` | Subscribe a module to a parsed outgoing packet |
| `Context.InvokeOnMainThreadAsync(action)` | Execute a delegate on the game thread with module cancellation |
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

See [GTA and SA-MP API](docs/GAME_API.md) for service boundaries, tick/world queries and main-thread execution.

For detailed guides, API reference, and examples, visit the **[SF-Arizona Wiki](https://github.com/AllahPower/SF-Arizona/wiki)**.

---

## Dependencies

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| [`MinHook.NET`](https://www.nuget.org/packages/MinHook.NET) | 1.1.2 | Function hooking with trampoline calls |
| [`Microsoft.Extensions.Logging.Abstractions`](https://www.nuget.org/packages/Microsoft.Extensions.Logging.Abstractions) | 10.0.5 | Logging interfaces and abstractions |

### Framework References

| Framework | Purpose |
|---|---|
| `Microsoft.AspNetCore.App` | Minimal API web server for the traffic debugger |

### Build Requirements

| Tool | Version |
|---|---|
| .NET SDK | Stable 10.0, selected by global.json |
| Native tools | Visual Studio 2026 C++ x86/x64, v145 |
| Target | `net10.0`, `win-x86`, native loader + managed runtime |

---

## Versioning and Releases

`Version.props` owns the base version (**4.1.0**) and numeric assembly/file versions. Use SemVer: patch for fixes, minor for compatible features, major for breaking changes. The number of commits does not dictate version bumps; a folder-only refactor retains the current version.

- PRs and supported branch pushes build and verify a deployment archive.
- Pushes to `main` or `experiment/jit-runtime` publish `4.1.0-preview.<run_number>.<short_sha>` prereleases, never latest.
- Tag `v4.1.0` publishes a stable release; `v4.1.0-rc.1` publishes a prerelease. The numeric version must match Version.props.
- Reruns reuse the same preview identity. Archives contain build-info.json and a separate SHA-256 checksum.
- Automatic NuGet publication is not configured.

## Contribution and Validation

Use scoped Conventional Commits with imperative English subjects and concrete body bullets prefixed with `-`. Do not add coauthor trailers or assistant attribution. Keep fixes, structural moves, documentation and CI changes in separate commits.

Build and structural checks are the automated minimum. Native hooks require focused in-game checks: loader logs, SA-MP dialogs, `/sfs`, plugin load/unload and web debugger assets/traffic. Offset-sensitive changes must document the verified source/client build.

---

## TODO / Roadmap

- [x] SA-MP 0.3.7 R3-1 core independent of client modifications
- [x] RPC and raw packet interception
- [x] Expanded native gameplay pool abstractions
- [x] Runtime module metadata, lifecycle, telemetry, and `/sfs` management UI
- [x] Module-scoped logger contract via `SFModuleBase`
- [x] Shared SA-MP color builder via `SFColor` and `SFColors`
- [ ] Expand high-level `SF.*` wrappers over newly mapped native classes
- [ ] Add more module examples built on the new runtime contract
- [ ] Persist module settings such as autostart and per-module options
- [ ] Harden optional native integration lifecycle and coexistence with foreign hooks
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
