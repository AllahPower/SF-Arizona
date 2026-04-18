# CEF Emitor Example

External SF plugin that hosts a local ASP.NET Core web UI on
`http://localhost:7778` and injects synthetic Arizona `220/17` (`InjectCode`)
packets into the RakPeer receive queue via `ISFNetwork.SimulateIncomingPacket`.
The real in-game CEF handler parses the bytes and executes the JS inside the
target browser.

## Packet layout (220/17)

| Field         | Type  | Notes                                   |
| ------------- | ----- | --------------------------------------- |
| `packetId`    | u8    | `220`                                   |
| `subId`       | u8    | `17` (`InjectCode`)                     |
| `browserId`   | u32   | target browser id                       |
| `codeLength`  | u16   | length of `code` in bytes               |
| `encodedFlag` | u8    | `0` (plain, no encoding transform)      |
| `code`        | bytes | JS source, UTF-8                        |
| `requestId`   | u32   | response correlation id                 |

All multi-byte integers are little-endian.

## Build

```
dotnet build examples/CefEmitor/CefEmitor.Example.csproj -c Debug
```

Output lands in `examples/CefEmitor/bin/Debug/cef-emitor/` with the plugin
DLL, `module.json`, and `wwwroot/`.

## Install

Copy the output folder into the game's plugin root:

```
<GameDir>/SF/modules/cef-emitor/
  SF.Example.CefEmitor.dll
  module.json
  wwwroot/
    index.html
    style.css
```

The plugin ships with `enabledOnStart: false` and `DefaultEnabled = false`.
Start it manually from chat:

```
/sfs start example.cef-emitor
```

Open `http://localhost:7778/` in any browser on the host machine, fill in
`browserId` / `code` / `requestId`, press **Emit InjectCode**.

## Endpoint

`POST /emit/injectcode` — body:

```json
{ "browserId": 0, "code": "alert('hi')", "requestId": 0 }
```

Response:

```json
{ "ok": true, "sentBytes": 41 }
```

## Notes

- `ISFNetwork.SimulateIncomingPacket` is main-thread only; the module
  switches to the main thread before every call.
- JS bytes are UTF-8. ASCII-only JS renders identically under any encoding
  the client may assume. Change `Encoding.UTF8` in `CefEmitorModule.cs` if a
  specific code page is required.
- Port `7778` is hardcoded (the built-in `DebugWeb` already uses `7777`).
