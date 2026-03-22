using System.Text.Json.Serialization;

public enum TrafficDirection { Incoming, Outgoing }
public enum TrafficKind { Rpc, Packet }

public record struct TrafficEntry(
    long Seq,
    TrafficDirection Direction,
    TrafficKind Kind,
    int Id,
    string? Name,
    string? Parsed,
    string? Detail,
    int DataBytes,
    long TimestampMs);

public record struct TopEntry(int Id, string? Name, int Count);

public record struct StatsResponse(
    int TotalInRpc, int TotalOutRpc,
    int TotalInPkt, int TotalOutPkt,
    TopEntry[] TopRpc, TopEntry[] TopPkt);

public record class ConfigDto(
    bool Capture, bool Incoming, bool Outgoing,
    bool Rpc, bool Packets);

public record struct WsMessage<T>(string Type, T Data);

[JsonSerializable(typeof(TrafficEntry[]))]
[JsonSerializable(typeof(TrafficEntry))]
[JsonSerializable(typeof(StatsResponse))]
[JsonSerializable(typeof(ConfigDto))]
[JsonSerializable(typeof(TopEntry[]))]
[JsonSerializable(typeof(WsMessage<TrafficEntry>))]
[JsonSerializable(typeof(WsMessage<TrafficEntry[]>))]
[JsonSerializable(typeof(WsMessage<ConfigDto>))]
[JsonSerializable(typeof(WsMessage<StatsResponse>))]
[JsonSerializable(typeof(WsMessage<object?>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DebugJsonContext : JsonSerializerContext;
