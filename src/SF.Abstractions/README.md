# SFSharp.Abstractions

Plugin contracts for the SF managed host. Targets .NET 10 and C# 14.

The NuGet package is `SFSharp.Abstractions`; the assembly remains `SF.Abstractions.dll`
and namespaces remain under `SFSharp.Abstractions`.

Use `IModuleContext.SF` to access separate GTA and SA-MP services. Native-backed
operations require the game thread; use `InvokeOnMainThreadAsync` from workers.
Register subscriptions and resources with the module context so they are released
on stop. `SFModuleBase` is an optional lifecycle helper; `ISFModule` only requires
`RunAsync(IModuleContext)`.

The host supplies this assembly. Plugin deployments must not bundle a second copy.
Use `ExcludeAssets="runtime"` on the package reference in host plugins and exclude
host DLLs from their deployment archives. Ordinary .NET tools can use normal references.

Bitstreams and SA-MP parsers are provided by the separate `SFSharp.Protocol` package.
Version 5 changes the module lifecycle contract; rebuild plugins against the new API.

[Documentation](https://github.com/AllahPower/SF-Arizona/wiki)
