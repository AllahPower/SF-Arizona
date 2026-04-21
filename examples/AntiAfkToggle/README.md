# AntiAFK Toggle Example

External plugin that flips the AntiAFK flag exposed by `vorbisFile.asi` (the Arizona RP client core) through its `setAntiAfk` / `isAntiAfk` exports.

Resolves the exports lazily through `GetModuleHandleW` + `GetProcAddress`, tries `vorbisFile.asi` first and falls back to `vorbisFile.dll`. If neither is loaded, the plugin stays up but `/sfafk` reports unavailable.

## Commands

- `/sfafk` — toggle AntiAFK on/off.
- `/sfafk on` — force on.
- `/sfafk off` — force off.

## What the flag does

With AntiAFK on, the client keeps its game loop at full rate and keeps sending sync / action packets while the window is minimized or unfocused. With it off, the client throttles and blocks most outgoing sync when it detects AFK through `GetForegroundWindow`.
