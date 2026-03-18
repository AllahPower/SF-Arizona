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
- .NET SDK 10.0 for local builds

## Build

- `dotnet build src/SF.csproj -c Release`
- `dotnet publish src/SF.csproj -c Release`
- Output: `src/bin/Release/net10.0/win-x86/publish/SF.asi`

---

## TODO / Roadmap

- [x] **Arizona Port:** Core offsets and SA:MP class access migrated to Arizona RP 0.3.7 R3-1
- [x] **Hook Backend:** Old manual trampoline helper replaced with `MinHook.NET`
- [x] **Command Interception:** Local commands handled through `CInput::Send` before server dispatch
- [x] **Dialog Isolation:** SF dialogs moved away from conflicting IDs and filtered from foreign mod dialogs
- [x] **General Logging:** Centralized runtime logging added to `sf_arz.log`
- [x] **Version System:** Release metadata added with `2.0.0-arizona`
- [ ] **.NET Loader:** Turn SF-Arizona into a managed loader for external .NET libraries, closer to MoonLoader for C# plugins
- [ ] **Plugin ABI:** Define a stable module/plugin ABI for hooks, commands, dialogs, tasks, and lifecycle callbacks
- [ ] **Hook Hardening:** Finish resilience against chained hooks, foreign trampolines, and NativeAOT edge cases
- [ ] **API Expansion:** Expand access to pools, dialogs, textdraws, RPC/network handlers, and other gameplay systems
- [ ] **Diagnostics:** Add log levels, filtering, rotation, and richer crash-context capture
- [ ] **Compatibility Layer:** Improve coexistence with Arizona modpacks, ASI plugins, and SAMPFUNCS-based addons
- [ ] **Portability:** Document supported client builds, offsets, and the porting workflow for future Arizona updates
- [ ] **Testing:** Add smoke-test or regression utilities for command, dialog, and hook flows

---

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) for the original project
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [SAMP_IDBs](https://github.com/Northn/SAMP_IDBs) for reversed classes and functions
- [SAMPFUNCS](https://www.blast.hk/threads/17/) for inspiration and API ideas
- [blast.hk forum](https://www.blast.hk/) for GTA SA:MP reverse-engineering discussions
