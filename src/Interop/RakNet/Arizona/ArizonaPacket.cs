using System.Numerics;

namespace SFSharp;

// ============================================================================
// Arizona RP custom packet payload records.
// All payloads are read from the sub-packet body AFTER the sub-id byte/word
// has already been consumed by the dispatcher.
//
// Wire format (Packet 220):
//   [u8 packet_id=220] [u8 sub_id] [... payload ...]
//
// Wire format (Packet 221):
//   [u8 packet_id=221] [u16 sub_id] [... payload ...]
//
// Naming: Arz* prefix to avoid collisions with SAMP types.
// ============================================================================

// --- Packet 220 incoming (server -> client) ---

// id=0 | driver seat assignment
// seat_code is always 0x02 in practice
public readonly record struct ArzSetLocalDriver(byte SeatCode, bool State);

// id=2 | turn signal update for a vehicle
// state: 0=off, 1=left, 2=right, 3=hazard
public readonly record struct ArzTurnLightUpdate(ushort VehicleId, byte State);

// id=3 | hunger/satiety level (0-100)
public readonly record struct ArzSetSatiety(byte Satiety);

// id=8 | HUD display mode
public readonly record struct ArzSetHudMode(byte Mode);

// id=9 | minimap/radar mode
public readonly record struct ArzSetRadarMode(byte Mode);

// id=16 | load remote HTML page into CEF overlay
public readonly record struct ArzLoadHtml(uint ServerId, string Url);

// id=17 | main CEF event pipe
// text carries JS: window.executeEvent('eventName', `{json}`);
public readonly record struct ArzDisplay(uint ServerId, string Text);

// id=25 | toggle mouse cursor visibility
public readonly record struct ArzToggleCursor(uint Unknown1, bool Status, ushort Unknown2);

// id=27 | unknown player state flag
public readonly record struct ArzSetPlayerUnknownState(ushort PlayerId, bool Unknown1, byte State);

// id=34 | CEF element color/scale override
public readonly record struct ArzUiColorScale(ushort ServerId, uint Argb, float Scale, ushort U16a, ushort U16b, byte Flags);

// id=36 | register a chat group with command and color
public readonly record struct ArzSetChatGroup(byte ChatId, string Command, int Color, string ChatName);

// id=40 | local player entered/exited vehicle
public readonly record struct ArzSetLocalInVehicle(byte State);

// id=42 | nametag display mode
public readonly record struct ArzSetNicknameMode(byte Mode);

// id=52 | chat input mode switch
public readonly record struct ArzSwitchChatMode(byte Mode);

// id=64 | 3D marker draw distance
public readonly record struct ArzSetVisibleDistance3DMarker(bool Status, float Distance, byte Pad);

// id=71 | discord rich presence location toggle
public readonly record struct ArzShowPositionInDiscord(bool Status);

// id=91 | auto-drink beer toggle (RP event)
public readonly record struct ArzAutoDrinkBeer(bool State);

// id=108 | player chat window open/close notification
public readonly record struct ArzSwitchChatState(uint PlayerId, bool IsOpen);

// id=110 | CEF UI config packet
public readonly record struct ArzUiConfig(byte Type, ushort Len);

// id=120 | nametag flag strings (clan tags, faction icons, etc.)
public readonly record struct ArzSetPlayerNametagFlags(ushort PlayerId, bool Unknown1, string Flag1, string Flag2, string Flag3, string Flag4);

// id=127 | custom map icon (minimap marker)
public readonly record struct ArzSetMapIcon(byte IconId, byte[] Pad14, ushort IconModel, Vector3 Position, string IconName, byte Pad);

// id=135 | CEF UI scalar value (float property by index)
public readonly record struct ArzUiScalar(ushort ServerId, byte Index, float Value);

// id=139 | vehicle exhaust smoke color + intensity
// intensity: 0.4 is default
public readonly record struct ArzSetVehicleColorSmoke(ushort VehicleId, float Intensity, byte R, byte G, byte B);

// id=142 | vehicle body color (RGBA)
public readonly record struct ArzVehicleColor(ushort VehicleId, byte R, byte G, byte B, byte A);

// id=153 | vehicle license plate text + region
public readonly record struct ArzSetVehicleNumberPlate(ushort VehicleId, byte PlateType, string PlateText, string PlateRegion);

// id=155 | attached object on a player (hat, backpack, etc.)
public readonly record struct ArzSetPlayerAttachedObject(
    ushort PlayerId, int Index, bool Create,
    int Bone, int ModelId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2
);

// id=165 | load binary resource by name
public readonly record struct ArzLoadBinary(string Text);

// id=172 | current quest/task HUD text
public readonly record struct ArzSetCurrentTask(byte Unused, string Text, string Emoji);

// id=174 | toggle entire CEF interface visibility
public readonly record struct ArzToggleDrawInterface(bool Status);

// id=175 | interior teleport with position
public readonly record struct ArzSetInterior(Vector3 Position, ushort Pad, byte Interior, byte[] Remaining);

// id=176 | toggle specific CEF panel by server_id
public readonly record struct ArzUiToggle(ushort ServerId, bool State);

// id=180 | vehicle headlights on/off
public readonly record struct ArzVehicleHeadlightsState(ushort VehicleId, bool State);

// id=183 | set virtual world (dimension)
public readonly record struct ArzSetVirtualWorld(uint World);

// id=187 | drift mode toggle for a vehicle
public readonly record struct ArzSetVehicleDriftMode(ushort VehicleId, bool State);

// id=193 | custom vehicle light preset by name
public readonly record struct ArzSetVehicleLights(ushort VehicleId, string LightName);

// id=209 | police/emergency strobelights
public readonly record struct ArzSetVehicleStrobelights(ushort VehicleId, byte[] Unknown6);

// --- Packet 220 outgoing (client -> server) ---

// id=0 | key press notification
public readonly record struct ArzSendKey(byte Key, byte Unknown);

// id=1 | chat window open/close
public readonly record struct ArzSendSwitchChatState(bool IsOpen);

// id=2 | turn lights toggle
// state: 0=off, 1=left, 2=right, 3=hazard
public readonly record struct ArzSendTurnLights(byte State);

// id=17 | open CEF interface by server/menu id pair
public readonly record struct ArzSendOpenInterface(uint ServerId, uint MenuId);

// id=18 | send text/callback to CEF server handler
public readonly record struct ArzSendText(string Text, uint ServerId);

// id=20 | report client screen resolution
public readonly record struct ArzSendResolution(uint Width, uint Height);

// id=24 | client requests interface hide/show
public readonly record struct ArzSendToggleDrawInterface(uint ServerId, bool Status);

// id=38 | anti-cheat hash (64 bytes fixed)
public readonly record struct ArzSendHash(byte[] Hash);

// id=51 | switch chat mode from client side
public readonly record struct ArzSendSwitchChatMode(byte Mode);

// id=140 | client join payload (version/hwid string)
public readonly record struct ArzSendClientJoin(string Text);

// id=184 | mouse wheel weapon scroll
// direction: 0=up, 1=down
public readonly record struct ArzSendWeaponScroll(byte Direction);

// --- Packet 221 incoming (server -> client) --- bot/NPC system ---

// id=50 | bot appears in streaming range
public readonly record struct ArzBotStreamIn(
    ushort BotId, short ModelId,
    Vector3 Position, float Rotation,
    float Health, float Armour,
    int NametagColor1, string NametagText1,
    int NametagColor2, string NametagText2
);

// id=51 | bot leaves streaming range
public readonly record struct ArzBotStreamOut(ushort BotId);

// id=52 | bot health/armour sync tick
public readonly record struct ArzBotOnfootSync(
    ushort BotId,
    float Health, float MaxHealth,
    float Armour, float MaxArmour
);

// id=56 | bot invulnerability flag
public readonly record struct ArzSetBotInvulnerable(ushort BotId, bool Invulnerable);

// id=57 | rename a bot
public readonly record struct ArzSetBotName(ushort BotId, string Name);

// id=65 | set bot held weapon
public readonly record struct ArzSetBotWeapon(ushort BotId, ushort WeaponId, byte Unknown);

// id=66 | teleport bot to position
public readonly record struct ArzSetBotPos(ushort BotId, Vector3 Position);

// id=67 | move bot smoothly to position
public readonly record struct ArzMoveBotToPos(ushort BotId, Vector3 Position, ushort Unknown1, uint Unknown2);

// id=69 | play animation on bot
public readonly record struct ArzApplyBotAnimation(ushort BotId, string AnimLib, string AnimName, byte[] Unknown9);

// id=80 | bot attacks a player
public readonly record struct ArzBotAttackPlayer(ushort BotId, ushort PlayerId, uint Unknown);

// id=81 | bot enters a vehicle
public readonly record struct ArzBotEnterVehicle(ushort BotId, ushort VehicleId, short Unknown1, uint Unknown2);

// id=82 | bot riding as passenger sync
public readonly record struct ArzBotPassengerSync(ushort BotId, ushort VehicleId, short SeatId, float Health, float Armour);

// id=84 | bot exits vehicle
public readonly record struct ArzBotExitVehicle(ushort BotId);

// id=85 | chat bubble above bot
public readonly record struct ArzBotChatBubble(ushort BotId, string Text, int Color, float Distance, int Duration);

// id=86 | attach object to bot
public readonly record struct ArzSetBotAttachedObject(
    ushort BotId, ushort Slot,
    int ModelId, short BoneId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2
);

// id=87 | detach object from bot
public readonly record struct ArzRemoveBotAttachedObject(ushort BotId, ushort Slot);

// id=97 | bot shoots at another bot
public readonly record struct ArzShootBotAtBot(ushort ShooterBotId, ushort TargetBotId);

// id=102 | destroy/despawn bot
public readonly record struct ArzDestroyBot(ushort BotId, bool Unknown);

// id=103 | attach simple object to bot (same layout as id=86)
public readonly record struct ArzSetBotAttachedSimpleObject(
    ushort BotId, ushort Slot,
    int ModelId, short BoneId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2
);

// --- Packet 221 outgoing (client -> server) ---

// id=53 | client-side bot position sync
public readonly record struct ArzSendBotOnfootSync(ushort BotId, Vector3 Position, float Heading);

// id=73 | bot damage report
public readonly record struct ArzSendBotDamage(
    bool GiveOrTake, ushort BotId,
    float Damage, int Weapon, int Bodypart,
    ushort Unknown, ushort PlayerId
);

// ============================================================================
// Parser methods.
// Each method takes IncomingPacketArgs where the reader is positioned right
// after the sub-id byte (for 220) or word (for 221).
// The packet-id byte (220/221) and sub-id have already been consumed.
// ============================================================================
public static class ArizonaPacket
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

    // ---- Packet 220 incoming parsers ----

    public static ArzSetLocalDriver ParseSetLocalDriver(ref BitStreamReader r)
    {
        byte seatCode = r.ReadUInt8();
        bool state = r.ReadBool();
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

    public static ArzLoadHtml ParseLoadHtml(ref BitStreamReader r)
    {
        uint serverId = r.ReadUInt32();
        string url = r.ReadStringUInt32Length();
        return new(serverId, url);
    }

    public static ArzDisplay ParseDisplay(ref BitStreamReader r)
    {
        uint serverId = r.ReadUInt32();
        // text is "maybeEncoded": u16 len, u8 encoded_flag, then string or encoded_string
        // for now read as raw string with u16 length prefix + 1 flag byte
        ushort len = r.ReadUInt16();
        byte encodedFlag = r.ReadUInt8();
        // encoded strings use RakNet compressed encoding, we read raw bytes either way
        string text = r.ReadFixedString(encodedFlag == 0 ? len : len + 1);
        return new(serverId, text);
    }

    public static ArzToggleCursor ParseToggleCursor(ref BitStreamReader r)
    {
        uint u1 = r.ReadUInt32();
        bool status = r.ReadUInt8() != 0;
        ushort u2 = r.ReadUInt16();
        return new(u1, status, u2);
    }

    public static ArzSetPlayerUnknownState ParseSetPlayerUnknownState(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        bool u1 = r.ReadBool();
        byte state = r.ReadUInt8();
        return new(pid, u1, state);
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
        string command = r.ReadStringUInt8Length();
        int color = r.ReadInt32();
        string chatName = r.ReadStringUInt8Length();
        return new(chatId, command, color, chatName);
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
        bool status = r.ReadBool();
        float dist = r.ReadFloat();
        byte pad = r.ReadUInt8();
        return new(status, dist, pad);
    }

    public static ArzShowPositionInDiscord ParseShowPositionInDiscord(ref BitStreamReader r)
    {
        return new(r.ReadUInt8() != 0);
    }

    public static ArzAutoDrinkBeer ParseAutoDrinkBeer(ref BitStreamReader r)
    {
        return new(r.ReadBool());
    }

    public static ArzSwitchChatState ParseSwitchChatState(ref BitStreamReader r)
    {
        uint pid = r.ReadUInt32();
        bool isOpen = r.ReadBool();
        return new(pid, isOpen);
    }

    public static ArzUiConfig ParseUiConfig(ref BitStreamReader r)
    {
        byte type = r.ReadUInt8();
        ushort len = r.ReadUInt16();
        return new(type, len);
    }

    public static ArzSetPlayerNametagFlags ParseSetPlayerNametagFlags(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        bool u1 = r.ReadBool();
        string f1 = r.ReadStringUInt8Length();
        string f2 = r.ReadStringUInt8Length();
        string f3 = r.ReadStringUInt8Length();
        string f4 = r.ReadStringUInt8Length();
        return new(pid, u1, f1, f2, f3, f4);
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

    public static ArzUiScalar ParseUiScalar(ref BitStreamReader r)
    {
        ushort sid = r.ReadUInt16();
        byte idx = r.ReadUInt8();
        float val = r.ReadFloat();
        return new(sid, idx, val);
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

    public static ArzVehicleColor ParseVehicleColor(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        byte red = r.ReadUInt8();
        byte green = r.ReadUInt8();
        byte blue = r.ReadUInt8();
        byte alpha = r.ReadUInt8();
        return new(vid, red, green, blue, alpha);
    }

    public static ArzSetVehicleNumberPlate ParseSetVehicleNumberPlate(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        byte plateType = r.ReadUInt8();
        string plateText = r.ReadStringUInt8Length();
        string plateRegion = ReadStringRemaining(ref r);
        return new(vid, plateType, plateText, plateRegion);
    }

    public static ArzSetPlayerAttachedObject ParseSetPlayerAttachedObject(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        int index = r.ReadInt32();
        bool create = r.ReadBool();
        int bone = r.ReadInt32();
        int modelId = r.ReadInt32();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(pid, index, create, bone, modelId, offset, rotation, scale, c1, c2);
    }

    public static ArzLoadBinary ParseLoadBinary(ref BitStreamReader r)
    {
        return new(r.ReadStringUInt8Length());
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
        ushort sid = r.ReadUInt16();
        bool state = r.ReadBool();
        return new(sid, state);
    }

    public static ArzVehicleHeadlightsState ParseVehicleHeadlightsState(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadBool();
        return new(vid, state);
    }

    public static ArzSetVirtualWorld ParseSetVirtualWorld(ref BitStreamReader r)
    {
        return new(r.ReadUInt32());
    }

    public static ArzSetVehicleDriftMode ParseSetVehicleDriftMode(ref BitStreamReader r)
    {
        ushort vid = r.ReadUInt16();
        bool state = r.ReadUInt8() != 0;
        return new(vid, state);
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
        byte[] unknown = r.ReadBytes(6).ToArray();
        return new(vid, unknown);
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
        return new(r.ReadBool());
    }

    public static ArzSendTurnLights ParseSendTurnLights(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendOpenInterface ParseSendOpenInterface(ref BitStreamReader r)
    {
        uint sid = r.ReadUInt32();
        uint mid = r.ReadUInt32();
        return new(sid, mid);
    }

    public static ArzSendText ParseSendText(ref BitStreamReader r)
    {
        string text = r.ReadStringUInt16Length();
        uint sid = r.ReadUInt32();
        return new(text, sid);
    }

    public static ArzSendResolution ParseSendResolution(ref BitStreamReader r)
    {
        uint w = r.ReadUInt32();
        uint h = r.ReadUInt32();
        return new(w, h);
    }

    public static ArzSendToggleDrawInterface ParseSendToggleDrawInterface(ref BitStreamReader r)
    {
        uint sid = r.ReadUInt32();
        bool status = r.ReadBool();
        return new(sid, status);
    }

    public static ArzSendHash ParseSendHash(ref BitStreamReader r)
    {
        byte[] hash = r.ReadBytes(64).ToArray();
        return new(hash);
    }

    public static ArzSendSwitchChatMode ParseSendSwitchChatMode(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    public static ArzSendClientJoin ParseSendClientJoin(ref BitStreamReader r)
    {
        return new(r.ReadStringUInt16Length());
    }

    public static ArzSendWeaponScroll ParseSendWeaponScroll(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
    }

    // ---- Packet 221 incoming parsers (bot system) ----

    public static ArzBotStreamIn ParseBotStreamIn(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        short modelId = (short)r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        float rot = r.ReadFloat();
        r.SkipBits(1); // bool padding
        float hp = r.ReadFloat();
        float armour = r.ReadFloat();
        r.SkipBytes(3); // unknown padding

        // nametag 1: { i32 color, string32 text }
        int ntColor1 = r.ReadInt32();
        string ntText1 = r.ReadStringUInt32Length();
        r.SkipBytes(1); // separator

        // nametag 2: { i32 color, string32 text }
        int ntColor2 = r.ReadInt32();
        string ntText2 = r.ReadStringUInt32Length();
        r.SkipBytes(7); // trailing padding

        return new(botId, modelId, pos, rot, hp, armour, ntColor1, ntText1, ntColor2, ntText2);
    }

    public static ArzBotStreamOut ParseBotStreamOut(ref BitStreamReader r)
    {
        return new(r.ReadUInt16());
    }

    public static ArzBotOnfootSync ParseBotOnfootSync(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        r.SkipBytes(4); // unknown
        r.SkipBits(1);  // bool padding
        float hp = r.ReadFloat();
        float maxHp = r.ReadFloat();
        float armour = r.ReadFloat();
        float maxArmour = r.ReadFloat();
        r.SkipBytes(1); // trailing
        return new(botId, hp, maxHp, armour, maxArmour);
    }

    public static ArzSetBotInvulnerable ParseSetBotInvulnerable(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        bool invuln = r.ReadBool();
        return new(botId, invuln);
    }

    public static ArzSetBotName ParseSetBotName(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        string name = r.ReadStringUInt32Length();
        r.SkipBytes(1); // unused trailing byte
        return new(botId, name);
    }

    public static ArzSetBotWeapon ParseSetBotWeapon(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort weaponId = r.ReadUInt16();
        byte unknown = r.ReadUInt8();
        return new(botId, weaponId, unknown);
    }

    public static ArzSetBotPos ParseSetBotPos(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        return new(botId, pos);
    }

    public static ArzMoveBotToPos ParseMoveBotToPos(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        ushort u1 = r.ReadUInt16();
        uint u2 = r.ReadUInt32();
        return new(botId, pos, u1, u2);
    }

    public static ArzApplyBotAnimation ParseApplyBotAnimation(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        string animLib = r.ReadStringUInt32Length();
        string animName = r.ReadStringUInt32Length();
        byte[] unknown = r.ReadBytes(9).ToArray();
        return new(botId, animLib, animName, unknown);
    }

    public static ArzBotAttackPlayer ParseBotAttackPlayer(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort playerId = r.ReadUInt16();
        uint unknown = r.ReadUInt32();
        return new(botId, playerId, unknown);
    }

    public static ArzBotEnterVehicle ParseBotEnterVehicle(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort vid = r.ReadUInt16();
        short u1 = (short)r.ReadUInt16();
        uint u2 = r.ReadUInt32();
        return new(botId, vid, u1, u2);
    }

    public static ArzBotPassengerSync ParseBotPassengerSync(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort vid = r.ReadUInt16();
        short seatId = (short)r.ReadUInt16();
        float hp = r.ReadFloat();
        float armour = r.ReadFloat();
        return new(botId, vid, seatId, hp, armour);
    }

    public static ArzBotExitVehicle ParseBotExitVehicle(ref BitStreamReader r)
    {
        return new(r.ReadUInt16());
    }

    public static ArzBotChatBubble ParseBotChatBubble(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        string text = r.ReadStringUInt32Length();
        int color = r.ReadInt32();
        float dist = r.ReadFloat();
        int duration = r.ReadInt32();
        return new(botId, text, color, dist, duration);
    }

    public static ArzSetBotAttachedObject ParseSetBotAttachedObject(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        int modelId = r.ReadInt32();
        short boneId = (short)r.ReadUInt16();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(botId, slot, modelId, boneId, offset, rotation, scale, c1, c2);
    }

    public static ArzRemoveBotAttachedObject ParseRemoveBotAttachedObject(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        return new(botId, slot);
    }

    public static ArzShootBotAtBot ParseShootBotAtBot(ref BitStreamReader r)
    {
        ushort shooter = r.ReadUInt16();
        ushort target = r.ReadUInt16();
        return new(shooter, target);
    }

    public static ArzDestroyBot ParseDestroyBot(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        bool unknown = r.ReadUInt8() != 0;
        return new(botId, unknown);
    }

    public static ArzSetBotAttachedSimpleObject ParseSetBotAttachedSimpleObject(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        ushort slot = r.ReadUInt16();
        int modelId = r.ReadInt32();
        short boneId = (short)r.ReadUInt16();
        Vector3 offset = ReadVec3(ref r);
        Vector3 rotation = ReadVec3(ref r);
        Vector3 scale = ReadVec3(ref r);
        int c1 = r.ReadInt32();
        int c2 = r.ReadInt32();
        return new(botId, slot, modelId, boneId, offset, rotation, scale, c1, c2);
    }

    // ---- Packet 221 outgoing parsers ----

    public static ArzSendBotOnfootSync ParseSendBotOnfootSync(ref BitStreamReader r)
    {
        ushort botId = r.ReadUInt16();
        Vector3 pos = ReadVec3(ref r);
        r.SkipBytes(4); // unknown
        r.SkipBits(1);  // bool padding
        float heading = r.ReadFloat();
        r.SkipBytes(3); // trailing
        return new(botId, pos, heading);
    }

    public static ArzSendBotDamage ParseSendBotDamage(ref BitStreamReader r)
    {
        bool giveOrTake = r.ReadBool();
        ushort botId = r.ReadUInt16();
        float damage = r.ReadFloat();
        int weapon = r.ReadInt32();
        int bodypart = r.ReadInt32();
        ushort unknown = r.ReadUInt16();
        ushort playerId = r.ReadUInt16();
        return new(giveOrTake, botId, damage, weapon, bodypart, unknown, playerId);
    }

    // ============================================================================
    // Dispatcher helpers.
    // Call these from packet hooks to extract sub-id and create a reader
    // positioned at the payload start.
    //
    // Usage from IncomingPacketArgs:
    //   var reader = args.CreateReader();
    //   reader.SkipBytes(1); // skip packet id byte (220/221)
    //   byte subId = reader.ReadUInt8(); // or ReadUInt16 for 221
    //   // now call the appropriate Parse* method with ref reader
    // ============================================================================

    // extract sub-id from a Packet 220 bitstream (after skipping the packet id byte)
    public static byte ReadSubId220(ref BitStreamReader reader)
    {
        return reader.ReadUInt8();
    }

    // extract sub-id from a Packet 221 bitstream (after skipping the packet id byte)
    public static ushort ReadSubId221(ref BitStreamReader reader)
    {
        return reader.ReadUInt16();
    }
}
