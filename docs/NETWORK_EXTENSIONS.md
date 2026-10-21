# Optional Network Extensions

The host recognizes standard RakNet and SA-MP R3 packets. Unknown byte-valued IDs
remain available through raw packet subscriptions, filters, send and emulation APIs.
Their enums, layouts, sub-ID routing and native adapters belong to external libraries.

## Decoders And Labels

`ISF.Samp.PacketParsers.RegisterIncomingDecoder` and `RegisterOutgoingDecoder` accept a
numeric custom packet ID and a callback returning a parsed packet. They reject
standard enum IDs and duplicate registrations. Dispose each registration when its
module stops. Typed standard subscriptions remain backed by SF.Protocol; extensions
can provide their own typed subscription facade over raw frames.

`RegisterMetadata` adds a display name, optional sub-ID, category/group and header
bit length. Its callback receives the direction and valid original packet bits.
Keep metadata extraction cheap: full decoding occurs only when inspecting a record.
DebugWeb derives extension tabs and filters from this metadata rather than hard-coding
client modules. With no provider installed, unknown packets remain raw and unlabelled.

## Observations From Native Integrations

An external native integration can own `ISF.Samp.Packets.RegisterSource("vendor.feature")`
and publish captured bytes through the returned `ISFPacketSource`. Publishing copies
the valid bytes and carries an explicit bit window and optional sub-ID. It never
injects a packet into the game or runs network send filters.

`ObserveSources` receives observations on the publishing thread. Observers must not
call game APIs without switching threads. An in-flight callback may finish after
disposal; the source owner must stop and drain native callbacks before unloading.
Keep the original wire frame and timestamp envelope where one exists. Synthetic
control frames must be identified as separate observations, not fabricated raw traffic.

Dispose source registrations, listeners and decoders before plugin unload. Export API
assemblies using manifest sharedAssemblies and declare direct dependencies; reference
built host DLLs, never host sources or SF.Runtime.
