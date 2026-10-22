# SFSharp.Protocol

Native-free SA-MP packet/RPC models, parsers and managed bitstreams for .NET 10.
Depends on the `SFSharp.Abstractions` package; does not depend on the host, GTA or `samp.dll`.
The NuGet package is `SFSharp.Protocol`; the assembly remains `SF.Protocol.dll`
and namespaces remain under `SFSharp.Protocol`.

Managed bitstreams are in `SFSharp.Protocol.BitStream`. They support unaligned
RakNet fields and Windows-1251 strings. The SA-MP-specific compressed-string path
requires an explicitly supplied `ISampStringDecoder`; no native address is guessed.

Normal .NET applications can reference this package directly. SF plugins must use
the host's shared Protocol and Abstractions assemblies, not ship private copies.

[Network documentation](https://github.com/AllahPower/SF-Arizona/wiki/Networking)
