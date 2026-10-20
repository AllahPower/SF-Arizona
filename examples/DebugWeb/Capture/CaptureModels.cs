using System.Text.Json.Serialization;

namespace SFSharp.Examples.DebugWeb.Capture;

internal sealed record CaptureSettings
{
    public bool Capture { get; init; }
    public bool Incoming { get; init; } = true;
    public bool Outgoing { get; init; } = true;
    public bool Rpc { get; init; } = true;
    public bool Packets { get; init; } = true;
    public int Port { get; init; } = 7777;
    public int MaxEntries { get; init; } = 10_000;
    public int RawBudgetMiB { get; init; } = 32;
    public int[] PacketIds { get; init; } = [];
    public int[] RpcIds { get; init; } = [];
    public int[] SubIds { get; init; } = [];

    public string? Validate()
    {
        if (Port is < 1024 or > 65535) return "Port must be between 1024 and 65535.";
        if (MaxEntries is < 1 or > 100_000) return "MaxEntries must be between 1 and 100000.";
        if (RawBudgetMiB is < 1 or > 128) return "RawBudgetMiB must be between 1 and 128.";
        if (PacketIds is null || RpcIds is null || SubIds is null) return "ID filters must be arrays.";
        if (PacketIds.Any(id => id is < 0 or > 255) || RpcIds.Any(id => id is < 0 or > 255)
            || SubIds.Any(id => id is < 0 or > 65535)) return "Invalid packet, RPC or sub-ID.";
        return null;
    }
}

internal sealed record TrafficSummary(long Seq, string Direction, string Kind, int Id, int EffectiveId,
    int? SubId, string Name, string? Group, int DataBytes, long TimestampMs, string? Category = null, string? Source = null);

internal sealed record CapturedTraffic(TrafficSummary Summary, ReadOnlyMemory<byte> Data,
    int DataBitLength, int PayloadBitOffset, int PayloadBitLength, bool Control = false, bool Reconstructed = false);

internal sealed record InspectionNode(string Name, string Type, string Kind, string? Value, InspectionNode[] Children);
internal sealed record TrafficDetails(string Session, TrafficSummary Summary, InspectionNode? Parsed,
    string Parser, string? Error, int DataBitLength, int PayloadBitOffset, int PayloadBitLength, byte[] Raw, bool Reconstructed = false);
internal sealed record CaptureStats(string Session, long Captured, int Retained, long RawBytes,
    long Evicted, long DroppedBroadcastEntries, int ListeningPort, long OldestSeq);
internal sealed record CaptureExport(int SchemaVersion, string Session, CapturedTraffic[] Entries);
internal sealed record TrafficSnapshot(string Session, TrafficSummary[] Entries);
internal sealed record ApiError(string Error);

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CaptureSettings))]
[JsonSerializable(typeof(TrafficSummary[]))]
[JsonSerializable(typeof(TrafficSnapshot))]
[JsonSerializable(typeof(TrafficDetails))]
[JsonSerializable(typeof(CaptureStats))]
[JsonSerializable(typeof(CaptureExport))]
[JsonSerializable(typeof(ApiError))]
internal partial class CaptureJsonContext : JsonSerializerContext;
