# DebugWeb

DebugWeb is an optional plugin under `examples/DebugWeb`, not an embedded Runtime module.
It subscribes through SF.Abstractions and parses SA-MP through SF.Protocol. Optional custom
decoders and metadata are registered through ISF.Samp.PacketParsers, not linked source files.
It has no Runtime reference, offsets, native hooks or private copy of either host assembly.
The native hooks remain in the core. World inspection was removed from this plugin.

## Build And Install

Install Node.js 24.12+ and the normal SF-Arizona build prerequisites. From the repository root:

```powershell
./scripts/package-debug-web.ps1
# If the plugin was already built:
./scripts/package-debug-web.ps1 -NoBuild
./scripts/verify.ps1 -DebugWebArchive artifacts/releases/SF-DebugWeb-1.0.0-win-x86.zip
```

`-RestoreSource` accepts the same explicit local NuGet-cache paths as the main build.
The build runs npm ci when dependencies change, then typechecks and bundles the frontend.
Generated output stays under `artifacts`; node_modules stays ignored. Main-solution builds
do not build this plugin and do not require Node.js. The default build.ps1 (also called by
package.ps1) builds DebugWeb alongside the other examples, so it requires Node.js 24.12+.
The ready-to-install plugin is in `artifacts/examples/Release/debug-web/` (or `Debug`
for that configuration). It is not included in the core archive; use
package-debug-web.ps1 -NoBuild after the common build to create its separate archive.

Extract the ZIP beside `gta_sa.exe`. Its complete contents are installed in
`SF/modules/debug-web/`: manifest.json, SF.Example.DebugWeb.dll, its deps.json, and wwwroot.
Never copy SF.Runtime.dll, SF.Protocol.dll or SF.Abstractions.dll into this folder.
Install the core archive from the same build: this plugin uses public decoder and observation
APIs that older core builds do not provide. Client-specific extensions are installed separately.
The game uses the host's ASP.NET Core Runtime .NET 10 x86; no Node.js process, npm packages,
CDN assets or external web service is required at runtime.
The server accepts loopback hosts and rejects foreign browser origins for both HTTP and WebSocket.

Start `debug-web` through `/sfs` and open http://localhost:7777. Its module ID is unchanged,
so the saved `/sfs` Autostart preference continues to apply. A fresh installation defaults
to autostart disabled. Enable Capture to collect traffic; it starts disabled on every run.
Older SF/debug-web assets are unused and are not included in new core archives.

## Inspecting Traffic

The traffic table is virtualized. Directions, packet/RPC types, search and extension groups are
browser-side view filters. Pause freezes the view, not server capture; Resume reloads the
current retained buffer. Follow scrolls to new entries. Settings and view state are stored
separately, in the module's user configuration and browser localStorage respectively.
The browser applies incoming batches at most ten times per second, independently of
server capture. Follow pins the rendered window to the latest entries before paint;
scrolling back turns Follow off. With Follow off, retained rows keep their position as
older history is evicted; if the viewed row itself is evicted, the view clamps to the
remaining history. The browser's pending queue uses the same entry limit as retention.

Traffic also has a Routes filter: `220` selects a packet ID, `220/17` selects an exact
packet/sub-ID pair, and `rpc:93` selects an RPC. Commas join alternatives; `!` excludes
a route, for example `220, !220/17`. Exclusions take precedence. Packet routes use the
effective ID inside timestamp envelopes. Only selected route isolates the selected
route and resets search/direction/type filters; Hide selected route adds an exclusion.
Reset filters clears Traffic view filters. Invalid expressions show an error until fixed.
These filters persist in the browser and never change capture, retention or exports.

Select a row to fetch its details. Structure expands actual objects, public fields,
properties and array elements. Runtime segment types remain visible, including linked chat
text, commands, item IDs and slots. JSON uses the same tree with `$type` markers.
There is an explicit marker for cycles, depth limits or a property that cannot be inspected.
Raw bytes are still available when a parser or structural inspection fails.

Raw shows offsets relative to the retained buffer, hex bytes and ASCII, UTF-8 or Windows-1251
text. Payload highlighting respects bit offsets and lengths, including partial bytes.
The last byte is retained unchanged even if not all its bits are valid. The buffer is what
the host API supplied: incoming RPC can include its encoded header and a non-byte-aligned
payload; outgoing RPC generally contains payload only. The viewer never invents wire headers.
Original and effective IDs remain distinct for timestamp envelopes.

Additional categories and groups come from explicitly installed extensions. The base viewer
contains no client-specific packet IDs, sub-ID layouts or CEF commands. Without an extension,
unknown IDs remain visible in Traffic and Raw, but have no custom parsed structure.

## Retention And Export

Defaults: 10000 entries and 32 MiB of raw data. Both limits apply; oldest entries are evicted.
All retained raw bytes are kept, not a hex preview. A selected entry evicted before its
details request returns HTTP 410 with a visible explanation. Captured, retained, raw-byte,
history-eviction and delivery-drop counters are separate. A slow WebSocket client times out
after two seconds instead of blocking delivery indefinitely.

Settings control capture directions/types, ID allowlists, port and both retention limits.
Empty allowlists accept all IDs. Packet allowlists use effective IDs inside timestamps;
sub-ID allowlists exclude entries without a matching sub-ID. Save persists settings; port
changes apply on the next module restart. Invalid settings are rejected without replacing
the current settings. Capture enabled state is intentionally not restored on restart.

Download .bin exports the selected original buffer. Export downloads a schema-versioned
JSON snapshot (schema 2) of retained entries, with raw bytes encoded as Base64 and explicit valid-bit
and payload-bit windows. No continuous disk recorder or import is provided.
Entries can additionally carry extension-owned category/group labels and an observation source.
Captures can contain chat, authentication or other private data; review them before sharing.

## Development And Verification

Open DebugWeb.slnx next to the plugin project. For frontend development:

```powershell
cd examples/DebugWeb/frontend
npm ci
npm run dev
npm test
npx playwright install chromium --only-shell
npm run test:e2e
```

Vite proxies API and WebSocket requests to the running plugin on localhost:7777.
The browser tests mock the backend and exercise structured chat, hex inspection, settings,
CEF filters, virtualized traffic and mobile layout. Production assets are fully local.

```powershell
dotnet test tests/SF.DebugWeb.Tests/SF.DebugWeb.Tests.csproj -c Release
```

The plugin tests cover byte/bit fidelity, polymorphic structures, bounded retention,
exports, control parsing, WebSocket serialization, HTTP restart and collectible-ALC release.
After replacing the core and installing the plugin, verify real traffic and module stop,
restart and plugin-unload in the game. Native hook behavior is described in NETWORK.md.
