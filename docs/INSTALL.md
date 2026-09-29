# Installing SF-Arizona (win-x86)

1. Install the **ASP.NET Core Runtime .NET 10 x86** from https://dotnet.microsoft.com/download/dotnet/10.0. The x64 runtime alone is not sufficient for GTA SA.
2. Extract the archive into the game directory next to `gta_sa.exe` and `samp.dll`:

   ```text
   SF.asi
   nethost.dll
   SF/
     SF.Runtime.dll
     SF.Abstractions.dll
     SF.Runtime.runtimeconfig.json
     SF.Runtime.deps.json
     ... runtime dependencies ...
     debug-web/wwwroot/
   ```

3. Keep existing user configuration and third-party modules. Do not copy an older `SF.Abstractions.dll` over the host copy.
4. Start the game and inspect `sf_loader.log` and `sf_arz.log`. Check `/sfs`, a local dialog, module loading/unloading and the web debugger.

Target: GTA SA 1.0 US, SA-MP 0.3.7 R3-1, Arizona RP, SAMPFUNCS 5.5.0 rel.22. This distribution uses a native hostfxr loader and managed .NET assemblies; it is not the historical single-file NativeAOT build.

The `.sha256` file contains the archive checksum. `build-info.json` records its version and source commit. The archive does not contain user data, example plugins or a bundled .NET runtime.
