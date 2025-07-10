# SF

This not-yet-properly-named project is both a C# plugin framework for GTA SAMP and its single consuming plugin.

I develop and maintain this project for personal use, but it should also look good on a low-level C# job resume, if such a job even exists. You can also clone the project and write your own [Program.cs](src/Modules/Program.cs) if you want to try your hand at SAMP plugin development.

## Features

- Pure C#, including game class interaction and hooks (except for the native `DllMain` entry point), compiled into a single ASI,
- Managed "entry point" method for plugin code, with `async`/`await` support - all continuations run inside the game loop, and `await Task.Yield` waits for the next game loop frame,
- Managed [game API facade](src/SF) with awaitable methods where appropriate, including [awaitable game dialogs](src/SF/SFDialog.cs) and a [chat reader with `await foreach` support](src/SF/SFChat.StreamChatEntries.cs),
- [A minimal hook framework](src/Interop/Hooking) to create jump/call hooks with managed pipeline API,
- [A simple sub-plugin management system](src/Modules/SFModuleContainer.cs) that allows to view running `async` methods and start/stop them via an in-game dialog.

## Game build prerequisites
- GTA San Andreas (1.0 US)
- [SAMP](https://www.sa-mp.mp/downloads/) 0.3.7-R5
- If you use [SAMPFUNCS](https://www.blast.hk/threads/17/), it must be version 5.7.1 ([why?](src/Interop/Hooking/Hooks/CDialogCloseHook.txt))

## Acknowledgements

- [SAMP-API](https://github.com/BlastHackNet/SAMP-API) and [SAMP_IDBs](https://github.com/Northn/SAMP_IDBs) - reversed game classes/functions),
- [SAMPFUNCS](https://www.blast.hk/threads/17/) - inspiration for the project and API design,
- [blast.hk forum](https://www.blast.hk/) - for answering my silly questions when I was only getting started with GTA SAMP plugins.