# Incoming RakNet timestamps

`IncomingPacketFrame` retains its existing three-argument constructor and deconstruction:
`PacketId`, `Data`, `DataBitLength`. The raw bytes and subscription/filter routing are unchanged.

An incoming RakNet timestamp envelope has this layout:

```text
byte ID_TIMESTAMP (40)
uint32 little-endian transport timestamp
byte inner packet ID
inner packet body
```

The frame exposes computed, allocation-free views without adding instance fields:

- `TryGetTimestampEnvelope(out RakNetTimestampEnvelope)`: decode the header once into a value type.
- `RakNetTimestamp`: nullable uint; null for an absent or incomplete envelope. Zero is a present value.
- `EffectivePacketId`: inner ID for a complete envelope, otherwise `PacketId`.
- `PacketDataBitOffset`: 40 for a complete envelope, otherwise zero.
- `PacketData`: a zero-copy memory view starting at the inner packet ID, not after it.
- `PacketDataBitLength`: the valid bit length after removing the prefix. Respect it when parsing the final byte.

Recognizing a header does not validate the inner packet body. A buffer shorter than six bytes,
a header shorter than 48 valid bits, an inconsistent raw ID, or a bit length exceeding the buffer
does not expose a timestamp or strip any bytes.

## Compatibility and subscriptions

`StreamIncoming(207)` and `SubscribeIncoming(207, ...)` still select raw wire ID 207.
Timestamp-wrapped onfoot sync remains raw ID 40. To observe it, explicitly subscribe to 40,
then inspect `EffectivePacketId`. Keep the normal 207/200 subscriptions as well when observing both formats.
Filters see the original ID and original bytes exactly as before; effective-ID filtering is not added implicitly.

```csharp
await foreach (IncomingPacketFrame frame in sf.Packets.StreamIncoming((int)EPacketId.Timestamp, token))
{
    if (!frame.TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope))
    {
        continue;
    }

    uint timestamp = envelope.Timestamp;
    int packetId = envelope.PacketId;
    ReadOnlyMemory<byte> packetData = frame.Data[RakNetTimestampEnvelope.PrefixByteLength..];
    int packetBitLength = frame.DataBitLength - RakNetTimestampEnvelope.PrefixByteLength * 8;
    // Decode using the inner packet ID, the valid bit window and the received transport timestamp.
}
```

For the unsafe managed `BitStreamReader`, keep the memory pinned for the entire reader lifetime
and pass the explicit bit length. Do not use the buffer byte length as a substitute for the valid bit window.

Runtime `IncomingPacketPayload.ToFrame()` exposes the same views without another byte copy.
Legacy payload `Parse`/`Use` callbacks and `IncomingPacketArgs.CreateReader()` continue to receive the raw envelope.

## Timestamp meaning

This is the 32-bit transport tick value in the buffer returned by RakNet Receive. It is not UTC,
not the local receive time, and not a guaranteed original sender clock. RakNet can adjust transport time.
The value wraps; do not treat a numerically smaller value as sufficient evidence of reordering or a bot.
The API does not synthesize timestamps for unwrapped packets or unwrap nested envelopes recursively.

A behavior collector still needs separate monotonic capture time and sequence recorded before dispatch.
These changes do not add those observations or modify detection/ban behavior.

## Verified client and tests

Read-only IDA verification: SA-MP R3 reference `sampR3.dll`, image base `0x10000000`, SHA-256
`9c9b2cc31a4ced6967420b1880c096b5c4e7630e227aa379be4019c21b6fddc1`.
`CNetGame::UpdateNetwork` at RVA `0xAF20` selects `data[5]` for ID 40;
`Packet_PlayerSync` at RVA `0xA3E0` reads the four-byte timestamp before the inner packet ID.
This verifies the envelope layout, not every Arizona/mobile sync body.

```powershell
dotnet test tests/SF.Network.Contracts.Tests/SF.Network.Contracts.Tests.csproj -c Release
dotnet test tests/SF.Network.Tests/SF.Network.Tests.csproj -c Release
dotnet build src/SF.Runtime/SF.Runtime.csproj -c Release
```

The xUnit v3 contract tests run on the host architecture and cover zero/max timestamps, an independent
little-endian wire fixture, inner IDs, partial bit lengths, truncated/inconsistent headers and legacy fields.
Runtime tests run x86 and cover detached copies and managed dispatch.
Runtime tests reference the compiled x86 Runtime assembly rather than recompiling Runtime source files into the test assembly.
The timestamp prefix is decoded by `RakNetTimestampEnvelope.TryRead`, not guessed from arrival time,
sync payload fields or native structure padding. No packed native overlay or synthesized fallback is used.

Memory tests run separately from parallel collections. After warm-up, they measure thread-local managed
allocations for one million header/frame view reads and compare payload copying and subscribed dispatch
against their existing byte-array and listener-snapshot allocation budgets. These paths are not all
allocation-free: packet ownership still requires a byte copy and dispatch still snapshots listeners.

Weak-reference tests force GC after 10,000 dispatches and check sampled source/copied buffers while
the manager and subscription remain alive. A disposed subscription must release its captured callback
owner even if the subscription itself is retained; disposal remains idempotent under contention and
propagates callback failures. CI runs both test projects before release packaging.

These checks do not prove absence of every leak in the game. They cover the referenced production packet
manager, payload and subscription implementations, not the native hook, native allocator or scheduled
dispatcher/channel backlog. Slow consumers can still accumulate queued packets; in-game profiling
is required for those paths. The tests also do not replace native transport/routing validation.
