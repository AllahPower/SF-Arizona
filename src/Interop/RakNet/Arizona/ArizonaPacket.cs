using System.Numerics;
using System.Text;

namespace SFSharp;

public static partial class ArizonaPacket
{
    // ---- helpers ----

    private static Vector3 ReadVec3(ref BitStreamReader r)
    {
        return new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
    }

    // read remaining bytes as string (cp1251)
    private static string ReadStringRemaining(ref BitStreamReader r)
    {
        int bytes = (r.RemainingBits + 7) / 8;
        return bytes > 0 ? r.ReadFixedString(bytes) : string.Empty;
    }

    // Arizona maybeEncoded format seen on Packet 220 CEF packets:
    // u16 length, i8 encoded_flag, then either plain bytes or encoded/compressed bytes.
    private static string ReadMaybeEncodedString(ref BitStreamReader r)
    {
        ushort decodedLength = r.ReadUInt16();
        byte encodedFlag = r.ReadUInt8();

        if (encodedFlag == 0)
        {
            return decodedLength > 0 ? r.ReadFixedString(decodedLength) : string.Empty;
        }

        int maxCharsToWrite = decodedLength + encodedFlag;
        return r.ReadEncodedString(maxCharsToWrite);
    }

    // ---- Packet 220 incoming parsers ----

    public static ArzSetLocalDriver ParseSetLocalDriver(ref BitStreamReader r)
    {
        byte seatCode = r.ReadUInt8();
        bool state = r.ReadBool8();
        return new(seatCode, state);
    }

    public static ArzTurnLightUpdate ParseTurnLightUpdate(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        byte state = r.ReadUInt8();
        return new(vehicleId, state);
    }

    public static ArzSetSatiety ParseSetSatiety(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetHudMode ParseSetHudMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetRadarMode ParseSetRadarMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzLoadJs ParseLoadJs(ref BitStreamReader r)
    {
        byte[] unknown16 = r.ReadBytes(16).ToArray();
        string js = ReadMaybeEncodedString(ref r);
        string any = ReadMaybeEncodedString(ref r);
        uint browserId = r.ReadUInt32();
        return new(unknown16, js, any, browserId);
    }

    public static ArzPlayMediaOnBillboard ParsePlayMediaOnBillboard(ref BitStreamReader r)
    {
        int billboardId = r.ReadInt32();
        byte[] pad12a = r.ReadBytes(12).ToArray();
        string link = ReadMaybeEncodedString(ref r);
        string userAgent = ReadMaybeEncodedString(ref r);
        byte[] pad12b = r.ReadBytes(12).ToArray();
        return new(billboardId, pad12a, link, userAgent, pad12b);
    }

    public static ArzUiColorScale ParseUiColorScale(ref BitStreamReader r)
    {
        ushort sid = r.ReadUInt16();
        uint argb = r.ReadUInt32();
        float scale = r.ReadFloat();
        ushort a = r.ReadUInt16();
        ushort b = r.ReadUInt16();
        byte flags = r.ReadUInt8();
        return new(sid, argb, scale, a, b, flags);
    }

    public static ArzSetChatGroup ParseSetChatGroup(ref BitStreamReader r)
    {
        byte chatId = r.ReadUInt8();
        string icon = r.ReadStringUInt8Length();
        int color = r.ReadInt32();
        string chatName = r.ReadStringUInt8Length();
        byte flags = r.RemainingBits >= 8 ? r.ReadUInt8() : (byte)0;
        return new(chatId, icon, color, chatName, flags);
    }

    public static ArzHideDynamicRoom ParseHideDynamicRoom(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSrcursorSyncMode ParseSrcursorSyncMode(ref BitStreamReader r)
    {
        byte mode = r.ReadUInt8();
        float? minCursorDelta = mode == 2 && r.RemainingBits >= 32 ? r.ReadFloat() : null;
        return new(mode, minCursorDelta);
    }

    public static ArzSetChatFlag ParseSetChatFlag(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzChatMessageRelay ParseChatMessageRelay(ref BitStreamReader r)
    {
        uint colorRgba = r.ReadUInt32();
        byte chatType = r.ReadUInt8();
        byte[] rawPayload = r.ReadRemainingBytes();
        return new(colorRgba, chatType, rawPayload);
    }

    public static ArzSetLocalInVehicle ParseSetLocalInVehicle(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetNicknameMode ParseSetNicknameMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSwitchChatMode ParseSwitchChatMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetVisibleDistance3DMarker ParseSetVisibleDistance3DMarker(ref BitStreamReader r)
    {
        bool status = r.ReadBool8();
        float dist = r.ReadFloat();
        byte pad = r.ReadUInt8();
        return new(status, dist, pad);
    }

    public static ArzShowPositionInDiscord ParseShowPositionInDiscord(ref BitStreamReader r)
    {
        return new(r.ReadUInt8() != 0);
    }

    public static ArzChatCommandHelperEnabled ParseChatCommandHelperEnabled(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzUnknown74 ParseUnknown74(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }


    public static ArzDiscordSetStateText ParseDiscordSetStateText(ref BitStreamReader r)
    {
        uint byteLength = r.ReadUInt32();
        string text = byteLength > 0 ? Encoding.UTF8.GetString(r.ReadBytes((int)byteLength).ToArray()) : string.Empty;
        return new(text);
    }

    public static ArzDiscordClearStateText ParseDiscordClearStateText(ref BitStreamReader r)
    {
        return new();
    }
    public static ArzSetRadarVisibility ParseSetRadarVisibility(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzAutoDrinkBeer ParseAutoDrinkBeer(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetDayNightColors ParseSetDayNightColors(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzToggleCompass ParseToggleCompass(ref BitStreamReader r)
    {
        return new(r.ReadBool8());
    }

    public static ArzSetAnimationProperty ParseSetAnimationProperty(ref BitStreamReader r)
    {
        return new(r.ReadUInt32());
    }

    public static ArzToggleMapColors ParseToggleMapColors(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzChangeServer ParseChangeServer(ref BitStreamReader r)
    {
        string host = r.ReadStringUInt32Length();
        uint port = r.ReadUInt32();
        string nickname = r.ReadStringUInt32Length();
        string password = r.ReadStringUInt32Length();
        bool connectMode = r.ReadBool8();
        return new(host, port, nickname, password, connectMode);
    }

    public static ArzShowLoadScreenVc ParseShowLoadScreenVc(ref BitStreamReader r)
    {
        byte bgType = r.ReadUInt8();
        uint? timeout = r.RemainingBits >= 32 ? r.ReadUInt32() : null;
        return new(bgType, timeout);
    }

    public static ArzSetChatIconState ParseSetChatIconState(ref BitStreamReader r)
    {
        uint pid = r.ReadUInt32();
        bool active = r.ReadBitBool();
        return new(pid, active);
    }

    public static ArzUiConfig ParseUiConfig(ref BitStreamReader r)
    {
        byte type = r.ReadUInt8();
        byte len = r.ReadUInt8();
        return new(type, len);
    }

    public static ArzSetVehicleModelSpeedLimit ParseSetVehicleModelSpeedLimit(ref BitStreamReader r)
    {
        float speedLimitOrMinusOne = r.ReadFloat();
        uint modelCount = r.ReadUInt32();
        ushort[] vehicleModels = new ushort[modelCount];
        for (int i = 0; i < vehicleModels.Length; i++)
        {
            vehicleModels[i] = r.ReadUInt16();
        }

        return new(speedLimitOrMinusOne, vehicleModels);
    }

    public static ArzSetSpectatorPatches ParseSetSpectatorPatches(ref BitStreamReader r)
    {
        byte state = r.ReadUInt8();
        byte unknown = r.ReadUInt8();
        return new(state, unknown);
    }

    public static ArzSetViceCityFlag ParseSetViceCityFlag(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static SetPlayerNametagFlags ParseSetPlayerNametagFlags(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        byte[] rawPayload = r.RemainingBits >= 8 ? r.ReadBytes(r.RemainingBits / 8).ToArray() : [];
        bool? trailingBit = r.RemainingBits > 0 ? r.ReadBitBool() : null;
        return new(pid, rawPayload, trailingBit);
    }

    public static ArzStreamFixMode ParseStreamFixMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetMapIcon ParseSetMapIcon(ref BitStreamReader r)
    {
        byte iconId = r.ReadUInt8();
        byte[] pad14 = r.ReadBytes(14).ToArray();
        ushort iconModel = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        string iconName = r.ReadStringUInt8Length();
        byte pad = r.ReadUInt8();
        return new(iconId, pad14, iconModel, pos, iconName, pad);
    }

    public static ArzDeleteCustomMarker ParseDeleteCustomMarker(ref BitStreamReader r)
    {
        return new(r.ReadUInt32());
    }

    public static ArzClearCustomMarkers ParseClearCustomMarkers(ref BitStreamReader r)
    {
        return new();
    }

    public static ArzUiScalar ParseUiScalar(ref BitStreamReader r)
    {
        ushort sid = r.ReadUInt16();
        byte idx = r.ReadUInt8();
        float val = r.ReadFloat();
        return new(sid, idx, val);
    }

    public static ArzSetDriveOnWater ParseSetDriveOnWater(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetVehicleFlight ParseSetVehicleFlight(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzAttachVehicleToVehicleData ParseAttachVehicleToVehicleData(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        byte slot = r.ReadUInt8();
        bool hasData = r.ReadBitBool();
        ArzAttachVehicleToVehicleDataDescriptor? data = null;

        if (hasData && r.RemainingBits >= 48 * 8)
        {
            Vector3 offset = ReadVec3(ref r);
            Vector3 rotationDegrees = ReadVec3(ref r);
            byte[] componentIds = r.ReadBytes(14).ToArray();
            byte featureFlags = r.ReadUInt8();
            byte variantId = r.ReadUInt8();
            ushort modelId = r.ReadUInt16();
            byte extraByte0 = r.ReadUInt8();
            byte extraByte1 = r.ReadUInt8();
            float drawDistance = r.ReadFloat();
            data = new(offset, rotationDegrees, componentIds, featureFlags, variantId, modelId, extraByte0, extraByte1, drawDistance);
        }

        return new(vehicleId, slot, hasData, data);
    }

    public static ArzSetVehicleColorSmoke ParseSetVehicleColorSmoke(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        float intensity = r.ReadFloat();
        byte red = r.ReadUInt8();
        byte green = r.ReadUInt8();
        byte blue = r.ReadUInt8();
        return new(vid, intensity, red, green, blue);
    }

    public static ArzSetVehicleNeonColor ParseSetVehicleNeonColor(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        byte red = r.ReadUInt8();
        byte green = r.ReadUInt8();
        byte blue = r.ReadUInt8();
        byte alpha = r.ReadUInt8();
        return new(vid, red, green, blue, alpha);
    }

    public static ArzSetSkyboxImages ParseSetSkyboxImages(ref BitStreamReader r)
    {
        byte tag0 = r.ReadUInt8();
        byte tag1 = r.ReadUInt8();
        byte tag2 = r.ReadUInt8();

        int remainingBytes = (r.RemainingBits + 7) / 8;
        if (remainingBytes < 22)
        {
            return new(tag0, tag1, tag2, string.Empty, 0, 0, 0, 0, 0, 0);
        }

        int namesLength = remainingBytes - 22;
        string names = namesLength > 0 ? r.ReadFixedString(namesLength) : string.Empty;
        uint offset1 = r.ReadUInt32();
        uint offset2 = r.ReadUInt32();
        uint offset3 = r.ReadUInt32();
        uint offset4 = r.ReadUInt32();
        uint offset5 = r.ReadUInt32();
        ushort end = r.ReadUInt16();
        return new(tag0, tag1, tag2, names, offset1, offset2, offset3, offset4, offset5, end);
    }

    public static ArzSetVehicleNumberPlate ParseSetVehicleNumberPlate(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        byte plateType = r.ReadUInt8();
        string plateText = string.Empty;
        string plateRegion = string.Empty;

        if (plateType != 0)
        {
            plateText = r.ReadStringUInt8Length();
            plateRegion = r.ReadStringUInt8Length();
        }

        return new(vid, plateType, plateText, plateRegion);
    }

    public static ArzSetPlayerAttachedObject ParseSetPlayerAttachedObject(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        int index = r.ReadInt32();
        bool create = r.ReadBool8();
        int bone = r.ReadInt32();
        int modelId = r.ReadInt32();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(pid, index, create, bone, modelId, offset, rotation, scale, c1, c2);
    }

    public static ArzSetCurrentTask ParseSetCurrentTask(ref BitStreamReader r)
    {
        byte unused = r.ReadUInt8();
        string text = r.ReadStringUInt8Length();
        string emoji = ReadStringRemaining(ref r);
        return new(unused, text, emoji);
    }

    public static ArzToggleDrawInterface ParseToggleDrawInterface(ref BitStreamReader r)
    {
        return new(r.ReadUInt8() != 0);
    }

    public static ArzSetInterior ParseSetInterior(ref BitStreamReader r)
    {
        Vector3 pos = ReadVec3(ref r);
        ushort pad = r.ReadUInt16();
        byte interior = r.ReadUInt8();
        byte[] remaining = r.ReadRemainingBytes();
        return new(pos, pad, interior, remaining);
    }

    public static ArzUiToggle ParseUiToggle(ref BitStreamReader r)
    {
        bool state = r.ReadBitBool();
        ushort sid = r.ReadUInt16();
        return new(sid, state);
    }

    public static ArzVehicleHeadlightsState ParseVehicleHeadlightsState(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool(); // inferred from vehicle packet pattern; not confirmed via IDA
        return new(vid, state);
    }

    public static ArzSetVirtualWorld ParseSetVirtualWorld(ref BitStreamReader r)
    {
        return new(r.ReadUInt32());
    }

    public static ArzSetVehicleDriftMode ParseSetVehicleDriftMode(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    public static ArzSetLines ParseSetLines(ref BitStreamReader r)
    {
        byte action = r.ReadUInt8();
        ushort lineId = r.ReadUInt16();
        byte[] raw = r.ReadRemainingBytes();
        return new(action, lineId, raw);
    }

    public static ArzSetVehicleLights ParseSetVehicleLights(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        string lightName = r.ReadStringUInt8Length();
        return new(vid, lightName);
    }

    public static ArzSetVehicleStrobelights ParseSetVehicleStrobelights(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        byte step = r.ReadUInt8();
        float speed = r.ReadFloat();
        bool beam = r.ReadBitBool();
        return new(vid, step, speed, beam);
    }

    public static ArzCreate3DWaypoint ParseCreate3DWaypoint(ref BitStreamReader r)
    {
        ushort playerId = r.ReadUInt16();
        uint color = r.ReadUInt32();
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        uint timeout = r.ReadUInt32();
        uint extra = r.ReadUInt32();
        bool active = r.ReadBitBool();
        return new(playerId, color, x, y, timeout, extra, active);
    }

    public static ArzSetHudStyle ParseSetHudStyle(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzToggleRenderTarget ParseToggleRenderTarget(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzTogglePortal ParseTogglePortal(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzCreatePortal ParseCreatePortal(ref BitStreamReader r)
    {
        ushort id = r.ReadUInt16();
        byte type = r.ReadUInt8();
        Vector3 position = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        return new(id, type, position, rotation);
    }

    public static ArzDestroyPortal ParseDestroyPortal(ref BitStreamReader r)
    {
        ushort id = r.ReadUInt16();
        byte type = r.ReadUInt8();
        return new(id, type);
    }

    public static ArzShowLoadScreenVcQueue ParseShowLoadScreenVcQueue(ref BitStreamReader r)
    {
        return new();
    }

    public static ArzUnknown200 ParseUnknown200(ref BitStreamReader r)
    {
        byte mode = r.ReadUInt8();
        return new(mode);
    }

    public static ArzWallHackToggle ParseWallHackToggle(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzRadarFixPlayerStyle ParseRadarFixPlayerStyle(ref BitStreamReader r)
    {
        ushort playerIndex = r.ReadUInt16();
        byte? style = r.RemainingBits >= 8 ? r.ReadUInt8() : null;
        bool? lockFlag = r.RemainingBits > 0 ? r.ReadBitBool() : null;
        return new(playerIndex, style, lockFlag);
    }

    public static ArzSimpleAttachmentsSetMaterial ParseSimpleAttachmentsSetMaterial(ref BitStreamReader r)
    {
        ushort playerId = r.ReadUInt16();
        ushort attachIndex = r.ReadUInt16();
        byte selector = r.ReadUInt8();
        string materialName = r.ReadStringUInt8Length();
        string textureName = r.ReadStringUInt8Length();
        byte byte0 = r.ReadUInt8();
        byte byte1 = r.ReadUInt8();
        byte byte2 = r.ReadUInt8();
        byte byte3 = r.ReadUInt8();
        return new(playerId, attachIndex, selector, materialName, textureName, byte0, byte1, byte2, byte3);
    }

    public static ArzNavigationArrowTargets ParseNavigationArrowTargets(ref BitStreamReader r)
    {
        bool followVertical = r.ReadBitBool();
        bool specialMode = r.ReadBitBool();
        byte count = r.ReadUInt8();
        ArzNavigationArrowTarget[] targets = new ArzNavigationArrowTarget[count];
        for (int i = 0; i < count; i++)
        {
            targets[i] = new(r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16(), r.ReadUInt16());
        }
        return new(followVertical, specialMode, targets);
    }

    public static ArzGoogleAnalyticsMessage ParseGoogleAnalyticsMessage(ref BitStreamReader r)
    {
        byte len = r.ReadUInt8();
        string text = len > 0 ? r.ReadFixedString(len) : string.Empty;
        uint flags = r.ReadUInt32();
        return new(text, flags);
    }

    public static ArzAttachVehicleToVehicleToggle ParseAttachVehicleToVehicleToggle(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzVehicleDamageDoorPanelRules ParseVehicleDamageDoorPanelRules(ref BitStreamReader r)
    {
        ushort groupId = r.ReadUInt16();
        ushort count = r.ReadUInt16();
        ArzVehicleDamageDoorPanelRule[] entries = new ArzVehicleDamageDoorPanelRule[count];
        for (int i = 0; i < count; i++)
        {
            entries[i] = new(r.ReadUInt16(), r.ReadFloat());
        }
        return new(groupId, entries);
    }

    public static ArzDirtySampObjectsMakeObjectDirty ParseDirtySampObjectsMakeObjectDirty(ref BitStreamReader r)
    {
        bool isAttachedObject = r.ReadBitBool();
        byte dirtyLevel = r.ReadUInt8();
        ushort objectId = r.ReadUInt16();
        byte? attachIndex = isAttachedObject && r.RemainingBits >= 8 ? r.ReadUInt8() : null;
        byte? extra = r.RemainingBits >= 8 ? r.ReadUInt8() : null;
        return new(isAttachedObject, dirtyLevel, objectId, attachIndex, extra);
    }

    public static ArzTranslateObservedTextDrawPosition ParseTranslateObservedTextDrawPosition(ref BitStreamReader r)
    {
        ushort textDrawId = r.ReadUInt16();
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        return new(textDrawId, x, y);
    }

    public static ArzWaypoint3DSetPosition ParseWaypoint3DSetPosition(ref BitStreamReader r)
    {
        bool enabled = r.ReadBitBool();
        Vector3? position = enabled ? ReadVec3(ref r) : null;
        return new(enabled, position);
    }

    public static ArzSetGpsRoute ParseSetGpsRoute(ref BitStreamReader r)
    {
        byte action = r.ReadUInt8();
        byte slot = r.ReadUInt8();
        if (action == 0)
        {
            return new(action, slot, 0, false, 0, 0, null, null);
        }

        if (action != 1)
        {
            return new(action, slot, 0, false, 0, 0, null, null);
        }

        byte speed = r.ReadUInt8();
        bool loop = r.ReadBool8();
        int color1 = r.ReadInt32();
        int color2 = r.ReadInt32();
        ArzGpsRoutePoint? first = ParseGpsRoutePoint(ref r);
        ArzGpsRoutePoint? second = ParseGpsRoutePoint(ref r);
        return new(action, slot, speed, loop, color1, color2, first, second);
    }

    public static ArzSetFirstPersonCamera ParseSetFirstPersonCamera(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzTestDrive ParseTestDrive(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    public static ArzVehicleFeatureFlag1 ParseVehicleFeatureFlag1(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    public static ArzVehicleFeatureFlag0 ParseVehicleFeatureFlag0(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    public static ArzVehicleFeatureFlag2 ParseVehicleFeatureFlag2(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    public static ArzVehicleFeatureReset ParseVehicleFeatureReset(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBitBool();
        return new(vid, state);
    }

    private static ArzGpsRoutePoint? ParseGpsRoutePoint(ref BitStreamReader r)
    {
        byte type = r.ReadUInt8();
        switch (type)
        {
            case 0:
                return new ArzGpsRouteWorldPoint(ReadVec3(ref r));
            case 1:
                {
                    byte lineType = r.ReadUInt8();
                    ushort lineId = r.ReadUInt16();
                    Vector3 position = ReadVec3(ref r);
                    return new ArzGpsRouteLinePoint(lineType, lineId, position);
                }
            case 2:
                {
                    bool useEntitySpace = r.ReadBool8();
                    ushort entityId = r.ReadUInt16();
                    ushort boneId = r.ReadUInt16();
                    return new ArzGpsRoutePedBonePoint(useEntitySpace, entityId, boneId);
                }
            case 3:
                {
                    ushort vehicleId = r.ReadUInt16();
                    string label = r.ReadStringUInt8Length();
                    Vector3 position = ReadVec3(ref r);
                    return new ArzGpsRouteVehiclePoint(vehicleId, label, position);
                }
            default:
                return null;
        }
    }

    private static ArzVehicleMaterialsPacket ParseVehicleMaterialsPacket(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        List<ArzVehicleMaterialsOp> operations = [];

        while (r.RemainingBits >= 8)
        {
            byte opcode = r.ReadUInt8();
            switch (opcode)
            {
                case 0:
                    {
                        byte count = r.ReadUInt8();
                        ArzVehicleMaterialsFieldPatch[] patches = new ArzVehicleMaterialsFieldPatch[count];
                        for (int i = 0; i < count; i++)
                        {
                            byte fieldId = r.ReadUInt8();
                            byte valueKind = fieldId switch
                            {
                                33 => 1,
                                34 or 35 => 4,
                                >= 25 and <= 28 => 3,
                                30 or 31 or 32 => 3,
                                _ => 2
                            };

                            uint u32Value = 0;
                            byte u8Value = 0;
                            if (valueKind == 3)
                            {
                                u8Value = r.ReadUInt8();
                            }
                            else
                            {
                                u32Value = r.ReadUInt32();
                            }

                            patches[i] = new(fieldId, valueKind, u32Value, u8Value);
                        }

                        operations.Add(new ArzVehicleMaterialsFieldPatchOp(count, patches));
                        break;
                    }

                case 1:
                    {
                        byte count = r.ReadUInt8();
                        string[] names = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            ushort len = r.ReadUInt16();
                            names[i] = len > 0 ? r.ReadFixedString(len) : string.Empty;
                        }

                        operations.Add(new ArzVehicleMaterialsNamedToggleOp(names));
                        break;
                    }

                case 2:
                    {
                        byte count = r.ReadUInt8();
                        ArzVehicleMaterialsNamedTransform[] entries = new ArzVehicleMaterialsNamedTransform[count];
                        for (int i = 0; i < count; i++)
                        {
                            ushort len = r.ReadUInt16();
                            string name = len > 0 ? r.ReadFixedString(len) : string.Empty;
                            float x = r.ReadFloat();
                            float y = r.ReadFloat();
                            float z = r.ReadFloat();
                            uint extraValue = r.ReadUInt32();
                            entries[i] = new(name, new Vector3(x, y, z), extraValue);
                        }

                        operations.Add(new ArzVehicleMaterialsNamedTransformOp(entries));
                        break;
                    }

                case 3:
                    {
                        byte count = r.ReadUInt8();
                        string[] names = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            ushort len = r.ReadUInt16();
                            names[i] = len > 0 ? r.ReadFixedString(len) : string.Empty;
                        }

                        operations.Add(new ArzVehicleMaterialsResetByNameOp(names));
                        break;
                    }

                case 4:
                    {
                        byte count = r.ReadUInt8();
                        ArzVehicleMaterialsNamedPair[] entries = new ArzVehicleMaterialsNamedPair[count];
                        for (int i = 0; i < count; i++)
                        {
                            ushort nameLen = r.ReadUInt16();
                            string name = nameLen > 0 ? r.ReadFixedString(nameLen) : string.Empty;
                            uint packedValue = r.ReadUInt32();
                            ushort otherNameLen = r.ReadUInt16();
                            string otherName = otherNameLen > 0 ? r.ReadFixedString(otherNameLen) : string.Empty;
                            entries[i] = new(name, packedValue, otherName);
                        }

                        operations.Add(new ArzVehicleMaterialsNamedPairOp(entries));
                        break;
                    }

                default:
                    return new(vehicleId, [.. operations]);
            }
        }

        return new(vehicleId, [.. operations]);
    }
    public static ArzSetCompassMode ParseSetCompassMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetCompassCoords ParseSetCompassCoords(ref BitStreamReader r)
    {
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        return new(x, y);
    }

    public static ArzShowStunIcon ParseShowStunIcon(ref BitStreamReader r)
    {
        byte primaryCounter = r.ReadUInt8();
        byte secondaryCounter = r.ReadUInt8();
        byte tertiaryCounter = r.ReadUInt8();
        return new(primaryCounter, secondaryCounter, tertiaryCounter);
    }

    public static ArzHideStunIcon ParseHideStunIcon(ref BitStreamReader r)
    {
        return new();
    }

    public static ArzToggleCgps ParseToggleCgps(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetGreenZone ParseSetGreenZone(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSetTuningConfig ParseSetTuningConfig(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzLoadSharedTexture ParseLoadSharedTexture(ref BitStreamReader r)
    {
        byte len = r.ReadUInt8();
        byte[] data = r.ReadBytes(len).ToArray();
        return new(data);
    }

    public static ArzToggleSharedTxdFlag ParseToggleSharedTxdFlag(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetVehicleLightsColor ParseSetVehicleLightsColor(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        uint argb = r.ReadUInt32();
        return new(vehicleId, argb);
    }

    public static ArzSetWeaponUpgrade ParseSetWeaponUpgrade(ref BitStreamReader r)
    {
        byte weaponId = r.ReadUInt8();
        byte[] raw = r.ReadRemainingBytes();
        return new(weaponId, raw);
    }

    public static ArzLoadBinary ParseLoadBinary(ref BitStreamReader r)
    {
        return new(r.ReadStringUInt8Length());
    }

    public static ArzSetWaterLevel ParseSetWaterLevel(ref BitStreamReader r)
    {
        byte mode = r.ReadUInt8();
        float level = r.ReadFloat();
        if (mode == 1)
        {
            float target = r.ReadFloat();
            uint durationMs = r.ReadUInt32();
            return new(mode, level, target, durationMs);
        }
        return new(mode, level, null, null);
    }

    public static ArzUpdateWeaponSlots ParseUpdateWeaponSlots(ref BitStreamReader r)
    {
        byte weaponId = r.ReadUInt8();
        byte[] raw = r.ReadRemainingBytes();
        return new(weaponId, raw);
    }

    public static ArzSetExtendAnimGroups ParseSetExtendAnimGroups(ref BitStreamReader r)
    {
        ushort playerId = r.ReadUInt16();
        string groupName = r.ReadStringUInt8Length();
        return new(playerId, groupName);
    }

    public static ArzSetSingleAnimGroup ParseSetSingleAnimGroup(ref BitStreamReader r)
    {
        ushort playerId = r.ReadUInt16();
        ushort nameLen = r.ReadUInt16();
        string groupName = nameLen > 0 ? r.ReadFixedString(nameLen) : string.Empty;
        return new(playerId, groupName);
    }

    public static ArzResetFirstPersonState ParseResetFirstPersonState(ref BitStreamReader r)
    {
        return new();
    }

    public static ArzSetVehicleBrakeCalipersModel ParseSetVehicleBrakeCalipersModel(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        bool toggle = r.ReadBitBool();
        bool? isSimpleModel = null;
        ushort? modelId = null;

        if (toggle)
        {
            isSimpleModel = r.ReadBitBool();
            modelId = r.ReadUInt16();
        }

        return new(vehicleId, toggle, isSimpleModel, modelId);
    }

    public static ArzToggleHeadMove ParseToggleHeadMove(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetVehicleBrakeCalipers ParseSetVehicleBrakeCalipers(ref BitStreamReader r)
    {
        ushort vehicleId = r.ReadUInt16();
        byte count = r.ReadUInt8();
        ushort[] modelIds = new ushort[count];
        for (int i = 0; i < count; i++)
            modelIds[i] = r.ReadUInt16();
        uint? extra = count > 0 ? r.ReadUInt32() : null;
        return new(vehicleId, count, modelIds, extra);
    }

    // ---- Packet 220 outgoing parsers ----

    public static ArzSendKey ParseSendKey(ref BitStreamReader r)
    {
        byte key = r.ReadUInt8();
        byte unknown = r.ReadUInt8();
        return new(key, unknown);
    }

    public static ArzSendSwitchChatState ParseSendSwitchChatState(ref BitStreamReader r)
    {
        return new(r.ReadBool8());
    }

    public static ArzSendTurnLights ParseSendTurnLights(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzInjectCodeResponse ParseInjectCodeResponse(ref BitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        uint requestId = r.ReadUInt32();
        return new(browserId, requestId);
    }

    public static ArzSendText ParseSendText(ref BitStreamReader r)
    {
        string text = r.ReadStringUInt16Length();
        uint sid = r.ReadUInt32();
        return new(text, sid);
    }

    public static ArzModuleReadResponse ParseModuleReadResponse(ref BitStreamReader r)
    {
        uint moduleOffset = r.ReadUInt32();
        byte moduleNameLength = r.ReadUInt8();
        string moduleName = moduleNameLength == 0
            ? string.Empty
            : Encoding.ASCII.GetString(r.ReadBytes(moduleNameLength).ToArray());
        byte status = r.ReadUInt8();
        byte[] data = status == 0 && r.RemainingBits > 0
            ? r.ReadBytes((r.RemainingBits + 7) / 8).ToArray()
            : [];
        return new(moduleOffset, moduleName, status, data);
    }

    public static ArzBrowserControlStateReply ParseBrowserControlStateReply(ref BitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        bool state = r.ReadBitBool();
        return new(browserId, state);
    }

    public static ArzSendHWID ParseSendHWID(ref BitStreamReader r)
    {
        byte[] hexDigestBytes = r.ReadRemainingBytes();
        return new(hexDigestBytes);
    }

    public static ArzSendSwitchChatMode ParseSendSwitchChatMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendSrcursorPosition ParseSendSrcursorPosition(ref BitStreamReader r)
    {
        float x = r.ReadFloat();
        float y = r.ReadFloat();
        return new(x, y);
    }

    public static ArzInCarNanCheckReport ParseInCarNanCheckReport(ref BitStreamReader r)
    {
        byte reportKind = r.ReadUInt8();
        ushort vehicleId = r.ReadUInt16();
        return new(reportKind, vehicleId);
    }

    public static ArzSendFloatValue ParseSendFloatValue(ref BitStreamReader r)
    {
        return new(r.ReadFloat());
    }

    public static ArzSendToggleActionState ParseSendToggleActionState(ref BitStreamReader r)
    {
        return new(r.ReadBool8());
    }

    public static ArzSendTargetPosition ParseSendTargetPosition(ref BitStreamReader r)
    {
        return new(ReadVec3(ref r));
    }

    public static ArzSendCommandLine ParseSendCommandLine(ref BitStreamReader r)
    {
        return new(r.ReadStringUInt16Length());
    }

    public static ArzSendDroneHeading ParseSendDroneHeading(ref BitStreamReader r)
    {
        return new(r.ReadFloat());
    }

    public static ArzSendNavigationArrowSelection ParseSendNavigationArrowSelection(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendPortalToggle ParseSendPortalToggle(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendWeaponScroll ParseSendWeaponScroll(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendDamageResponseWeapon ParseSendDamageResponseWeapon(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }


}

