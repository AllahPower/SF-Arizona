using System.Collections.Generic;
using System.Numerics;

namespace SFSharp;

// ---- Incoming RPC records (server -> client) ----

public readonly record struct SetPlayerNameRpc(ushort PlayerId, string Name, bool Success);
public readonly record struct SetPlayerPosRpc(Vector3 Position);
public readonly record struct SetPlayerPosFindZRpc(Vector3 Position);
public readonly record struct SetPlayerHealthRpc(float Health);
public readonly record struct TogglePlayerControllableRpc(bool Controllable);
public readonly record struct PlaySoundRpc(int SoundId, Vector3 Position);
public readonly record struct SetWorldBoundsRpc(float MaxX, float MinX, float MaxY, float MinY);
public readonly record struct GivePlayerMoneyRpc(int Money);
public readonly record struct SetPlayerFacingAngleRpc(float Angle);
public readonly record struct ResetPlayerMoneyRpc();
public readonly record struct ResetPlayerWeaponsRpc();
public readonly record struct GivePlayerWeaponRpc(int WeaponId, int Ammo);
public readonly record struct SetVehicleParamsExRpc(ushort VehicleId, VehicleParamsExStatusRpc Params, VehicleDoorStateRpc Doors, VehicleDoorStateRpc Windows);
public readonly record struct CancelEditRpc();
public readonly record struct SetPlayerTimeRpc(byte Hour, byte Minute);
public readonly record struct ToggleClockRpc(bool State);
public readonly record struct WorldPlayerAddRpc(ushort PlayerId, byte Team, int Model, Vector3 Position, float Rotation, int Color, byte FightingStyle);
public readonly record struct SetShopNameRpc(string Name);
public readonly record struct SetPlayerSkillLevelRpc(ushort PlayerId, int Skill, ushort Level);
public readonly record struct SetPlayerDrunkLevelRpc(int DrunkLevel);
public readonly record struct Create3DTextLabelRpc(ushort Id, int Color, Vector3 Position, float Distance, bool TestLOS, ushort AttachedPlayerId, ushort AttachedVehicleId, string Text);
public readonly record struct DisableCheckpointRpc();
public readonly record struct SetRaceCheckpointRpc(byte Type, Vector3 CurrentPosition, Vector3 NextPosition, float Size);
public readonly record struct DisableRaceCheckpointRpc();
public readonly record struct GameModeRestartRpc();
public readonly record struct PlayAudioStreamRpc(string Url, Vector3 Position, float Radius, bool UsePosition);
public readonly record struct StopAudioStreamRpc();
public readonly record struct RemoveBuildingForPlayerRpc(int ModelId, Vector3 Position, float Radius);
public sealed class CreateObjectRpc
{
    public required ushort ObjectId { get; init; }
    public required int ModelId { get; init; }
    public required Vector3 Position { get; init; }
    public required Vector3 Rotation { get; init; }
    public required float DrawDistance { get; init; }
    public required bool NoCameraCollision { get; init; }
    public required ushort AttachToVehicleId { get; init; }
    public required ushort AttachToObjectId { get; init; }
    public Vector3? AttachOffsets { get; init; }
    public Vector3? AttachRotation { get; init; }
    public bool? SyncRotation { get; init; }
    public required byte TexturesCount { get; init; }
    public required List<ObjectMaterialTextureRpc> Materials { get; init; }
    public required List<ObjectMaterialTextRpc> MaterialText { get; init; }
}
public readonly record struct SetObjectPosRpc(ushort ObjectId, Vector3 Position);
public readonly record struct SetObjectRotRpc(ushort ObjectId, Vector3 Rotation);
public readonly record struct DestroyObjectRpc(ushort ObjectId);
public readonly record struct DeathMessageRpc(ushort KillerPlayerId, ushort KilledPlayerId, byte WeaponId);
public readonly record struct SetPlayerMapIconRpc(byte IconId, Vector3 Position, byte Type, int Color, byte Style);
public readonly record struct RemoveVehicleComponentRpc(ushort VehicleId, ushort ComponentId);
public readonly record struct Destroy3DTextLabelRpc(ushort TextLabelId);
public readonly record struct ChatBubbleRpc(ushort PlayerId, int Color, float Distance, int Duration, string Message);
public readonly record struct UpdateTimeRpc(int Time);
public readonly record struct ShowDialogRpc(ushort DialogId, DialogStyle Style, string Title, string LeftButton, string RightButton, string Text);
public readonly record struct DestroyPickupRpc(int Id);
public readonly record struct LinkVehicleToInteriorRpc(ushort VehicleId, byte InteriorId);
public readonly record struct SetPlayerArmourRpc(float Armour);
public readonly record struct SetPlayerArmedWeaponRpc(int WeaponId);
public readonly record struct SetSpawnInfoRpc(byte Team, int Skin, byte Unused, Vector3 Position, float Rotation, int Weapon1, int Weapon2, int Weapon3, int Ammo1, int Ammo2, int Ammo3);
public readonly record struct SetPlayerTeamRpc(ushort PlayerId, byte TeamId);
public readonly record struct PutPlayerInVehicleRpc(ushort VehicleId, byte SeatId);
public readonly record struct RemovePlayerFromVehicleRpc();
public readonly record struct SetPlayerColorRpc(ushort PlayerId, int Color);
public readonly record struct DisplayGameTextRpc(int Style, int Time, string Text);
public readonly record struct ForceClassSelectionRpc();
public readonly record struct AttachObjectToPlayerRpc(ushort ObjectId, ushort PlayerId, Vector3 Offsets, Vector3 Rotation);
public sealed class InitMenuRpc
{
    public required byte MenuId { get; init; }
    public required string Title { get; init; }
    public required float X { get; init; }
    public required float Y { get; init; }
    public required bool TwoColumns { get; init; }
    public required List<MenuColumnRpc> Columns { get; init; }
    public required int[] Rows { get; init; }
    public required bool Menu { get; init; }
}
public readonly record struct ShowMenuRpc(byte MenuId);
public readonly record struct HideMenuRpc(byte MenuId);
public readonly record struct CreateExplosionRpc(Vector3 Position, int Style, float Radius);
public readonly record struct ShowPlayerNameTagRpc(ushort PlayerId, bool Show);
public readonly record struct AttachCameraToObjectRpc(ushort ObjectId);
public readonly record struct InterpolateCameraRpc(bool SetPos, Vector3 FromPos, Vector3 DestPos, int Time, byte Mode);
public sealed class SetObjectMaterialRpc
{
    public required ushort ObjectId { get; init; }
    public required ObjectMaterialRpc Material { get; init; }
}
public readonly record struct GangZoneStopFlashRpc(ushort ZoneId);
public readonly record struct ApplyPlayerAnimationRpc(ushort PlayerId, string AnimLib, string AnimName, float FrameDelta, bool Loop, bool LockX, bool LockY, bool Freeze, int Time);
public readonly record struct ClearPlayerAnimationsRpc(ushort PlayerId);
public readonly record struct SetPlayerSpecialActionRpc(byte ActionId);
public readonly record struct SetPlayerFightingStyleRpc(ushort PlayerId, byte StyleId);
public readonly record struct SetPlayerVelocityRpc(Vector3 Velocity);
public readonly record struct SetVehicleVelocityRpc(bool Turn, Vector3 Velocity);
public readonly record struct SetPlayerDrunkVisualsRpc(int Level);
public readonly record struct ClientMessageRpc(uint Color, string Text);
public readonly record struct SetWorldTimeRpc(byte Hour);
public readonly record struct CreatePickupRpc(int Id, int Model, int PickupType, Vector3 Position);
public readonly record struct SetVehicleTiresRpc(ushort VehicleId, byte Tires);
public readonly record struct MoveObjectRpc(ushort ObjectId, Vector3 FromPos, Vector3 DestPos, float Speed, Vector3 Rotation);
public readonly record struct EnableStuntBonusRpc(bool State);
public readonly record struct TextDrawSetStringRpc(ushort Id, string Text);
public readonly record struct SetCheckpointRpc(Vector3 Position, float Radius);
public readonly record struct CreateGangZoneRpc(ushort ZoneId, float StartX, float StartY, float EndX, float EndY, int Color);
public readonly record struct ToggleWidescreenRpc(bool Enable);
public readonly record struct PlayCrimeReportRpc(ushort SuspectId, int InVehicle, int VehicleModel, int VehicleColor, int Crime, Vector3 Coordinates);
public readonly record struct GangZoneDestroyRpc(ushort ZoneId);
public readonly record struct GangZoneFlashRpc(ushort ZoneId, int Color);
public readonly record struct StopObjectRpc(ushort ObjectId);
public readonly record struct SetNumberPlateRpc(ushort VehicleId, string Text);
public readonly record struct TogglePlayerSpectatingRpc(int State);
public readonly record struct PlayerSpectatePlayerRpc(ushort PlayerId, byte CamType);
public readonly record struct PlayerSpectateVehicleRpc(ushort VehicleId, byte CamType);
public readonly record struct ConnectionRejectedRpc(byte Reason);
public readonly record struct SetPlayerWantedLevelRpc(byte WantedLevel);
public sealed class ShowTextDrawRpc
{
    public required ushort TextDrawId { get; init; }
    public required ShowTextDrawDataRpc TextDraw { get; init; }
}
public readonly record struct HideTextDrawRpc(ushort TextDrawId);
public readonly record struct ServerJoinRpc(ushort PlayerId, int Color, bool IsNpc, string Nickname);
public readonly record struct ServerQuitRpc(ushort PlayerId, byte Reason);
public sealed class InitGameRpc
{
    public required ushort PlayerId { get; init; }
    public required string HostName { get; init; }
    public required InitGameSettingsRpc Settings { get; init; }
    public required byte[] VehicleModels { get; init; }
    public required bool VehicleFriendlyFire { get; init; }
}
public readonly record struct RemovePlayerMapIconRpc(byte IconId);
public readonly record struct SetPlayerAmmoRpc(byte WeaponId, ushort Ammo);
public readonly record struct SetGravityRpc(float Gravity);
public readonly record struct SetVehicleHealthRpc(ushort VehicleId, float Health);
public readonly record struct AttachTrailerToVehicleRpc(ushort TrailerId, ushort VehicleId);
public readonly record struct DetachTrailerFromVehicleRpc(ushort VehicleId);
public readonly record struct SetPlayerDrunkHandlingRpc(int Level);
public readonly record struct SetWeatherRpc(byte WeatherId);
public readonly record struct SetPlayerSkinRpc(int PlayerId, int SkinId);
public readonly record struct SetPlayerInteriorRpc(byte Interior);
public readonly record struct SetPlayerCameraPosRpc(Vector3 Position);
public readonly record struct SetPlayerCameraLookAtRpc(Vector3 LookAt, byte CutType);
public readonly record struct SetVehiclePosRpc(ushort VehicleId, Vector3 Position);
public readonly record struct SetVehicleZAngleRpc(ushort VehicleId, float Angle);
public readonly record struct SetVehicleParamsForPlayerRpc(ushort VehicleId, bool Objective, bool DoorsLocked);
public readonly record struct SetCameraBehindPlayerRpc();
public readonly record struct WorldPlayerRemoveRpc(ushort PlayerId);
public sealed class WorldVehicleAddRpc
{
    public required ushort VehicleId { get; init; }
    public required WorldVehicleInfoRpc Data { get; init; }
}
public readonly record struct WorldVehicleRemoveRpc(ushort VehicleId);
public readonly record struct WorldPlayerDeathRpc(ushort PlayerId);
public readonly record struct DisableVehicleCollisionsRpc(bool Disable);
public readonly record struct SetPlayerObjectNoCameraColRpc(ushort ObjectId);
public readonly record struct ToggleCameraTargetRpc(bool Enable);
public readonly record struct CreateActorRpc(ushort ActorId, int SkinId, Vector3 Position, float Rotation, float Health);
public readonly record struct DestroyActorRpc(ushort ActorId);
public readonly record struct ApplyActorAnimationRpc(ushort ActorId, string AnimLib, string AnimName, float FrameDelta, bool Loop, bool LockX, bool LockY, bool Freeze, int Time);
public readonly record struct ClearActorAnimationRpc(ushort ActorId);
public readonly record struct SetActorFacingAngleRpc(ushort ActorId, float Angle);
public readonly record struct SetActorPosRpc(ushort ActorId, Vector3 Position);
public readonly record struct SetActorHealthRpc(ushort ActorId, float Health);

// ---- Bidirectional incoming records ----

public readonly record struct ChatMessageRpc(string Prefix, uint PrefixColor, string Text);
public readonly record struct EnterVehicleRpc(ushort PlayerId, ushort VehicleId, bool Passenger);
public readonly record struct ExitVehicleRpc(ushort PlayerId, ushort VehicleId);
public readonly record struct ClickTextDrawIncomingRpc(bool State, int HoverColor);
public readonly record struct ScmEventIncomingRpc(ushort PlayerId, int Event, int VehicleId, int Param1, int Param2);
public readonly record struct ClientCheckIncomingRpc(byte RequestType, int Subject, ushort Offset, ushort Length);
public readonly record struct UpdateVehicleDamageStatusRpc(ushort VehicleId, int PanelDmg, int DoorDmg, byte Lights, byte Tires);
public sealed class UpdateScoresAndPingsRpc
{
    public required Dictionary<ushort, ScorePingRpc> Players { get; init; }
}
public readonly record struct EditAttachedObjectIncomingRpc(int Index);
public readonly record struct EditObjectIncomingRpc(bool PlayerObject, ushort ObjectId);
public readonly record struct RequestClassResponseRpc(bool CanSpawn, byte Team, int Skin, byte Unused, Vector3 Position, float Rotation, int Weapon1, int Weapon2, int Weapon3, int Ammo1, int Ammo2, int Ammo3);
public readonly record struct RequestSpawnResponseRpc(bool Response);
public readonly record struct DestroyWeaponPickupRpc(byte Id);
public readonly record struct SelectObjectRpc();
public readonly record struct ServerNetStatsResponseRpc();

// ---- Outgoing RPC records (client -> server) ----

public readonly record struct ClickPlayerRpc(ushort PlayerId, byte Source);
public readonly record struct ClientJoinRpc(int Version, byte Mod, string Nickname, int ChallengeResponse, string JoinAuthKey, string ClientVer, int ChallengeResponse2);
public readonly record struct SendCommandRpc(string Command);
public readonly record struct SpawnRpc();
public readonly record struct DeathNotificationRpc(byte Reason, ushort KillerId);
public readonly record struct DialogResponseRpc(ushort DialogId, byte Button, ushort ListboxId, string Input);
public readonly record struct SendClickTextDrawRpc(ushort TextDrawId);
public readonly record struct ScmEventOutgoingRpc(int VehicleId, int Param1, int Param2, int Event);
public readonly record struct SendChatRpc(string Message);
public readonly record struct ClientCheckResponseRpc(byte RequestType, int Result1, byte Result2);
public readonly record struct SendVehicleDamageStatusRpc(ushort VehicleId, int PanelDmg, int DoorDmg, byte Lights, byte Tires);
public readonly record struct GiveTakeDamageRpc(bool Take, ushort PlayerId, float Damage, int Weapon, int Bodypart);
public readonly record struct EditAttachedObjectOutgoingRpc(int Response, int Index, int Model, int Bone, Vector3 Position, Vector3 Rotation, Vector3 Scale, int Color1, int Color2);
public readonly record struct EditObjectOutgoingRpc(bool PlayerObject, ushort ObjectId, int Response, Vector3 Position, Vector3 Rotation);
public readonly record struct SendEnterVehicleRpc(ushort VehicleId, bool Passenger);
public readonly record struct SendExitVehicleRpc(ushort VehicleId);
public readonly record struct SetInteriorIdRpc(byte Interior);
public readonly record struct MapMarkerRpc(Vector3 Position);
public readonly record struct SendRequestClassRpc(int ClassId);
public readonly record struct SendRequestSpawnRpc();
public readonly record struct PickedUpPickupRpc(int PickupId);
public readonly record struct MenuSelectRpc(byte Row);
public readonly record struct MenuQuitRpc();
public readonly record struct VehicleDestroyedRpc(ushort VehicleId);
public readonly record struct NpcJoinRpc(int Version, byte Mod, string Nickname, int ChallengeResponse);
public readonly record struct CameraTargetUpdateRpc(ushort ObjectId, ushort VehicleId, ushort PlayerId, ushort ActorId);
public readonly record struct GiveActorDamageRpc(bool Unused, ushort ActorId, float Damage, int Weapon, int Bodypart);
public readonly record struct SelectObjectOutgoingRpc(int Type, ushort ObjectId, int Model, Vector3 Position);
public readonly record struct UpdateScoresAndPingsOutgoingRpc();

public readonly record struct VehicleParamsExStatusRpc(byte Engine, byte Lights, byte Alarm, byte Doors, byte Bonnet, byte Boot, byte Objective, byte Unknown);
public readonly record struct VehicleDoorStateRpc(byte Driver, byte Passenger, byte BackLeft, byte BackRight);
public readonly record struct ScorePingRpc(int Score, int Ping);

public sealed record class InitGameSettingsRpc
{
    public required bool ZoneNames { get; init; }
    public required bool UseCJWalk { get; init; }
    public required bool AllowWeapons { get; init; }
    public required bool LimitGlobalChatRadius { get; init; }
    public required float GlobalChatRadius { get; init; }
    public required bool StuntBonus { get; init; }
    public required float NametagDrawDistance { get; init; }
    public required bool DisableEnterExits { get; init; }
    public required bool NametagLineOfSight { get; init; }
    public required bool TirePopping { get; init; }
    public required int ClassesAvailable { get; init; }
    public required bool ShowPlayerTags { get; init; }
    public required int PlayerMarkersMode { get; init; }
    public required byte WorldTime { get; init; }
    public required byte WorldWeather { get; init; }
    public required float Gravity { get; init; }
    public required bool LanMode { get; init; }
    public required int DeathMoneyDrop { get; init; }
    public required bool Instagib { get; init; }
    public required int NormalOnfootSendrate { get; init; }
    public required int NormalIncarSendrate { get; init; }
    public required int NormalFiringSendrate { get; init; }
    public required int SendMultiplier { get; init; }
    public required int LagCompMode { get; init; }
    public required bool VehicleFriendlyFire { get; init; }
}

public sealed class MenuColumnRpc
{
    public required string Title { get; init; }
    public required float Width { get; init; }
    public required List<string> Text { get; init; }
}

public enum ObjectMaterialType : byte
{
    None = 0,
    Texture = 1,
    Text = 2,
}

public abstract class ObjectMaterialRpc
{
    public required ObjectMaterialType Type { get; init; }
    public required byte MaterialId { get; init; }
}

public sealed class ObjectMaterialTextureRpc : ObjectMaterialRpc
{
    public required ushort ModelId { get; init; }
    public required string LibraryName { get; init; }
    public required string TextureName { get; init; }
    public required int Color { get; init; }
}

public sealed class ObjectMaterialTextRpc : ObjectMaterialRpc
{
    public required byte MaterialSize { get; init; }
    public required string FontName { get; init; }
    public required byte FontSize { get; init; }
    public required byte Bold { get; init; }
    public required int FontColor { get; init; }
    public required int BackgroundColor { get; init; }
    public required byte Align { get; init; }
    public required string Text { get; init; }
}

public sealed class ShowTextDrawDataRpc
{
    public required byte Flags { get; init; }
    public required float LetterWidth { get; init; }
    public required float LetterHeight { get; init; }
    public required int LetterColor { get; init; }
    public required float LineWidth { get; init; }
    public required float LineHeight { get; init; }
    public required int BoxColor { get; init; }
    public required byte Shadow { get; init; }
    public required byte Outline { get; init; }
    public required int BackgroundColor { get; init; }
    public required byte Style { get; init; }
    public required byte Selectable { get; init; }
    public required Vector2 Position { get; init; }
    public required ushort ModelId { get; init; }
    public required Vector3 Rotation { get; init; }
    public required float Zoom { get; init; }
    public required short Color1 { get; init; }
    public required short Color2 { get; init; }
    public required string Text { get; init; }
}

public sealed class WorldVehicleInfoRpc
{
    public required int Type { get; init; }
    public required Vector3 Position { get; init; }
    public required float Rotation { get; init; }
    public required byte BodyColor1 { get; init; }
    public required byte BodyColor2 { get; init; }
    public required float Health { get; init; }
    public required byte InteriorId { get; init; }
    public required int DoorDamageStatus { get; init; }
    public required int PanelDamageStatus { get; init; }
    public required byte LightDamageStatus { get; init; }
    public required byte TireDamageStatus { get; init; }
    public required byte AddSiren { get; init; }
    public required byte[] ModSlots { get; init; }
    public required byte PaintJob { get; init; }
    public required int InteriorColor1 { get; init; }
    public required int InteriorColor2 { get; init; }
}

public sealed class PlayerAttachedObjectInfoRpc
{
    public required int ModelId { get; init; }
    public required int Bone { get; init; }
    public required Vector3 Offset { get; init; }
    public required Vector3 Rotation { get; init; }
    public required Vector3 Scale { get; init; }
    public required int Color1 { get; init; }
    public required int Color2 { get; init; }
}

public sealed class SetPlayerAttachedObjectRpc
{
    public required ushort PlayerId { get; init; }
    public required int Index { get; init; }
    public required bool Create { get; init; }
    public required PlayerAttachedObjectInfoRpc Object { get; init; }
}

public static class SampRpc
{
    public static int HandleRpcPacketOffset => SampOffsets.RpcRuntime.HandleRpcPacket;

    // ---- Incoming (server -> client) ----

    public static SetPlayerNameRpc ParseSetPlayerName(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort playerId = r.ReadUInt16();
        string name = r.ReadStringUInt8Length();
        bool success = r.ReadBool8();
        return new(playerId, name, success);
    }

    public static SetPlayerPosRpc ParseSetPlayerPos(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerPosFindZRpc ParseSetPlayerPosFindZ(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerHealthRpc ParseSetPlayerHealth(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static TogglePlayerControllableRpc ParseTogglePlayerControllable(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static PlaySoundRpc ParsePlaySound(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        int soundId = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        return new(soundId, pos);
    }

    public static SetWorldBoundsRpc ParseSetWorldBounds(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFloat(), r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
    }

    public static GivePlayerMoneyRpc ParseGivePlayerMoney(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetPlayerFacingAngleRpc ParseSetPlayerFacingAngle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static ResetPlayerMoneyRpc ParseResetPlayerMoney(IncomingRpcArgs args)
    {
        return new();
    }

    public static ResetPlayerWeaponsRpc ParseResetPlayerWeapons(IncomingRpcArgs args)
    {
        return new();
    }

    public static GivePlayerWeaponRpc ParseGivePlayerWeapon(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SetVehicleParamsExRpc ParseSetVehicleParamsEx(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort vehicleId = r.ReadUInt16();
        VehicleParamsExStatusRpc parameters = new(
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8(),
            r.ReadUInt8());
        VehicleDoorStateRpc doors = new(r.ReadUInt8(), r.ReadUInt8(), r.ReadUInt8(), r.ReadUInt8());
        VehicleDoorStateRpc windows = new(r.ReadUInt8(), r.ReadUInt8(), r.ReadUInt8(), r.ReadUInt8());
        return new(vehicleId, parameters, doors, windows);
    }

    public static CancelEditRpc ParseCancelEdit(IncomingRpcArgs args)
    {
        return new();
    }

    public static SetPlayerTimeRpc ParseSetPlayerTime(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt8());
    }

    public static ToggleClockRpc ParseToggleClock(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static WorldPlayerAddRpc ParseWorldPlayerAdd(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        byte team = r.ReadUInt8();
        int model = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        int color = r.ReadInt32();
        byte style = r.ReadUInt8();
        return new(pid, team, model, pos, rot, color, style);
    }

    public static SetShopNameRpc ParseSetShopName(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFixedString(32));
    }

    public static SetPlayerSkillLevelRpc ParseSetPlayerSkillLevel(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadUInt16());
    }

    public static SetPlayerDrunkLevelRpc ParseSetPlayerDrunkLevel(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static Create3DTextLabelRpc ParseCreate3DTextLabel(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort id = r.ReadUInt16();
        int color = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        float dist = r.ReadFloat();
        bool testLOS = r.ReadBool8();
        ushort attachedPlayer = r.ReadUInt16();
        ushort attachedVehicle = r.ReadUInt16();
        string text = r.ReadEncodedString(4096);
        return new(id, color, pos, dist, testLOS, attachedPlayer, attachedVehicle, text);
    }

    public static DisableCheckpointRpc ParseDisableCheckpoint(IncomingRpcArgs args)
    {
        return new();
    }

    public static SetRaceCheckpointRpc ParseSetRaceCheckpoint(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        byte type = r.ReadUInt8();
        Vector3 cur = ReadVector3(ref r);
        Vector3 next = ReadVector3(ref r);
        float size = r.ReadFloat();
        return new(type, cur, next, size);
    }

    public static DisableRaceCheckpointRpc ParseDisableRaceCheckpoint(IncomingRpcArgs args)
    {
        return new();
    }

    public static GameModeRestartRpc ParseGameModeRestart(IncomingRpcArgs args)
    {
        return new();
    }

    public static PlayAudioStreamRpc ParsePlayAudioStream(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        string url = r.ReadStringUInt8Length();
        Vector3 pos = ReadVector3(ref r);
        float radius = r.ReadFloat();
        bool usePos = r.ReadBool8();
        return new(url, pos, radius, usePos);
    }

    public static StopAudioStreamRpc ParseStopAudioStream(IncomingRpcArgs args)
    {
        return new();
    }

    public static RemoveBuildingForPlayerRpc ParseRemoveBuildingForPlayer(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), ReadVector3(ref r), r.ReadFloat());
    }

    public static CreateObjectRpc ParseCreateObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort objectId = r.ReadUInt16();
        int modelId = r.ReadInt32();
        Vector3 position = ReadVector3(ref r);
        Vector3 rotation = ReadVector3(ref r);
        float drawDistance = r.ReadFloat();
        bool noCameraCollision = r.ReadBool8();
        ushort attachToVehicleId = r.ReadUInt16();
        ushort attachToObjectId = r.ReadUInt16();

        Vector3? attachOffsets = null;
        Vector3? attachRotation = null;
        bool? syncRotation = null;
        if (attachToVehicleId != ushort.MaxValue || attachToObjectId != ushort.MaxValue)
        {
            attachOffsets = ReadVector3(ref r);
            attachRotation = ReadVector3(ref r);
            syncRotation = r.ReadBool8();
        }

        byte texturesCount = r.ReadUInt8();
        List<ObjectMaterialTextureRpc> materials = [];
        List<ObjectMaterialTextRpc> materialText = [];
        while (r.RemainingBits >= 8)
        {
            ObjectMaterialType materialType = (ObjectMaterialType)r.ReadUInt8();
            switch (materialType)
            {
                case ObjectMaterialType.Texture:
                    materials.Add(ReadObjectMaterialTexture(ref r));
                    break;
                case ObjectMaterialType.Text:
                    materialText.Add(ReadObjectMaterialText(ref r));
                    break;
                default:
                    r.SkipBits(r.RemainingBits);
                    break;
            }
        }

        return new CreateObjectRpc
        {
            ObjectId = objectId,
            ModelId = modelId,
            Position = position,
            Rotation = rotation,
            DrawDistance = drawDistance,
            NoCameraCollision = noCameraCollision,
            AttachToVehicleId = attachToVehicleId,
            AttachToObjectId = attachToObjectId,
            AttachOffsets = attachOffsets,
            AttachRotation = attachRotation,
            SyncRotation = syncRotation,
            TexturesCount = texturesCount,
            Materials = materials,
            MaterialText = materialText
        };
    }

    public static SetObjectPosRpc ParseSetObjectPos(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetObjectRotRpc ParseSetObjectRot(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static DestroyObjectRpc ParseDestroyObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static DeathMessageRpc ParseDeathMessage(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerMapIconRpc ParseSetPlayerMapIcon(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        byte iconId = r.ReadUInt8();
        Vector3 pos = ReadVector3(ref r);
        byte type = r.ReadUInt8();
        int color = r.ReadInt32();
        byte style = r.ReadUInt8();
        return new(iconId, pos, type, color, style);
    }

    public static RemoveVehicleComponentRpc ParseRemoveVehicleComponent(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static Destroy3DTextLabelRpc ParseDestroy3DTextLabel(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ChatBubbleRpc ParseChatBubble(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        int color = r.ReadInt32();
        float dist = r.ReadFloat();
        int dur = r.ReadInt32();
        string msg = r.ReadStringUInt8Length();
        return new(pid, color, dist, dur, msg);
    }

    public static UpdateTimeRpc ParseUpdateTime(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static ShowDialogRpc ParseShowDialog(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort dialogId = r.ReadUInt16();
        DialogStyle style = (DialogStyle)r.ReadUInt8();
        string title = r.ReadStringUInt8Length();
        string leftButton = r.ReadStringUInt8Length();
        string rightButton = r.ReadStringUInt8Length();
        string text = r.ReadEncodedString(4096);
        return new(dialogId, style, title, leftButton, rightButton, text);
    }

    public static DestroyPickupRpc ParseDestroyPickup(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static LinkVehicleToInteriorRpc ParseLinkVehicleToInterior(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerArmourRpc ParseSetPlayerArmour(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static SetPlayerArmedWeaponRpc ParseSetPlayerArmedWeapon(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetSpawnInfoRpc ParseSetSpawnInfo(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        byte team = r.ReadUInt8();
        int skin = r.ReadInt32();
        byte unused = r.ReadUInt8();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        int w1 = r.ReadInt32(); int w2 = r.ReadInt32(); int w3 = r.ReadInt32();
        int a1 = r.ReadInt32(); int a2 = r.ReadInt32(); int a3 = r.ReadInt32();
        return new(team, skin, unused, pos, rot, w1, w2, w3, a1, a2, a3);
    }

    public static SetPlayerTeamRpc ParseSetPlayerTeam(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static PutPlayerInVehicleRpc ParsePutPlayerInVehicle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static RemovePlayerFromVehicleRpc ParseRemovePlayerFromVehicle(IncomingRpcArgs args)
    {
        return new();
    }

    public static SetPlayerColorRpc ParseSetPlayerColor(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32());
    }

    public static DisplayGameTextRpc ParseDisplayGameText(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        int style = r.ReadInt32();
        int time = r.ReadInt32();
        string text = r.ReadStringUInt32Length();
        return new(style, time, text);
    }

    public static ForceClassSelectionRpc ParseForceClassSelection(IncomingRpcArgs args)
    {
        return new();
    }

    public static AttachObjectToPlayerRpc ParseAttachObjectToPlayer(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort objId = r.ReadUInt16();
        ushort pid = r.ReadUInt16();
        Vector3 offsets = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        return new(objId, pid, offsets, rot);
    }

    public static InitMenuRpc ParseInitMenu(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        byte menuId = r.ReadUInt8();
        bool twoColumns = r.ReadBool32();
        string title = r.ReadFixedString(32);
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        float firstColumnWidth = r.ReadFloat();
        float secondColumnWidth = twoColumns ? r.ReadFloat() : 0.0f;
        bool menu = r.ReadBool32();

        int[] rows = new int[12];
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i] = r.ReadInt32();
        }

        List<MenuColumnRpc> columns = [ReadMenuColumn(ref r, firstColumnWidth)];
        if (twoColumns)
        {
            columns.Add(ReadMenuColumn(ref r, secondColumnWidth));
        }

        return new InitMenuRpc
        {
            MenuId = menuId,
            Title = title,
            X = x,
            Y = y,
            TwoColumns = twoColumns,
            Columns = columns,
            Rows = rows,
            Menu = menu
        };
    }

    public static ShowMenuRpc ParseShowMenu(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static HideMenuRpc ParseHideMenu(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static CreateExplosionRpc ParseCreateExplosion(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadInt32(), r.ReadFloat());
    }

    public static ShowPlayerNameTagRpc ParseShowPlayerNameTag(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8());
    }

    public static AttachCameraToObjectRpc ParseAttachCameraToObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static InterpolateCameraRpc ParseInterpolateCamera(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        bool setPos = r.ReadBitBool();
        Vector3 from = ReadVector3(ref r);
        Vector3 dest = ReadVector3(ref r);
        int time = r.ReadInt32();
        byte mode = r.ReadUInt8();
        return new(setPos, from, dest, time, mode);
    }

    public static SetObjectMaterialRpc ParseSetObjectMaterial(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort objectId = r.ReadUInt16();
        ObjectMaterialType materialType = (ObjectMaterialType)r.ReadUInt8();
        ObjectMaterialRpc material = materialType switch
        {
            ObjectMaterialType.Texture => ReadObjectMaterialTexture(ref r),
            ObjectMaterialType.Text => ReadObjectMaterialText(ref r),
            _ => throw new InvalidOperationException($"Unsupported object material type {materialType}.")
        };

        return new SetObjectMaterialRpc
        {
            ObjectId = objectId,
            Material = material
        };
    }

    public static GangZoneStopFlashRpc ParseGangZoneStopFlash(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ApplyPlayerAnimationRpc ParseApplyPlayerAnimation(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        string lib = r.ReadStringUInt8Length();
        string name = r.ReadStringUInt8Length();
        float fd = r.ReadFloat();
        bool loop = r.ReadBitBool();
        bool lx = r.ReadBitBool();
        bool ly = r.ReadBitBool();
        bool freeze = r.ReadBitBool();
        int time = r.ReadInt32();
        return new(pid, lib, name, fd, loop, lx, ly, freeze, time);
    }

    public static ClearPlayerAnimationsRpc ParseClearPlayerAnimations(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetPlayerSpecialActionRpc ParseSetPlayerSpecialAction(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerFightingStyleRpc ParseSetPlayerFightingStyle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerVelocityRpc ParseSetPlayerVelocity(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetVehicleVelocityRpc ParseSetVehicleVelocity(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBool8(), ReadVector3(ref r));
    }

    public static SetPlayerDrunkVisualsRpc ParseSetPlayerDrunkVisuals(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static ClientMessageRpc ParseClientMessage(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        uint color = r.ReadUInt32();
        string text = r.ReadStringUInt32Length();
        return new(color, text);
    }

    public static SetWorldTimeRpc ParseSetWorldTime(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static CreatePickupRpc ParseCreatePickup(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), ReadVector3(ref r));
    }

    public static SetVehicleTiresRpc ParseSetVehicleTires(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static MoveObjectRpc ParseMoveObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort objId = r.ReadUInt16();
        Vector3 from = ReadVector3(ref r);
        Vector3 dest = ReadVector3(ref r);
        float speed = r.ReadFloat();
        Vector3 rot = ReadVector3(ref r);
        return new(objId, from, dest, speed, rot);
    }

    public static EnableStuntBonusRpc ParseEnableStuntBonus(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static TextDrawSetStringRpc ParseTextDrawSetString(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadStringUInt16Length());
    }

    public static SetCheckpointRpc ParseSetCheckpoint(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadFloat());
    }

    public static CreateGangZoneRpc ParseCreateGangZone(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort id = r.ReadUInt16();
        float sx = r.ReadFloat(); float sy = r.ReadFloat();
        float ex = r.ReadFloat(); float ey = r.ReadFloat();
        int color = r.ReadInt32();
        return new(id, sx, sy, ex, ey, color);
    }

    public static ToggleWidescreenRpc ParseToggleWidescreen(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBool8());
    }

    public static PlayCrimeReportRpc ParsePlayCrimeReport(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort suspect = r.ReadUInt16();
        int inVehicle = r.ReadInt32();
        int model = r.ReadInt32();
        int color = r.ReadInt32();
        int crime = r.ReadInt32();
        Vector3 coords = ReadVector3(ref r);
        return new(suspect, inVehicle, model, color, crime, coords);
    }

    public static GangZoneDestroyRpc ParseGangZoneDestroy(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static GangZoneFlashRpc ParseGangZoneFlash(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32());
    }

    public static StopObjectRpc ParseStopObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetNumberPlateRpc ParseSetNumberPlate(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadStringUInt8Length());
    }

    public static TogglePlayerSpectatingRpc ParseTogglePlayerSpectating(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static PlayerSpectatePlayerRpc ParsePlayerSpectatePlayer(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static PlayerSpectateVehicleRpc ParsePlayerSpectateVehicle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static ConnectionRejectedRpc ParseConnectionRejected(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerWantedLevelRpc ParseSetPlayerWantedLevel(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static ShowTextDrawRpc ParseShowTextDraw(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort textDrawId = r.ReadUInt16();
        ShowTextDrawDataRpc textDraw = new()
        {
            Flags = r.ReadUInt8(),
            LetterWidth = r.ReadFloat(),
            LetterHeight = r.ReadFloat(),
            LetterColor = r.ReadInt32(),
            LineWidth = r.ReadFloat(),
            LineHeight = r.ReadFloat(),
            BoxColor = r.ReadInt32(),
            Shadow = r.ReadUInt8(),
            Outline = r.ReadUInt8(),
            BackgroundColor = r.ReadInt32(),
            Style = r.ReadUInt8(),
            Selectable = r.ReadUInt8(),
            Position = ReadVector2(ref r),
            ModelId = r.ReadUInt16(),
            Rotation = ReadVector3(ref r),
            Zoom = r.ReadFloat(),
            Color1 = r.ReadInt16(),
            Color2 = r.ReadInt16(),
            Text = r.ReadStringUInt16Length()
        };

        return new ShowTextDrawRpc
        {
            TextDrawId = textDrawId,
            TextDraw = textDraw
        };
    }

    public static HideTextDrawRpc ParseHideTextDraw(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ServerJoinRpc ParseServerJoin(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        int color = r.ReadInt32();
        bool isNpc = r.ReadBool8();
        string nick = r.ReadStringUInt8Length();
        return new(pid, color, isNpc, nick);
    }

    public static ServerQuitRpc ParseServerQuit(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static InitGameRpc ParseInitGame(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        bool zoneNames = r.ReadBitBool();
        bool useCJWalk = r.ReadBitBool();
        bool allowWeapons = r.ReadBitBool();
        bool limitGlobalChatRadius = r.ReadBitBool();
        float globalChatRadius = r.ReadFloat();
        bool stuntBonus = r.ReadBitBool();
        float nametagDrawDistance = r.ReadFloat();
        bool disableEnterExits = r.ReadBitBool();
        bool nametagLineOfSight = r.ReadBitBool();
        bool tirePopping = r.ReadBitBool();
        int classesAvailable = r.ReadInt32();
        ushort playerId = r.ReadUInt16();
        bool showPlayerTags = r.ReadBitBool();
        int playerMarkersMode = r.ReadInt32();
        byte worldTime = r.ReadUInt8();
        byte worldWeather = r.ReadUInt8();
        float gravity = r.ReadFloat();
        bool lanMode = r.ReadBitBool();
        int deathMoneyDrop = r.ReadInt32();
        bool instagib = r.ReadBitBool();
        int normalOnfootSendrate = r.ReadInt32();
        int normalIncarSendrate = r.ReadInt32();
        int normalFiringSendrate = r.ReadInt32();
        int sendMultiplier = r.ReadInt32();
        int lagCompMode = r.ReadInt32();
        string hostName = r.ReadStringUInt8Length();
        byte[] vehicleModels = new byte[212];
        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i] = r.ReadUInt8();
        }

        bool vehicleFriendlyFire = r.ReadBool32();
        InitGameSettingsRpc settings = new()
        {
            ZoneNames = zoneNames,
            UseCJWalk = useCJWalk,
            AllowWeapons = allowWeapons,
            LimitGlobalChatRadius = limitGlobalChatRadius,
            GlobalChatRadius = globalChatRadius,
            StuntBonus = stuntBonus,
            NametagDrawDistance = nametagDrawDistance,
            DisableEnterExits = disableEnterExits,
            NametagLineOfSight = nametagLineOfSight,
            TirePopping = tirePopping,
            ClassesAvailable = classesAvailable,
            ShowPlayerTags = showPlayerTags,
            PlayerMarkersMode = playerMarkersMode,
            WorldTime = worldTime,
            WorldWeather = worldWeather,
            Gravity = gravity,
            LanMode = lanMode,
            DeathMoneyDrop = deathMoneyDrop,
            Instagib = instagib,
            NormalOnfootSendrate = normalOnfootSendrate,
            NormalIncarSendrate = normalIncarSendrate,
            NormalFiringSendrate = normalFiringSendrate,
            SendMultiplier = sendMultiplier,
            LagCompMode = lagCompMode,
            VehicleFriendlyFire = vehicleFriendlyFire
        };

        return new InitGameRpc
        {
            PlayerId = playerId,
            HostName = hostName,
            Settings = settings,
            VehicleModels = vehicleModels,
            VehicleFriendlyFire = vehicleFriendlyFire
        };
    }

    public static RemovePlayerMapIconRpc ParseRemovePlayerMapIcon(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerAmmoRpc ParseSetPlayerAmmo(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt16());
    }

    public static SetGravityRpc ParseSetGravity(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static SetVehicleHealthRpc ParseSetVehicleHealth(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static AttachTrailerToVehicleRpc ParseAttachTrailerToVehicle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static DetachTrailerFromVehicleRpc ParseDetachTrailerFromVehicle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetPlayerDrunkHandlingRpc ParseSetPlayerDrunkHandling(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetWeatherRpc ParseSetWeather(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerSkinRpc ParseSetPlayerSkin(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SetPlayerInteriorRpc ParseSetPlayerInterior(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerCameraPosRpc ParseSetPlayerCameraPos(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerCameraLookAtRpc ParseSetPlayerCameraLookAt(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadUInt8());
    }

    public static SetVehiclePosRpc ParseSetVehiclePos(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetVehicleZAngleRpc ParseSetVehicleZAngle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static SetVehicleParamsForPlayerRpc ParseSetVehicleParamsForPlayer(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8(), r.ReadBool8());
    }

    public static SetCameraBehindPlayerRpc ParseSetCameraBehindPlayer(IncomingRpcArgs args)
    {
        return new();
    }

    public static WorldPlayerRemoveRpc ParseWorldPlayerRemove(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static WorldVehicleAddRpc ParseWorldVehicleAdd(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort vehicleId = r.ReadUInt16();
        WorldVehicleInfoRpc data = new()
        {
            Type = r.ReadInt32(),
            Position = ReadVector3(ref r),
            Rotation = r.ReadFloat(),
            BodyColor1 = r.ReadUInt8(),
            BodyColor2 = r.ReadUInt8(),
            Health = r.ReadFloat(),
            InteriorId = r.ReadUInt8(),
            DoorDamageStatus = r.ReadInt32(),
            PanelDamageStatus = r.ReadInt32(),
            LightDamageStatus = r.ReadUInt8(),
            TireDamageStatus = r.ReadUInt8(),
            AddSiren = r.ReadUInt8(),
            ModSlots = ReadUInt8Array(ref r, 14),
            PaintJob = r.ReadUInt8(),
            InteriorColor1 = r.ReadInt32(),
            InteriorColor2 = r.ReadInt32()
        };

        return new WorldVehicleAddRpc
        {
            VehicleId = vehicleId,
            Data = data
        };
    }

    public static WorldVehicleRemoveRpc ParseWorldVehicleRemove(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static WorldPlayerDeathRpc ParseWorldPlayerDeath(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static DisableVehicleCollisionsRpc ParseDisableVehicleCollisions(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static SetPlayerObjectNoCameraColRpc ParseSetPlayerObjectNoCameraCol(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ToggleCameraTargetRpc ParseToggleCameraTarget(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static CreateActorRpc ParseCreateActor(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort id = r.ReadUInt16();
        int skin = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        float hp = r.ReadFloat();
        return new(id, skin, pos, rot, hp);
    }

    public static DestroyActorRpc ParseDestroyActor(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ApplyActorAnimationRpc ParseApplyActorAnimation(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        ushort actorId = r.ReadUInt16();
        string lib = r.ReadStringUInt8Length();
        string name = r.ReadStringUInt8Length();
        float fd = r.ReadFloat();
        bool loop = r.ReadBitBool();
        bool lx = r.ReadBitBool();
        bool ly = r.ReadBitBool();
        bool freeze = r.ReadBitBool();
        int time = r.ReadInt32();
        return new(actorId, lib, name, fd, loop, lx, ly, freeze, time);
    }

    public static ClearActorAnimationRpc ParseClearActorAnimation(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetActorFacingAngleRpc ParseSetActorFacingAngle(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static SetActorPosRpc ParseSetActorPos(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetActorHealthRpc ParseSetActorHealth(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    // ---- Bidirectional incoming ----

    public static ChatMessageRpc ParseChatMessage(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        string prefix = r.ReadStringUInt8Length();
        uint prefixColor = r.ReadUInt32();
        string text = r.ReadStringUInt32Length();
        return new(prefix, prefixColor, text);
    }

    public static EnterVehicleRpc ParseEnterVehicleIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadBool8());
    }

    public static ExitVehicleRpc ParseExitVehicleIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static ClickTextDrawIncomingRpc ParseClickTextDrawIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadInt32());
    }

    public static ScmEventIncomingRpc ParseScmEventIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
    }

    public static ClientCheckIncomingRpc ParseClientCheckIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadInt32(), r.ReadUInt16(), r.ReadUInt16());
    }

    public static UpdateVehicleDamageStatusRpc ParseUpdateVehicleDamageStatusIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadUInt8(), r.ReadUInt8());
    }

    public static UpdateScoresAndPingsRpc ParseUpdateScoresAndPings(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        Dictionary<ushort, ScorePingRpc> players = [];
        while (r.RemainingBits >= 80)
        {
            ushort playerId = r.ReadUInt16();
            players[playerId] = new ScorePingRpc(r.ReadInt32(), r.ReadInt32());
        }

        return new UpdateScoresAndPingsRpc
        {
            Players = players
        };
    }

    public static EditAttachedObjectIncomingRpc ParseEditAttachedObjectIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static EditObjectIncomingRpc ParseEditObjectIncoming(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadUInt16());
    }

    public static RequestClassResponseRpc ParseRequestClassResponse(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        bool canSpawn = r.ReadBool8();
        byte team = r.ReadUInt8();
        int skin = r.ReadInt32();
        byte unused = r.ReadUInt8();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        int w1 = r.ReadInt32(); int w2 = r.ReadInt32(); int w3 = r.ReadInt32();
        int a1 = r.ReadInt32(); int a2 = r.ReadInt32(); int a3 = r.ReadInt32();
        return new(canSpawn, team, skin, unused, pos, rot, w1, w2, w3, a1, a2, a3);
    }

    public static RequestSpawnResponseRpc ParseRequestSpawnResponse(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBool8());
    }

    public static DestroyWeaponPickupRpc ParseDestroyWeaponPickup(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerAttachedObjectRpc ParseSetPlayerAttachedObject(IncomingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        PlayerAttachedObjectInfoRpc attachedObject = new()
        {
            ModelId = 0,
            Bone = 0,
            Offset = Vector3.Zero,
            Rotation = Vector3.Zero,
            Scale = Vector3.Zero,
            Color1 = 0,
            Color2 = 0
        };

        ushort playerId = r.ReadUInt16();
        int index = r.ReadInt32();
        bool create = r.ReadBitBool();
        if (r.RemainingBits >= (32 * 11))
        {
            attachedObject = new PlayerAttachedObjectInfoRpc
            {
                ModelId = r.ReadInt32(),
                Bone = r.ReadInt32(),
                Offset = ReadVector3(ref r),
                Rotation = ReadVector3(ref r),
                Scale = ReadVector3(ref r),
                Color1 = r.ReadInt32(),
                Color2 = r.ReadInt32()
            };
        }

        return new SetPlayerAttachedObjectRpc
        {
            PlayerId = playerId,
            Index = index,
            Create = create,
            Object = attachedObject
        };
    }

    public static SelectObjectRpc ParseSelectObject(IncomingRpcArgs args)
    {
        return new();
    }

    public static ServerNetStatsResponseRpc ParseServerNetStatsResponse(IncomingRpcArgs args)
    {
        return new();
    }

    // ---- Outgoing (client -> server) ----

    public static ClickPlayerRpc ParseClickPlayer(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static ClientJoinRpc ParseClientJoin(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        int ver = r.ReadInt32();
        byte mod = r.ReadUInt8();
        string nick = r.ReadStringUInt8Length();
        int cr1 = r.ReadInt32();
        string authKey = r.ReadStringUInt8Length();
        string clientVer = r.ReadStringUInt8Length();
        int cr2 = r.ReadInt32();
        return new(ver, mod, nick, cr1, authKey, clientVer, cr2);
    }

    public static SendEnterVehicleRpc ParseSendEnterVehicle(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8());
    }

    public static SendCommandRpc ParseSendCommand(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadStringUInt32Length());
    }

    public static SpawnRpc ParseSpawn(OutgoingRpcArgs args)
    {
        return new();
    }

    public static DeathNotificationRpc ParseDeathNotification(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt16());
    }

    public static DialogResponseRpc ParseDialogResponse(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8(), r.ReadUInt16(), r.ReadStringUInt8Length());
    }

    public static SendClickTextDrawRpc ParseSendClickTextDraw(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ScmEventOutgoingRpc ParseScmEventOutgoing(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), r.ReadInt32());
    }

    public static SendChatRpc ParseSendChat(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadStringUInt8Length());
    }

    public static ClientCheckResponseRpc ParseClientCheckResponse(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadInt32(), r.ReadUInt8());
    }

    public static SendVehicleDamageStatusRpc ParseSendVehicleDamageStatus(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadInt32(), r.ReadUInt8(), r.ReadUInt8());
    }

    public static GiveTakeDamageRpc ParseGiveTakeDamage(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        bool take = r.ReadBitBool();
        return new(take, r.ReadUInt16(), r.ReadFloat(), r.ReadInt32(), r.ReadInt32());
    }

    public static EditAttachedObjectOutgoingRpc ParseEditAttachedObjectOutgoing(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        int response = r.ReadInt32();
        int index = r.ReadInt32();
        int model = r.ReadInt32();
        int bone = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        Vector3 scale = ReadVector3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(response, index, model, bone, pos, rot, scale, c1, c2);
    }

    public static EditObjectOutgoingRpc ParseEditObjectOutgoing(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        bool playerObj = r.ReadBitBool();
        ushort objId = r.ReadUInt16();
        int response = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        return new(playerObj, objId, response, pos, rot);
    }

    public static SendExitVehicleRpc ParseSendExitVehicle(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetInteriorIdRpc ParseSetInteriorId(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static MapMarkerRpc ParseMapMarker(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SendRequestClassRpc ParseSendRequestClass(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SendRequestSpawnRpc ParseSendRequestSpawn(OutgoingRpcArgs args)
    {
        return new();
    }

    public static PickedUpPickupRpc ParsePickedUpPickup(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static MenuSelectRpc ParseMenuSelect(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static MenuQuitRpc ParseMenuQuit(OutgoingRpcArgs args)
    {
        return new();
    }

    public static VehicleDestroyedRpc ParseVehicleDestroyed(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static NpcJoinRpc ParseNpcJoin(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadUInt8(), r.ReadStringUInt8Length(), r.ReadInt32());
    }

    public static CameraTargetUpdateRpc ParseCameraTargetUpdate(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
    }

    public static GiveActorDamageRpc ParseGiveActorDamage(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool(), r.ReadUInt16(), r.ReadFloat(), r.ReadInt32(), r.ReadInt32());
    }

    public static SelectObjectOutgoingRpc ParseSelectObjectOutgoing(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadUInt16(), r.ReadInt32(), ReadVector3(ref r));
    }

    public static UpdateScoresAndPingsOutgoingRpc ParseUpdateScoresAndPingsOutgoing(OutgoingRpcArgs args)
    {
        return new();
    }

    // ---- Scoring helpers ----

    public static int ScoreClientMessagePayload(IncomingRpcArgs args)
    {
        ClientMessageRpc payload = ParseClientMessage(args);
        return ScoreText(payload.Text);
    }

    public static int ScoreChatMessagePayload(IncomingRpcArgs args)
    {
        ChatMessageRpc payload = ParseChatMessage(args);
        return ScoreText(payload.Prefix) + ScoreText(payload.Text);
    }

    private static int ScoreText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return int.MinValue;

        int score = 0;
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c)) { score += 4; continue; }
            if (c is ' ' or '\t' or '\r' or '\n') { score += 1; continue; }
            if (c is '{' or '}' or '[' or ']' or '(' or ')' or ':' or ';' or '.' or ',' or '!' or '?' or '-' or '+' or '/' or '\\' or '@' or '#' or '_' or '"' or '\'') { score += 2; continue; }
            if (char.IsControl(c)) { score -= 40; continue; }
            score -= 6;
        }
        return score;
    }

    private static MenuColumnRpc ReadMenuColumn(ref BitStreamReader reader, float width)
    {
        string title = reader.ReadFixedString(32);
        int rowCount = reader.ReadUInt8();
        List<string> text = new(rowCount);
        for (int i = 0; i < rowCount; i++)
        {
            text.Add(reader.ReadFixedString(32));
        }

        return new MenuColumnRpc
        {
            Title = title,
            Width = width,
            Text = text
        };
    }

    private static ObjectMaterialTextureRpc ReadObjectMaterialTexture(ref BitStreamReader reader)
    {
        return new ObjectMaterialTextureRpc
        {
            Type = ObjectMaterialType.Texture,
            MaterialId = reader.ReadUInt8(),
            ModelId = reader.ReadUInt16(),
            LibraryName = reader.ReadStringUInt8Length(),
            TextureName = reader.ReadStringUInt8Length(),
            Color = reader.ReadInt32()
        };
    }

    private static ObjectMaterialTextRpc ReadObjectMaterialText(ref BitStreamReader reader)
    {
        return new ObjectMaterialTextRpc
        {
            Type = ObjectMaterialType.Text,
            MaterialId = reader.ReadUInt8(),
            MaterialSize = reader.ReadUInt8(),
            FontName = reader.ReadStringUInt8Length(),
            FontSize = reader.ReadUInt8(),
            Bold = reader.ReadUInt8(),
            FontColor = reader.ReadInt32(),
            BackgroundColor = reader.ReadInt32(),
            Align = reader.ReadUInt8(),
            Text = reader.ReadEncodedString(2048)
        };
    }

    private static byte[] ReadUInt8Array(ref BitStreamReader reader, int count)
    {
        byte[] values = new byte[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadUInt8();
        }

        return values;
    }

    private static Vector2 ReadVector2(ref BitStreamReader reader)
    {
        return new Vector2(reader.ReadFloat(), reader.ReadFloat());
    }

    private static Vector3 ReadVector3(ref BitStreamReader reader)
    {
        return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
    }
}
