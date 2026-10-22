# Installing SF-Arizona

1. Install ASP.NET Core Runtime .NET 10 **x86**. The x64 runtime alone cannot host the mod inside GTA SA.
2. Extract the release archive beside `gta_sa.exe` and `samp.dll`:

```text
SF.asi
nethost.dll
SF/
  SF.Runtime.dll
  SF.Abstractions.dll
  SF.Protocol.dll
  SF.Runtime.runtimeconfig.json
  SF.Runtime.deps.json
  ... runtime dependencies and assets ...
```

3. Keep existing user configuration. Do not overwrite host libraries with older plugin copies.
4. Install optional plugins under `SF/modules/<folder>/` with their `manifest.json`.
   Keep plugin-private DLLs and assets in that directory. Shared extension libraries
   belong to the exporting plugin and must be declared through `sharedAssemblies`;
   do not copy them into the host's `SF/` directory.
5. Start the game, inspect `sf_loader.log` and `sf_arz.log`, then check `/sfs`,
   dialogs and plugin loading/unloading.

DebugWeb is distributed separately. Extract its archive beside `gta_sa.exe`, start
the `debug-web` module through `/sfs`, then open `http://localhost:7777` and enable
Capture. Node.js is needed only to build its frontend, not to run it in the game.

The runtime targets GTA SA 1.0 US. SA-MP must match a recognized client profile;
an unknown binary is rejected rather than treated as another revision. Native
support still needs in-game verification for the specific client build.

The `.sha256` file contains the archive checksum. `build-info.json` identifies the
version and source commit. Releases do not bundle .NET or user data.
