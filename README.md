# SF-Arizona

Fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), adapted for **Arizona RP** on **SA-MP 0.3.7 R3-1** with **SAMPFUNCS v5.5.0 rel.22**.

## Version

Current release: **2.0.0-arizona**

This release stabilizes the Arizona fork with a safer hook backend, live singleton resolution for SA:MP classes, command interception through `CInput::Send`, and structured runtime logging to `sf_arz.log`.

## Highlights

- NativeAOT `win-x86` build published as a single `SF.asi`
- `MinHook.NET`-based hook installation instead of the old manual trampoline helper
- Safer original-call path for x86 hooks with temporary self-suppression
- Live pointer resolution for `CChat`, `CDialog`, `CInput`, and other SAMP singletons
- Local command interception for `/sfd`, `/sfs`, and module-owned commands before server dispatch
- Dialog flow isolation to reduce collisions with external mods
- Centralized logging in [`src/Diagnostics/SFLog.cs`](src/Diagnostics/SFLog.cs)

## Prerequisites

- GTA San Andreas 1.0 US
- Arizona RP client
- SA:MP 0.3.7 R3-1
- SAMPFUNCS v5.5.0 rel.22
- .NET SDK 10.0 preview for local builds

## Build

- `dotnet build src/SF.csproj -c Release`
- `dotnet publish src/SF.csproj -c Release`
- Output: `src/bin/Release/net10.0/win-x86/publish/SF.asi`

## Roadmap / TODO

- Turn SF-Arizona into a robust **.NET loader for external libraries**, closer in spirit to MoonLoader but for managed plugins
- Define a stable plugin/module ABI so third-party .NET assemblies can register hooks, commands, dialogs, and tasks safely
- Finish hardening the hook layer against chained hooks, foreign trampolines, and NativeAOT edge cases
- Expand the game API with more pools, dialogs, textdraws, RPC/network handlers, and additional high-value hooks
- Improve diagnostics with log levels, filtering, rotation, and crash-context capture
- Add compatibility guards for common Arizona modpacks and better coexistence with other ASI/SAMPFUNCS addons
- Document supported offsets, client assumptions, and the process for porting to future Arizona builds
- Add focused regression tests or smoke-test utilities for command, dialog, and hook flows

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) for the original project
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [SAMP_IDBs](https://github.com/Northn/SAMP_IDBs) for reversed classes and functions
- [SAMPFUNCS](https://www.blast.hk/threads/17/) for inspiration and API ideas
- [blast.hk forum](https://www.blast.hk/) for GTA SA:MP reverse-engineering discussions
