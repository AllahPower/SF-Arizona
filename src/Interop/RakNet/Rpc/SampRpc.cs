using System.Collections.Generic;
using System.Numerics;

namespace SFSharp;

public static class SampRpc
{
    public static int HandleRpcPacketOffset => SampOffsets.RpcRuntime.HandleRpcPacket;

    #region incoming (server -> client)

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

    #endregion

    #region bidirectional incoming (server -> client-side semantics)

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

    #endregion

    #region outgoing (client -> server)

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

    public static ScriptCashRpc ParseScriptCash(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SrvNetStatsRequestRpc ParseSrvNetStatsRequest(OutgoingRpcArgs args)
    {
        return new();
    }

    public static WeaponPickupDestroyRpc ParseWeaponPickupDestroy(OutgoingRpcArgs args)
    {
        BitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    #endregion

    #region scoring helpers

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

    #endregion
}
