using System.Numerics;
using System.Text.Json;
using SFSharp.Abstractions.Network.Protocol;
using SFSharp.Examples.DebugWeb.Capture;
using SFSharp.Protocol.Packets.Catalog;
using SFSharp.Protocol.Transport;

namespace SF.DebugWeb.Tests;

public sealed class CaptureTests
{
    [Fact]
    public void ExtensionStructuresRetainConcreteTypesAndEveryField()
    {
        var tree = InspectionProjector.Project(new ExamplePayload([new ExampleText("hi"), new ExampleItem(4660)]));
        var segments = Assert.Single(tree.Children, child => child.Name == "Segments");
        Assert.Equal(2, segments.Children.Length);
        Assert.Equal("ExampleText", segments.Children[0].Type);
        Assert.Equal("hi", Assert.Single(segments.Children[0].Children).Value);
        Assert.Equal("4660", Assert.Single(segments.Children[1].Children).Value);
    }

    private abstract record ExampleSegment;
    private sealed record ExampleText(string Text) : ExampleSegment;
    private sealed record ExampleItem(int ItemId) : ExampleSegment;
    private sealed record ExamplePayload(ExampleSegment[] Segments);

    [Fact]
    public void ProjectionIncludesVectorFieldsAndNestedArraysWithoutStringifyingThem()
    {
        var tree = InspectionProjector.Project(new { Position = new Vector3(1.5f, 2, 3), Nested = new[] { new[] { 1, 2 }, new[] { 3 } } });
        var vector = Assert.Single(tree.Children, child => child.Name == "Position");
        Assert.Contains(vector.Children, field => field.Name == "X" && field.Value == "1.5");
        var nested = Assert.Single(tree.Children, child => child.Name == "Nested");
        Assert.Equal("2", nested.Children[0].Children[1].Value);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(new TrafficDetails("test", Summary(1), tree, "test", null, 0, 0, 0, []), CaptureJsonContext.Default.TrafficDetails);
        Assert.Contains("Position", System.Text.Encoding.UTF8.GetString(bytes));
    }

    [Fact]
    public void MemoryAndDomainDetailFieldsAreInspectable()
    {
        var tree = InspectionProjector.Project(new { Data = new ReadOnlyMemory<byte>([1, 2, 3]), Detail = "payload detail" });
        Assert.Contains(tree.Children, node => node.Name == "Detail" && node.Value == "payload detail");
        var memory = Assert.Single(tree.Children, node => node.Name == "Data");
        Assert.Equal("array", memory.Kind);
        Assert.Equal(new[] { "1", "2", "3" }, memory.Children.Select(node => node.Value));
    }

    [Fact]
    public void UnknownPacketAndParseFailureStillRetainOriginalBytes()
    {
        using TrafficCapture capture = new(new() { Capture = true });
        capture.Record("in", "packet", 255, new byte[] { 255, 1, 2 }, 21, 0, 21);
        capture.Record("out", "packet", 220, new byte[] { 220, 210 }, 16, 0, 16);
        CapturedTraffic[] entries = capture.Store.Snapshot();
        Assert.Equal(2, entries.Length);
        Assert.Equal(new byte[] { 255, 1, 2 }, entries[0].Data.ToArray());
        Assert.Equal(21, entries[0].DataBitLength);
        Assert.NotNull(new TrafficDecoder().Decode(entries[1]).Error);
    }

    [Fact]
    public void TimestampAndNonByteAlignedRpcPreserveTheirOriginalWindows()
    {
        using TrafficCapture capture = new(new() { Capture = true });
        byte[] timestamp = [40, 1, 2, 3, 4, 220, 18, 0, 0, 0, 0, 0, 0];
        capture.Record("in", "packet", 40, timestamp, 104, 40, 64);
        capture.Record("in", "rpc", 255, new byte[] { 1, 2, 3 }, 21, 11, 10);
        var entries = capture.Store.Snapshot();
        Assert.Equal(40, entries[0].Summary.Id);
        Assert.Equal(220, entries[0].Summary.EffectiveId);
        Assert.Null(entries[0].Summary.SubId);
        Assert.Equal(40, entries[0].PayloadBitOffset);
        Assert.Equal(timestamp, entries[0].Data.ToArray());
        Assert.Equal(11, entries[1].PayloadBitOffset);
        Assert.Equal(10, entries[1].PayloadBitLength);
        Assert.Equal(21, entries[1].DataBitLength);
    }

    [Fact]
    public void RetentionEnforcesEntryAndByteLimitsAndNeverReusesSequenceIds()
    {
        TrafficStore store = new(new() { MaxEntries = 2, RawBudgetMiB = 1 });
        for (int i = 0; i < 3; i++) store.Add(new(Summary(0), new byte[100], 800, 0, 800));
        Assert.Null(store.Find(1));
        Assert.Equal(1, store.Stats(0, 7777).Evicted);
        Assert.Equal(200, store.Stats(0, 7777).RawBytes);
        store.Configure(new() { MaxEntries = 100, RawBudgetMiB = 1 });
        store.Add(new(Summary(0), new byte[1024 * 1024], 8 * 1024 * 1024, 0, 0));
        Assert.Single(store.Snapshot());
        Assert.Equal(3, store.Stats(0, 7777).Evicted);
        store.Clear();
        Assert.Empty(store.Snapshot());
        Assert.Equal(0, store.Stats(0, 7777).RawBytes);
        Assert.Equal(5, store.Add(new(Summary(0), new byte[1], 8, 0, 8))!.Seq);
    }

    [Fact]
    public void ExportPreservesRawBytesAndBitLengths()
    {
        CapturedTraffic frame = new(Summary(1), new byte[] { 0xFF, 0xAB, 0x80 }, 19, 9, 10);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(new CaptureExport(2, "session", [frame]), CaptureJsonContext.Default.CaptureExport);
        var restored = JsonSerializer.Deserialize(json, CaptureJsonContext.Default.CaptureExport)!;
        Assert.Equal(frame.Data.ToArray(), restored.Entries[0].Data.ToArray());
        Assert.Equal(19, restored.Entries[0].DataBitLength);
        Assert.Equal(9, restored.Entries[0].PayloadBitOffset);
        Assert.Equal(10, restored.Entries[0].PayloadBitLength);
    }

    private static TrafficSummary Summary(long seq) => new(seq, "in", "packet", 255, 255, null, "Unknown", null, 1, 0);
}
