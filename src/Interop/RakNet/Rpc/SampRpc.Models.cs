using System.Collections.Generic;
using System.Numerics;

namespace SFSharp.Runtime.Networking;

#region incoming (server -> client)

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
    public override string ToString() => $"CreateObjectRpc {{ ObjectId = {ObjectId}, ModelId = {ModelId}, Position = {Position}, DrawDistance = {DrawDistance} }}";
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
    public override string ToString() => $"InitMenuRpc {{ MenuId = {MenuId}, Title = {Title}, TwoColumns = {TwoColumns} }}";
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
    public override string ToString() => $"SetObjectMaterialRpc {{ ObjectId = {ObjectId}, Type = {Material.Type}, MaterialId = {Material.MaterialId} }}";
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
    public override string ToString() => $"ShowTextDrawRpc {{ TextDrawId = {TextDrawId}, Style = {TextDraw.Style}, Text = {TextDraw.Text} }}";
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
    public override string ToString() => $"InitGameRpc {{ PlayerId = {PlayerId}, HostName = {HostName} }}";
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
    public override string ToString() => $"WorldVehicleAddRpc {{ VehicleId = {VehicleId}, Type = {Data.Type}, Position = {Data.Position} }}";
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

#endregion

#region bidirectional incoming (server -> client-side semantics)

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
    public override string ToString() => $"UpdateScoresAndPingsRpc {{ Players = {Players.Count} }}";
}
public readonly record struct EditAttachedObjectIncomingRpc(int Index);
public readonly record struct EditObjectIncomingRpc(bool PlayerObject, ushort ObjectId);
public readonly record struct RequestClassResponseRpc(bool CanSpawn, byte Team, int Skin, byte Unused, Vector3 Position, float Rotation, int Weapon1, int Weapon2, int Weapon3, int Ammo1, int Ammo2, int Ammo3);
public readonly record struct RequestSpawnResponseRpc(bool Response);
public readonly record struct DestroyWeaponPickupRpc(byte Id);
public readonly record struct SelectObjectRpc();
public readonly record struct ServerNetStatsResponseRpc();

#endregion

#region outgoing (client -> server)

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
public readonly record struct ScriptCashRpc(int Amount, int IncreaseType);
public readonly record struct SrvNetStatsRequestRpc();
public readonly record struct WeaponPickupDestroyRpc(ushort Id);

#endregion

#region shared helper models

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
    public override string ToString() => $"SetPlayerAttachedObjectRpc {{ PlayerId = {PlayerId}, Index = {Index}, Create = {Create}, ModelId = {Object.ModelId} }}";
}

#endregion
