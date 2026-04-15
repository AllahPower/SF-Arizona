# HelloWorld

A minimal external plugin for `SF`.

What the example shows:
- the module references only `SF.Abstractions`;
- the module implements `ISFModule` directly;
- prints `Hello world!` to the game chat via `context.SF.Chat.Add(...)`;
- writes a line to the standard SF log via `context.Log`.

Build:

```powershell
dotnet build example/HelloWorld/HelloWorld.Example.csproj -c Release
```