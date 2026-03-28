using System.Numerics;
using System.Text.Json;
using SFSharp;

public partial class DebugModule
{
    private const int WorldBroadcastIntervalMs = 500;

    private async Task WorldBroadcastLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (!_clients.IsEmpty)
                {
                    var snapshot = BuildWorldSnapshot();
                    var msg = new WsMessage<WorldSnapshotDto>("world", snapshot);
                    var json = JsonSerializer.SerializeToUtf8Bytes(msg, DebugJsonContext.Default.WsMessageWorldSnapshotDto);
                    await BroadcastRawAsync(json);
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

    private static WorldSnapshotDto BuildWorldSnapshot()
    {
        SFGamePools pools = SF.Pools;
        SFLocalPlayer localPlayer = SF.Players.Local;

        string status = pools.IsAvailable
            ? pools.IsInitialized ? "live" : "not-initialized"
            : "unavailable";

        return new WorldSnapshotDto(
            GeneratedAtMs: Environment.TickCount64,
            Status: status,
            GameState: pools.State,
            LocalPlayer: BuildWorldLocalPlayer(localPlayer),
            Overview: BuildWorldOverview(pools),
            Players: BuildWorldPlayers(),
            Vehicles: BuildWorldVehicles(),
            Objects: BuildWorldObjects(),
            Pickups: BuildWorldPickups(),
            Labels: BuildWorldLabels(),
            TextDraws: BuildWorldTextDraws(),
            GangZones: BuildWorldGangZones(),
            Actors: BuildWorldActors());
    }

    private static WorldOverviewDto BuildWorldOverview(SFGamePools pools)
    {
        return new WorldOverviewDto(
        [
            new("players", "Players", SF.Players.GetConnectedCount(), SampOffsets.CPlayerPool.MaxPlayers),
            new("vehicles", "Vehicles", SF.Vehicles.Count, SampOffsets.CVehiclePool.MaxVehicles),
            new("objects", "Objects", pools.Objects.Count, SampOffsets.CObjectPool.MaxObjects),
            new("pickups", "Pickups", pools.Pickups.Count, SampOffsets.CPickupPool.MaxPickups),
            new("labels", "Labels", pools.Labels.EnumerateIds().Count(), SampOffsets.CLabelPool.MaxLabels),
            new("textdraws", "TextDraws", pools.TextDraws.EnumerateIds().Count(), SampOffsets.CTextDrawPool.TotalTextDraws),
            new("gangzones", "GangZones", pools.GangZones.EnumerateIds().Count(), SampOffsets.CGangZonePool.MaxGangZones),
            new("actors", "Actors", pools.Actors.EnumerateIds().Count(), SampOffsets.CActorPool.MaxActors),
            new("menus", "Menus", pools.Menus.EnumerateIds().Count(), SampOffsets.CMenuPool.MaxMenus),
        ]);
    }

    private static WorldLocalPlayerDto BuildWorldLocalPlayer(SFLocalPlayer localPlayer)
    {
        SFPed? ped = localPlayer.Ped;
        return new WorldLocalPlayerDto(
            IsConnected: localPlayer.IsConnected,
            Id: localPlayer.Id,
            Name: localPlayer.Name,
            Position: ToDto(localPlayer.Position ?? Vector3.Zero),
            Health: ped?.Health ?? 0f,
            Armour: ped?.Armour ?? 0f,
            VehicleId: localPlayer.CurrentVehicleId == ushort.MaxValue ? null : localPlayer.CurrentVehicleId);
    }

    private static WorldPlayerRowDto[] BuildWorldPlayers()
    {
        return [.. SF.Players.EnumeratePlayers().Select(playerSnapshot =>
        {
            SFPlayer player = playerSnapshot.IsLocal ? SF.Players.Local : SF.Players.GetRemote(playerSnapshot.Id);
            SFPed? ped = player.Ped;
            return new WorldPlayerRowDto(
                Id: playerSnapshot.Id,
                Name: playerSnapshot.Name,
                Score: playerSnapshot.Score,
                Ping: playerSnapshot.Ping,
                Health: ped?.Health ?? 0f,
                Armour: ped?.Armour ?? 0f,
                Weapon: ped?.CurrentWeapon ?? 0,
                VehicleId: ResolveVehicleId(player),
                State: ped?.State ?? 0,
                Position: ToDto(player.Position ?? Vector3.Zero),
                IsLocal: playerSnapshot.IsLocal,
                IsNpc: playerSnapshot.IsNpc);
        })];
    }

    private static WorldVehicleRowDto[] BuildWorldVehicles()
    {
        return [.. SF.Vehicles.Enumerate().Select(vehicle => new WorldVehicleRowDto(
            Id: vehicle.Id ?? ushort.MaxValue,
            Model: vehicle.ModelIndex,
            Health: vehicle.Health,
            Position: ToDto(vehicle.Position),
            HasDriver: vehicle.HasDriver,
            IsOccupied: vehicle.IsOccupied,
            PrimaryColor: vehicle.PrimaryColor,
            SecondaryColor: vehicle.SecondaryColor,
            SirenEnabled: vehicle.SirenEnabled))];
    }

    private static WorldObjectRowDto[] BuildWorldObjects()
    {
        return [.. SF.Pools.Objects.Enumerate().Select(worldObject => new WorldObjectRowDto(
            Id: worldObject.Id,
            Model: worldObject.Model,
            Position: ToDto(worldObject.Position),
            Rotation: ToDto(worldObject.Rotation),
            DistanceToCamera: worldObject.DistanceToCamera,
            AttachedToVehicleId: worldObject.AttachedToVehicleId == ushort.MaxValue ? null : worldObject.AttachedToVehicleId,
            AttachedToObjectId: worldObject.AttachedToObjectId == ushort.MaxValue ? null : worldObject.AttachedToObjectId,
            MaterialCount: worldObject.EnumerateMaterialSlots().Count))];
    }

    private static WorldPickupRowDto[] BuildWorldPickups()
    {
        Vector3 localPosition = SF.Players.Local.Position ?? Vector3.Zero;
        return [.. SF.Pools.Pickups.Enumerate().Select(pickup => new WorldPickupRowDto(
            Index: pickup.Index,
            ServerId: pickup.ServerId,
            Model: pickup.Model,
            Type: pickup.Type,
            Position: ToDto(pickup.Position),
            DistanceToLocal: Vector3.Distance(localPosition, pickup.Position),
            IsWeaponPickup: pickup.IsWeaponPickup))];
    }

    private static WorldLabelRowDto[] BuildWorldLabels()
    {
        return [.. SF.Pools.Labels.Enumerate().Select(label => new WorldLabelRowDto(
            Id: label.Id,
            Text: label.Text,
            Color: label.Color,
            Position: ToDto(label.Position),
            DrawDistance: label.DrawDistance,
            AttachedToPlayer: label.AttachedToPlayer == ushort.MaxValue ? null : label.AttachedToPlayer,
            AttachedToVehicle: label.AttachedToVehicle == ushort.MaxValue ? null : label.AttachedToVehicle))];
    }

    private static WorldTextDrawRowDto[] BuildWorldTextDraws()
    {
        return [.. SF.Pools.TextDraws.Enumerate().Select(textDraw => new WorldTextDrawRowDto(
            Id: textDraw.Id,
            Text: textDraw.Text ?? textDraw.String,
            Style: textDraw.Style,
            X: textDraw.X,
            Y: textDraw.Y,
            Model: textDraw.Model,
            Color: textDraw.Data.LetterColor))];
    }

    private static WorldGangZoneRowDto[] BuildWorldGangZones()
    {
        return [.. SF.Pools.GangZones.Enumerate().Select(zone => new WorldGangZoneRowDto(
            Id: zone.Id,
            MinX: zone.Rect.Left,
            MinY: zone.Rect.Top,
            MaxX: zone.Rect.Right,
            MaxY: zone.Rect.Bottom,
            Color: zone.Color,
            IsFlashing: zone.IsFlashing))];
    }

    private static WorldActorRowDto[] BuildWorldActors()
    {
        return [.. SF.Pools.Actors.Enumerate().Select(actor => new WorldActorRowDto(
            Id: actor.Id,
            Health: actor.Health,
            Position: ToDto(actor.Position),
            Rotation: actor.EulerInverted.Z,
            IsInvulnerable: actor.IsInvulnerable))];
    }

    private static ushort? ResolveVehicleId(SFPlayer player)
    {
        return player switch
        {
            SFLocalPlayer localPlayer when localPlayer.CurrentVehicleId != ushort.MaxValue => localPlayer.CurrentVehicleId,
            SFRemotePlayer remotePlayer when remotePlayer.VehicleId != ushort.MaxValue => remotePlayer.VehicleId,
            _ => player.Vehicle?.Id
        };
    }

    private static Vec3Dto ToDto(Vector3 value)
    {
        return new Vec3Dto(value.X, value.Y, value.Z);
    }
}
