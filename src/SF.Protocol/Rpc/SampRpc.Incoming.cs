using System.Numerics;

namespace SFSharp.Protocol.Rpc;

public static partial class SampRpc
{
    public static SetPlayerNameRpc ParseSetPlayerName(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort playerId = r.ReadUInt16();
        string name = r.ReadStringUInt8Length();
        bool success = r.ReadBool8();
        return new(playerId, name, success);
    }

    public static SetPlayerPosRpc ParseSetPlayerPos(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerPosFindZRpc ParseSetPlayerPosFindZ(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerHealthRpc ParseSetPlayerHealth(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static TogglePlayerControllableRpc ParseTogglePlayerControllable(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static PlaySoundRpc ParsePlaySound(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        int soundId = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        return new(soundId, pos);
    }

    public static SetWorldBoundsRpc ParseSetWorldBounds(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadFloat(), r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
    }

    public static GivePlayerMoneyRpc ParseGivePlayerMoney(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetPlayerFacingAngleRpc ParseSetPlayerFacingAngle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SetVehicleParamsExRpc ParseSetVehicleParamsEx(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt8());
    }

    public static ToggleClockRpc ParseToggleClock(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static WorldPlayerAddRpc ParseWorldPlayerAdd(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadFixedString(32));
    }

    public static SetPlayerSkillLevelRpc ParseSetPlayerSkillLevel(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32(), r.ReadUInt16());
    }

    public static SetPlayerDrunkLevelRpc ParseSetPlayerDrunkLevel(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static Create3DTextLabelRpc ParseCreate3DTextLabel(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), ReadVector3(ref r), r.ReadFloat());
    }

    public static CreateObjectRpc ParseCreateObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetObjectRotRpc ParseSetObjectRot(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static DestroyObjectRpc ParseDestroyObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static DeathMessageRpc ParseDeathMessage(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerMapIconRpc ParseSetPlayerMapIcon(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        byte iconId = r.ReadUInt8();
        Vector3 pos = ReadVector3(ref r);
        byte type = r.ReadUInt8();
        int color = r.ReadInt32();
        byte style = r.ReadUInt8();
        return new(iconId, pos, type, color, style);
    }

    public static RemoveVehicleComponentRpc ParseRemoveVehicleComponent(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static Destroy3DTextLabelRpc ParseDestroy3DTextLabel(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ChatBubbleRpc ParseChatBubble(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        int color = r.ReadInt32();
        float dist = r.ReadFloat();
        int dur = r.ReadInt32();
        string msg = r.ReadStringUInt8Length();
        return new(pid, color, dist, dur, msg);
    }

    public static UpdateTimeRpc ParseUpdateTime(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static ShowDialogRpc ParseShowDialog(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static LinkVehicleToInteriorRpc ParseLinkVehicleToInterior(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerArmourRpc ParseSetPlayerArmour(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static SetPlayerArmedWeaponRpc ParseSetPlayerArmedWeapon(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetSpawnInfoRpc ParseSetSpawnInfo(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static PutPlayerInVehicleRpc ParsePutPlayerInVehicle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static RemovePlayerFromVehicleRpc ParseRemovePlayerFromVehicle(IncomingRpcArgs args)
    {
        return new();
    }

    public static SetPlayerColorRpc ParseSetPlayerColor(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32());
    }

    public static DisplayGameTextRpc ParseDisplayGameText(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        ushort objId = r.ReadUInt16();
        ushort pid = r.ReadUInt16();
        Vector3 offsets = ReadVector3(ref r);
        Vector3 rot = ReadVector3(ref r);
        return new(objId, pid, offsets, rot);
    }

    public static InitMenuRpc ParseInitMenu(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static HideMenuRpc ParseHideMenu(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static CreateExplosionRpc ParseCreateExplosion(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadInt32(), r.ReadFloat());
    }

    public static ShowPlayerNameTagRpc ParseShowPlayerNameTag(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8());
    }

    public static AttachCameraToObjectRpc ParseAttachCameraToObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static InterpolateCameraRpc ParseInterpolateCamera(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        bool setPos = r.ReadBitBool();
        Vector3 from = ReadVector3(ref r);
        Vector3 dest = ReadVector3(ref r);
        int time = r.ReadInt32();
        byte mode = r.ReadUInt8();
        return new(setPos, from, dest, time, mode);
    }

    public static SetObjectMaterialRpc ParseSetObjectMaterial(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ApplyPlayerAnimationRpc ParseApplyPlayerAnimation(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetPlayerSpecialActionRpc ParseSetPlayerSpecialAction(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerFightingStyleRpc ParseSetPlayerFightingStyle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static SetPlayerVelocityRpc ParseSetPlayerVelocity(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetVehicleVelocityRpc ParseSetVehicleVelocity(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBool8(), ReadVector3(ref r));
    }

    public static SetPlayerDrunkVisualsRpc ParseSetPlayerDrunkVisuals(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static ClientMessageRpc ParseClientMessage(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        uint color = r.ReadUInt32();
        string text = r.ReadStringUInt32Length();
        return new(color, text);
    }

    public static SetWorldTimeRpc ParseSetWorldTime(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static CreatePickupRpc ParseCreatePickup(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32(), r.ReadInt32(), ReadVector3(ref r));
    }

    public static SetVehicleTiresRpc ParseSetVehicleTires(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static MoveObjectRpc ParseMoveObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort objId = r.ReadUInt16();
        Vector3 from = ReadVector3(ref r);
        Vector3 dest = ReadVector3(ref r);
        float speed = r.ReadFloat();
        Vector3 rot = ReadVector3(ref r);
        return new(objId, from, dest, speed, rot);
    }

    public static EnableStuntBonusRpc ParseEnableStuntBonus(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static TextDrawSetStringRpc ParseTextDrawSetString(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadStringUInt16Length());
    }

    public static SetCheckpointRpc ParseSetCheckpoint(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadFloat());
    }

    public static CreateGangZoneRpc ParseCreateGangZone(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort id = r.ReadUInt16();
        float sx = r.ReadFloat(); float sy = r.ReadFloat();
        float ex = r.ReadFloat(); float ey = r.ReadFloat();
        int color = r.ReadInt32();
        return new(id, sx, sy, ex, ey, color);
    }

    public static ToggleWidescreenRpc ParseToggleWidescreen(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBool8());
    }

    public static PlayCrimeReportRpc ParsePlayCrimeReport(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static GangZoneFlashRpc ParseGangZoneFlash(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadInt32());
    }

    public static StopObjectRpc ParseStopObject(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetNumberPlateRpc ParseSetNumberPlate(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadStringUInt8Length());
    }

    public static TogglePlayerSpectatingRpc ParseTogglePlayerSpectating(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static PlayerSpectatePlayerRpc ParsePlayerSpectatePlayer(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static PlayerSpectateVehicleRpc ParsePlayerSpectateVehicle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static ConnectionRejectedRpc ParseConnectionRejected(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerWantedLevelRpc ParseSetPlayerWantedLevel(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static ShowTextDrawRpc ParseShowTextDraw(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ServerJoinRpc ParseServerJoin(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort pid = r.ReadUInt16();
        int color = r.ReadInt32();
        bool isNpc = r.ReadBool8();
        string nick = r.ReadStringUInt8Length();
        return new(pid, color, isNpc, nick);
    }

    public static ServerQuitRpc ParseServerQuit(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt8());
    }

    public static InitGameRpc ParseInitGame(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerAmmoRpc ParseSetPlayerAmmo(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8(), r.ReadUInt16());
    }

    public static SetGravityRpc ParseSetGravity(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadFloat());
    }

    public static SetVehicleHealthRpc ParseSetVehicleHealth(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static AttachTrailerToVehicleRpc ParseAttachTrailerToVehicle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadUInt16());
    }

    public static DetachTrailerFromVehicleRpc ParseDetachTrailerFromVehicle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetPlayerDrunkHandlingRpc ParseSetPlayerDrunkHandling(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32());
    }

    public static SetWeatherRpc ParseSetWeather(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerSkinRpc ParseSetPlayerSkin(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadInt32(), r.ReadInt32());
    }

    public static SetPlayerInteriorRpc ParseSetPlayerInterior(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt8());
    }

    public static SetPlayerCameraPosRpc ParseSetPlayerCameraPos(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r));
    }

    public static SetPlayerCameraLookAtRpc ParseSetPlayerCameraLookAt(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(ReadVector3(ref r), r.ReadUInt8());
    }

    public static SetVehiclePosRpc ParseSetVehiclePos(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetVehicleZAngleRpc ParseSetVehicleZAngle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static SetVehicleParamsForPlayerRpc ParseSetVehicleParamsForPlayer(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadBool8(), r.ReadBool8());
    }

    public static SetCameraBehindPlayerRpc ParseSetCameraBehindPlayer(IncomingRpcArgs args)
    {
        return new();
    }

    public static WorldPlayerRemoveRpc ParseWorldPlayerRemove(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static WorldVehicleAddRpc ParseWorldVehicleAdd(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static WorldPlayerDeathRpc ParseWorldPlayerDeath(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static DisableVehicleCollisionsRpc ParseDisableVehicleCollisions(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static SetPlayerObjectNoCameraColRpc ParseSetPlayerObjectNoCameraCol(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ToggleCameraTargetRpc ParseToggleCameraTarget(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadBitBool());
    }

    public static CreateActorRpc ParseCreateActor(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        ushort id = r.ReadUInt16();
        int skin = r.ReadInt32();
        Vector3 pos = ReadVector3(ref r);
        float rot = r.ReadFloat();
        float hp = r.ReadFloat();
        return new(id, skin, pos, rot, hp);
    }

    public static DestroyActorRpc ParseDestroyActor(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static ApplyActorAnimationRpc ParseApplyActorAnimation(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
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
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16());
    }

    public static SetActorFacingAngleRpc ParseSetActorFacingAngle(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

    public static SetActorPosRpc ParseSetActorPos(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), ReadVector3(ref r));
    }

    public static SetActorHealthRpc ParseSetActorHealth(IncomingRpcArgs args)
    {
        SampBitStreamReader r = args.CreateReader();
        return new(r.ReadUInt16(), r.ReadFloat());
    }

}
