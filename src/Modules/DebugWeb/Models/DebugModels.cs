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

public record class DebugServerSettingsDto(
    bool Capture, bool Incoming, bool Outgoing,
    bool Rpc, bool Packets);

public record struct PacketIdFilterDto(string Key, string Mode);

public record class PacketViewStateDto(
    string SearchText,
    bool IsPaused,
    bool AutoScroll,
    PacketIdFilterDto[] IdFilters);

public record struct WsMessage<T>(string Type, T Data);

public record struct Vec3Dto(float X, float Y, float Z);

public record struct WorldPoolUsageDto(string Key, string Name, int Count, int Max);

public record struct WorldOverviewDto(WorldPoolUsageDto[] Pools);

public record class WorldViewStateDto(
    string Section,
    string? Search,
    bool StreamZone);

public record struct WorldLocalPlayerDto(
    bool IsConnected,
    ushort Id,
    string? Name,
    Vec3Dto Position,
    float Health,
    float Armour,
    ushort? VehicleId);

public record struct WorldPlayerRowDto(
    ushort Id,
    string? Name,
    int Score,
    int Ping,
    float Health,
    float Armour,
    byte Weapon,
    ushort? VehicleId,
    byte State,
    Vec3Dto Position,
    bool IsStreamed,
    bool IsLocal,
    bool IsNpc);

public record struct WorldVehicleRowDto(
    ushort Id,
    int Model,
    float Health,
    Vec3Dto Position,
    bool HasDriver,
    bool IsOccupied,
    byte PrimaryColor,
    byte SecondaryColor,
    bool SirenEnabled);

public record struct WorldObjectRowDto(
    ushort Id,
    int Model,
    Vec3Dto Position,
    Vec3Dto Rotation,
    float DistanceToCamera,
    ushort? AttachedToVehicleId,
    ushort? AttachedToObjectId,
    int MaterialCount);

public record struct WorldPickupRowDto(
    int Index,
    int ServerId,
    int Model,
    int Type,
    Vec3Dto Position,
    float DistanceToLocal,
    bool IsWeaponPickup);

public record struct WorldLabelRowDto(
    ushort Id,
    string? Text,
    uint Color,
    Vec3Dto Position,
    float DrawDistance,
    ushort? AttachedToPlayer,
    ushort? AttachedToVehicle);

public record struct WorldTextDrawRowDto(
    ushort Id,
    string? Text,
    int Style,
    float X,
    float Y,
    ushort Model,
    uint Color);

public record struct WorldGangZoneRowDto(
    ushort Id,
    float MinX,
    float MinY,
    float MaxX,
    float MaxY,
    uint Color,
    uint AltColor,
    bool IsFlashing);

public record struct WorldActorRowDto(
    ushort Id,
    float Health,
    Vec3Dto Position,
    float Rotation,
    bool IsInvulnerable);

public record struct WorldSnapshotDto(
    long GeneratedAtMs,
    string Status,
    int GameState,
    WorldLocalPlayerDto LocalPlayer,
    WorldOverviewDto Overview,
    WorldPlayerRowDto[] Players,
    WorldVehicleRowDto[] Vehicles,
    WorldObjectRowDto[] Objects,
    WorldPickupRowDto[] Pickups,
    WorldLabelRowDto[] Labels,
    WorldTextDrawRowDto[] TextDraws,
    WorldGangZoneRowDto[] GangZones,
    WorldActorRowDto[] Actors);

[JsonSerializable(typeof(TrafficEntry[]))]
[JsonSerializable(typeof(TrafficEntry))]
[JsonSerializable(typeof(StatsResponse))]
[JsonSerializable(typeof(DebugServerSettingsDto))]
[JsonSerializable(typeof(PacketViewStateDto))]
[JsonSerializable(typeof(PacketIdFilterDto[]))]
[JsonSerializable(typeof(TopEntry[]))]
[JsonSerializable(typeof(WorldSnapshotDto))]
[JsonSerializable(typeof(WorldViewStateDto))]
[JsonSerializable(typeof(WorldPoolUsageDto[]))]
[JsonSerializable(typeof(WorldPlayerRowDto[]))]
[JsonSerializable(typeof(WorldVehicleRowDto[]))]
[JsonSerializable(typeof(WorldObjectRowDto[]))]
[JsonSerializable(typeof(WorldPickupRowDto[]))]
[JsonSerializable(typeof(WorldLabelRowDto[]))]
[JsonSerializable(typeof(WorldTextDrawRowDto[]))]
[JsonSerializable(typeof(WorldGangZoneRowDto[]))]
[JsonSerializable(typeof(WorldActorRowDto[]))]
[JsonSerializable(typeof(WsMessage<TrafficEntry>))]
[JsonSerializable(typeof(WsMessage<TrafficEntry[]>))]
[JsonSerializable(typeof(WsMessage<DebugServerSettingsDto>))]
[JsonSerializable(typeof(WsMessage<PacketViewStateDto>))]
[JsonSerializable(typeof(WsMessage<StatsResponse>))]
[JsonSerializable(typeof(WsMessage<WorldSnapshotDto>))]
[JsonSerializable(typeof(WsMessage<WorldViewStateDto>))]
[JsonSerializable(typeof(WsMessage<object?>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class DebugJsonContext : JsonSerializerContext;
