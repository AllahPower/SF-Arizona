# SF-Arizona

Fork of [TheLeftExit/SF](https://github.com/TheLeftExit/SF), adapted for **Arizona RP** client based on **SA-MP 0.3.7 R3-1** (SAMPFUNCS v5.5.0 rel.22).

All memory offsets have been updated to match this specific client version.

## Features

- Pure C#, including game class interaction and hooks (except for the native `DllMain` entry point), compiled into a single ASI,
- Managed "entry point" method for plugin code, with `async`/`await` support - all continuations run inside the game loop, and `await Task.Yield` waits for the next game loop frame,
- Managed [game API facade](src/SF) with awaitable methods where appropriate, including [awaitable game dialogs](src/SF/SFDialog.cs) and a [chat reader with `await foreach` support](src/SF/SFChat.StreamChatEntries.cs),
- [A minimal hook framework](src/Interop/Hooking) to create jump/call hooks with managed pipeline API,
- [A simple sub-plugin management system](src/Modules/SFModuleContainer.cs) that allows to view running `async` methods and start/stop them via an in-game dialog.

## Game build prerequisites
- GTA San Andreas (1.0 US)
- Arizona RP client, SAMPFUNCS v5.5.0 rel.22 (SA-MP 0.3.7 R3-1)

## Acknowledgements

- [TheLeftExit/SF](https://github.com/TheLeftExit/SF) - original project,
- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [SAMP_IDBs](https://github.com/Northn/SAMP_IDBs) - reversed game classes/functions,
- [SAMPFUNCS](https://www.blast.hk/threads/17/) - inspiration for the project and API design,
- [blast.hk forum](https://www.blast.hk/) - for answering questions on GTA SAMP plugin development.