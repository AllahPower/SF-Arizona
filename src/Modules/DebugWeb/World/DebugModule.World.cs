using System.Net.WebSockets;
using System.Numerics;
using System.Text.Json;

namespace SFSharp.Runtime.Modules;

public partial class DebugModule
{
    private const int WorldBroadcastIntervalMs = 500;
    private static class WorldSectionKeys
    {
        public const string Overview = "overview";
        public const string Players = "players";
        public const string Vehicles = "vehicles";
        public const string Objects = "objects";
        public const string Pickups = "pickups";
        public const string Labels = "labels";
        public const string TextDraws = "textdraws";
        public const string GangZones = "gangzones";
        public const string Actors = "actors";
    }

    private async Task BroadcastWorldUpdatesLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!_clients.IsEmpty)
                {
                    foreach (var (id, client) in _clients)
                    {
                        if (client.Socket.State != WebSocketState.Open)
                        {
                            _clients.TryRemove(id, out _);
                            continue;
                        }

                        await SendWorldSnapshotAsync(client);
                    }
                }

                await Task.Delay(WorldBroadcastIntervalMs, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                SFLog.Error($"WorldBroadcastLoop error: {ex.Message}");
            }
        }
    }

    private async Task SendWorldSnapshotAsync(DebugClientSession client)
    {
        WorldSnapshotDto snapshot = await CaptureWorldSnapshotOnMainThreadAsync(client.WorldView);
        var msg = new WsMessage<WorldSnapshotDto>("world", snapshot);
        byte[] json = JsonSerializer.SerializeToUtf8Bytes(msg, DebugJsonContext.Default.WsMessageWorldSnapshotDto);
        await SendRawAsync(client, json);
    }

    private static Task<WorldSnapshotDto> CaptureWorldSnapshotOnMainThreadAsync(WorldViewState request)
    {
        TaskCompletionSource<WorldSnapshotDto> tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        SFBootstrap.PostToMainThread(() =>
        {
            try
            {
                tcs.SetResult(CaptureWorldSnapshot(request));
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        });
        return tcs.Task;
    }

    private static WorldSnapshotDto CaptureWorldSnapshot(WorldViewState request)
    {
        string activeSection = NormalizeWorldSectionKey(request.Section);
        string? normalizedSearch = NormalizeWorldQueryText(request.SearchText);
        SFGamePools pools = SF.Pools;
        SFLocalPlayer localPlayer = SF.Players.Local;

        string status = pools.IsAvailable
            ? pools.IsInitialized ? "live" : "not-initialized"
            : "unavailable";

        return new WorldSnapshotDto(
            GeneratedAtMs: Environment.TickCount64,
            Status: status,
            GameState: pools.State,
            LocalPlayer: CaptureLocalPlayerState(localPlayer),
            Overview: CreateWorldOverview(pools),
            Players: activeSection == WorldSectionKeys.Players ? CreatePlayerRows(normalizedSearch, request.StreamZoneOnly) : [],
            Vehicles: activeSection == WorldSectionKeys.Vehicles ? CreateVehicleRows(normalizedSearch) : [],
            Objects: activeSection == WorldSectionKeys.Objects ? CreateObjectRows(normalizedSearch) : [],
            Pickups: activeSection == WorldSectionKeys.Pickups ? CreatePickupRows(normalizedSearch) : [],
            Labels: activeSection == WorldSectionKeys.Labels ? CreateLabelRows(normalizedSearch) : [],
            TextDraws: activeSection == WorldSectionKeys.TextDraws ? CreateTextDrawRows(normalizedSearch) : [],
            GangZones: activeSection == WorldSectionKeys.GangZones ? CreateGangZoneRows(normalizedSearch) : [],
            Actors: activeSection == WorldSectionKeys.Actors ? CreateActorRows(normalizedSearch) : []);
    }

    private static WorldOverviewDto CreateWorldOverview(SFGamePools pools)
    {
        return new WorldOverviewDto(
        [
            new("players", "Players", SF.Players.GetConnectedCount(), SampOffsets.CPlayerPool.MaxPlayers),
            new("vehicles", "Vehicles", SF.Vehicles.Count, SampOffsets.CVehiclePool.MaxVehicles),
            new("objects", "Objects", pools.Objects.Count, SampOffsets.CObjectPool.MaxObjects),
            new("pickups", "Pickups", pools.Pickups.Count, SampOffsets.CPickupPool.MaxPickups),
            new("labels", "Labels", pools.Labels.EnumerateIds().Count(), SampOffsets.CLabelPool.MaxLabels),
            new("textdraws", "TextDraws", GetEffectiveTextDrawCount(pools), SampOffsets.CTextDrawPool.TotalTextDraws),
            new("gangzones", "GangZones", pools.GangZones.EnumerateIds().Count(), SampOffsets.CGangZonePool.MaxGangZones),
            new("actors", "Actors", pools.Actors.EnumerateIds().Count(), SampOffsets.CActorPool.MaxActors),
            new("menus", "Menus", pools.Menus.EnumerateIds().Count(), SampOffsets.CMenuPool.MaxMenus),
        ]);
    }

    private static WorldLocalPlayerDto CaptureLocalPlayerState(SFLocalPlayer localPlayer)
    {
        SFPed? ped = localPlayer.Ped;
        return new WorldLocalPlayerDto(
            IsConnected: localPlayer.IsConnected,
            Id: localPlayer.Id,
            Name: localPlayer.Name,
            Position: ToWorldVec3(localPlayer.Position ?? Vector3.Zero),
            Health: Sanitize(ped?.Health ?? 0f),
            Armour: Sanitize(ped?.Armour ?? 0f),
            VehicleId: localPlayer.CurrentVehicleId == ushort.MaxValue ? null : localPlayer.CurrentVehicleId);
    }

    private static WorldPlayerRowDto[] CreatePlayerRows(string? search, bool streamZoneOnly)
    {
        List<WorldPlayerRowDto> rows = [];
        foreach (SFPlayerSnapshot playerSnapshot in SF.Players.EnumeratePlayers())
        {
            if (!MatchesWorldQuery(search, playerSnapshot.Id, playerSnapshot.Name))
            {
                continue;
            }

            SFPlayer player = playerSnapshot.IsLocal ? SF.Players.Local : SF.Players.GetRemote(playerSnapshot.Id);
            SFPed? ped = player.Ped;
            bool isStreamed = ped is not null && ped.ExistsInGame;
            if (streamZoneOnly && !playerSnapshot.IsLocal && !isStreamed)
            {
                continue;
            }

            SFPedSnapshot pedSnapshot = default;
            bool hasPedSnapshot = ped is not null && ped.TryGetSnapshot(out pedSnapshot);

            rows.Add(new WorldPlayerRowDto(
                Id: playerSnapshot.Id,
                Name: playerSnapshot.Name,
                Score: playerSnapshot.Score,
                Ping: playerSnapshot.Ping,
                Health: Sanitize(hasPedSnapshot ? pedSnapshot.Health : 0f),
                Armour: Sanitize(hasPedSnapshot ? pedSnapshot.Armour : 0f),
                Weapon: hasPedSnapshot ? pedSnapshot.CurrentWeapon : (byte)0,
                VehicleId: ResolveObservedVehicleId(player),
                State: hasPedSnapshot ? pedSnapshot.State : (byte)0,
                Position: ToWorldVec3(hasPedSnapshot ? pedSnapshot.Position : Vector3.Zero),
                IsStreamed: hasPedSnapshot && isStreamed,
                IsLocal: playerSnapshot.IsLocal,
                IsNpc: playerSnapshot.IsNpc));
        }

        return [.. rows];
    }

    private static WorldVehicleRowDto[] CreateVehicleRows(string? search)
    {
        return [.. SF.Vehicles.Enumerate().Select(vehicle => new WorldVehicleRowDto(
            Id: vehicle.Id ?? ushort.MaxValue,
            Model: vehicle.ModelIndex,
            Health: Sanitize(vehicle.Health),
            Position: ToWorldVec3(vehicle.Position),
            HasDriver: vehicle.HasDriver,
            IsOccupied: vehicle.IsOccupied,
            PrimaryColor: vehicle.PrimaryColor,
            SecondaryColor: vehicle.SecondaryColor,
            SirenEnabled: vehicle.SirenEnabled))
            .Where(vehicle => MatchesWorldQuery(search, vehicle.Id, vehicle.Model))];
    }

    private static WorldObjectRowDto[] CreateObjectRows(string? search)
    {
        return [.. SF.Pools.Objects.Enumerate().Select(worldObject => new WorldObjectRowDto(
            Id: worldObject.Id,
            Model: worldObject.Model,
            Position: ToWorldVec3(worldObject.Position),
            Rotation: ToWorldVec3(worldObject.Rotation),
            DistanceToCamera: Sanitize(worldObject.DistanceToCamera),
            AttachedToVehicleId: worldObject.AttachedToVehicleId == ushort.MaxValue ? null : worldObject.AttachedToVehicleId,
            AttachedToObjectId: worldObject.AttachedToObjectId == ushort.MaxValue ? null : worldObject.AttachedToObjectId,
            MaterialCount: worldObject.EnumerateMaterialSlots().Count))
            .Where(worldObject => MatchesWorldQuery(search, worldObject.Id, worldObject.Model))];
    }

    private static WorldPickupRowDto[] CreatePickupRows(string? search)
    {
        Vector3 localPosition = SF.Players.Local.Position ?? Vector3.Zero;
        return [.. SF.Pools.Pickups.Enumerate().Select(pickup => new WorldPickupRowDto(
            Index: pickup.Index,
            ServerId: pickup.ServerId,
            Model: pickup.Model,
            Type: pickup.Type,
            Position: ToWorldVec3(pickup.Position),
            DistanceToLocal: Sanitize(Vector3.Distance(localPosition, pickup.Position)),
            IsWeaponPickup: pickup.IsWeaponPickup))
            .Where(pickup => MatchesWorldQuery(search, pickup.Index, pickup.ServerId, pickup.Model))];
    }

    private static WorldLabelRowDto[] CreateLabelRows(string? search)
    {
        List<WorldLabelRowDto> rows = [];
        foreach (ushort labelId in SF.Pools.Labels.EnumerateIds())
        {
            if (!SF.Pools.Labels.TryGetSnapshot(labelId, out SFLabelSnapshot label))
            {
                continue;
            }

            rows.Add(new WorldLabelRowDto(
                Id: labelId,
                Text: label.Text,
                Color: label.Color,
                Position: ToWorldVec3(label.Position),
                DrawDistance: Sanitize(label.DrawDistance),
                AttachedToPlayer: label.AttachedToPlayer == ushort.MaxValue ? null : label.AttachedToPlayer,
                AttachedToVehicle: label.AttachedToVehicle == ushort.MaxValue ? null : label.AttachedToVehicle));
        }

        return [.. rows.Where(label => MatchesWorldQuery(search, label.Id, label.Text))];
    }

    private static WorldTextDrawRowDto[] CreateTextDrawRows(string? search)
    {
        List<WorldTextDrawRowDto> rows = [];
        foreach (ushort textDrawId in SF.Pools.TextDraws.EnumerateIds())
        {
            if (!SF.Pools.TextDraws.TryGetSnapshot(textDrawId, out SFTextDrawSnapshot textDraw))
            {
                continue;
            }

            rows.Add(new WorldTextDrawRowDto(
                Id: textDrawId,
                Text: textDraw.Text ?? textDraw.String,
                Style: textDraw.Data.Style,
                X: Sanitize(textDraw.Data.X),
                Y: Sanitize(textDraw.Data.Y),
                Model: textDraw.Data.Model,
                Color: textDraw.Data.LetterColor));
        }

        return [.. rows.Where(textDraw => MatchesWorldQuery(search, textDraw.Id, textDraw.Text, textDraw.Model))];
    }

    private static int GetEffectiveTextDrawCount(SFGamePools pools)
    {
        return pools.TextDraws.EnumerateIds().Count();
    }

    private static WorldGangZoneRowDto[] CreateGangZoneRows(string? search)
    {
        return [.. SF.Pools.GangZones.Enumerate().Select(zone => new WorldGangZoneRowDto(
            Id: zone.Id,
            MinX: Sanitize(zone.Rect.Left),
            MinY: Sanitize(zone.Rect.Top),
            MaxX: Sanitize(zone.Rect.Right),
            MaxY: Sanitize(zone.Rect.Bottom),
            Color: zone.Color,
            AltColor: zone.AltColor,
            IsFlashing: zone.IsFlashing))
            .Where(zone => MatchesWorldQuery(search, zone.Id))];
    }

    private static WorldActorRowDto[] CreateActorRows(string? search)
    {
        return [.. SF.Pools.Actors.Enumerate().Select(actor => new WorldActorRowDto(
            Id: actor.Id,
            Health: Sanitize(actor.Health),
            Position: ToWorldVec3(actor.Position),
            Rotation: Sanitize(actor.EulerInverted.Z),
            IsInvulnerable: actor.IsInvulnerable))
            .Where(actor => MatchesWorldQuery(search, actor.Id))];
    }

    private static ushort? ResolveObservedVehicleId(SFPlayer player)
    {
        return player switch
        {
            SFLocalPlayer localPlayer when localPlayer.CurrentVehicleId != ushort.MaxValue => localPlayer.CurrentVehicleId,
            SFRemotePlayer remotePlayer when remotePlayer.VehicleId != ushort.MaxValue => remotePlayer.VehicleId,
            _ => player.Vehicle?.Id
        };
    }

    private static Vec3Dto ToWorldVec3(Vector3 value)
    {
        return new Vec3Dto(Sanitize(value.X), Sanitize(value.Y), Sanitize(value.Z));
    }

    private static float Sanitize(float value)
    {
        return float.IsFinite(value) ? value : 0f;
    }

    private static bool MatchesWorldQuery(string? search, params object?[] values)
    {
        if (string.IsNullOrWhiteSpace(search))
        {
            return true;
        }

        foreach (object? value in values)
        {
            if (value is null)
            {
                continue;
            }

            if (value.ToString()?.Contains(search, StringComparison.OrdinalIgnoreCase) == true)
            {
                return true;
            }
        }

        return false;
    }

    private static string NormalizeWorldSectionKey(string? section)
    {
        return section switch
        {
            WorldSectionKeys.Players or
            WorldSectionKeys.Vehicles or
            WorldSectionKeys.Objects or
            WorldSectionKeys.Pickups or
            WorldSectionKeys.Labels or
            WorldSectionKeys.TextDraws or
            WorldSectionKeys.GangZones or
            WorldSectionKeys.Actors => section,
            _ => WorldSectionKeys.Overview
        };
    }

    private static string? NormalizeWorldQueryText(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return null;
        }

        string trimmed = searchText.Trim();
        return trimmed.Length > 64 ? trimmed[..64] : trimmed;
    }
}
