using SFSharp.Abstractions.Network;
using SFSharp.Abstractions.Parsing;
using SFSharp.Protocol.Packets.Catalog;
using SFSharp.Runtime.Bootstrap;
using SFSharp.Runtime.Networking;
using SFSharp.Runtime.Networking.RakNet;
using SFSharp.Runtime.Networking.RakNet.Filtering;
using System.Runtime.CompilerServices;

namespace SF.Network.Tests;

public sealed class PacketDecoderExtensionTests
{
    [Fact]
    public void StandardTypedAliasesRemainAvailableForTheSameWireId()
    {
        var registry = PacketParserCatalog.CreateDefaultRegistry();
        Assert.Single(registry.GetIncomingRoutes<SFSharp.Protocol.Packets.Models.IncomingConnectionAttemptFailedPacket>());
        Assert.Single(registry.GetIncomingRoutes<SFSharp.Protocol.Packets.Models.IncomingConnectionFailedPacket>());
    }
    [Fact]
    public void DecoderRegistrationIsExplicitAndReversible()
    {
        var parsers = CreateParsers();
        var frame = new IncomingPacketFrame(230, new byte[] { 230, 42 }, 16);
        Assert.False(parsers.TryParseIncoming(frame, out _));
        using (parsers.RegisterIncomingDecoder(230, _ => new CustomPacket(42)))
        {
            Assert.True(parsers.TryParseIncoming(frame, out var packet));
            Assert.Equal(42, Assert.IsType<CustomPacket>(packet).Value);
            Assert.Throws<ArgumentException>(() => parsers.RegisterIncomingDecoder(230, _ => null));
        }
        Assert.False(parsers.TryParseIncoming(frame, out _));
    }

    [Fact]
    public void ExtensionsCannotReplaceStandardPacketsOrReadPastTheBuffer()
    {
        var parsers = CreateParsers();
        Assert.Throws<ArgumentException>(() => parsers.RegisterIncomingDecoder(207, _ => null));
        Assert.False(parsers.TryParseIncoming(new IncomingPacketFrame(230, new byte[1], 16), out _));
        Assert.False(parsers.TryParseOutgoing(new OutgoingPacketFrame(230, new byte[1], -1), out _));
    }

    [Fact]
    public void MetadataIsRemovedWhenItsOwnerStops()
    {
        var parsers = CreateParsers();
        using (parsers.RegisterMetadata(230, (_, _, _) => new("Custom", 4, "TOOLS", "Example", 16)))
            Assert.Equal("TOOLS", parsers.Describe(230, true, new byte[2], 16)!.Category);
        Assert.Null(parsers.Describe(230, true, new byte[2], 16));
    }

    private static SFPacketParsers CreateParsers()
    {
        var dispatcher = new NetworkDispatcher(new MainThreadDispatcher(new SFSynchronizationContext(exception => throw exception)));
        return new(PacketParserCatalog.CreateDefaultRegistry(), new SFPackets(new NetworkFilters(), dispatcher));
    }

    [Fact]
    public void DisposedDecoderRegistrationDoesNotRetainItsCallbackOwner()
    {
        var parsers = CreateParsers();
        var (registration, owner) = RegisterOwner(parsers);
        registration.Dispose();
        for (int i = 0; i < 5 && owner.IsAlive; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(owner.IsAlive);
        GC.KeepAlive(registration);
        GC.KeepAlive(parsers);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (IDisposable Registration, WeakReference Owner) RegisterOwner(SFPacketParsers parsers)
    {
        var owner = new DecoderOwner();
        return (parsers.RegisterIncomingDecoder(230, owner.Decode), new(owner));
    }

    private sealed class DecoderOwner
    {
        public IParsedIncomingPacket? Decode(IncomingPacketFrame frame) => new CustomPacket(42);
    }

    [Fact]
    public void ObservationsAreCopiedAndCannotInjectOrOutliveTheirSource()
    {
        var dispatcher = new NetworkDispatcher(new MainThreadDispatcher(new SFSynchronizationContext(exception => throw exception)));
        var packets = new SFPackets(new NetworkFilters(), dispatcher);
        List<SFPacketObservation> observations = [];
        using var listener = packets.ObserveSources(observations.Add);
        var source = packets.RegisterSource("fixture.source");
        Assert.Throws<InvalidOperationException>(() => packets.RegisterSource("fixture.source"));
        byte[] data = [230, 1, 2];
        source.Publish(true, 230, data, 21, 8, 13, 7);
        data[1] = 99;
        Assert.Equal((byte)1, Assert.Single(observations).Data.Span[1]);
        Assert.Equal(21, observations[0].DataBitLength);
        Assert.Equal(7, observations[0].SubId);
        source.Dispose();
        source.Publish(true, 230, data, 21, 8, 13, 7);
        Assert.Single(observations);
        using var replacement = packets.RegisterSource("fixture.source");
    }

    private sealed record CustomPacket(int Value) : IParsedIncomingPacket
    {
        public SFSharp.Abstractions.Network.Protocol.RakNetPacketId RakNetPacketId => (SFSharp.Abstractions.Network.Protocol.RakNetPacketId)230;
        public string Name => "Custom";
        public string? Detail => null;
    }
}
