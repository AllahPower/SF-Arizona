using System.Numerics;
using System.Text;

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

// id=10 | load JS resource into CEF pipeline
// js/any use Arizona maybeEncoded strings: u16 length, i8 encoded_flag, then plain or RakNet-encoded bytes
public readonly record struct ArzLoadJs(byte[] Unknown16, string Js, string Any, uint BrowserId);

// id=12 | billboard media playback
// link/user_agent use Arizona maybeEncoded strings: u16 length, i8 encoded_flag, then plain or RakNet-encoded bytes
public readonly record struct ArzPlayMediaOnBillboard(int BillboardId, byte[] Pad12A, string Link, string UserAgent, byte[] Pad12B);

// id=16 | load remote HTML page into CEF overlay
public readonly record struct ArzLoadHtml(uint BrowserId, string Url);

// id=17 | main CEF event pipe
// raw wire format: u32 reserved, u16 length, u8 flag, then plain text, encoded text, or numeric payload
// the leading u32 exists on the wire, but the vorbisFile.dll Display handler does not use it after dispatch
public readonly record struct ArzInjectCode(string Text, string Detail)
{
    public bool HasText => !string.IsNullOrEmpty(Text);
}

// id=10 | RakCefLoader group-A packet, exact short name still unresolved
// common recovered layout: 4 x i32 rect/anchor values, 2 text payloads, optional raw tail.
// If the first two ints are zero, vorbisFile.dll derives them from screen size and the next two ints.
public readonly record struct ArzCustomUnknown10(
    uint Value0,
    uint Value1,
    uint Value2,
    uint Value3,
    string MaybeEncodedText,
    string Text,
    byte[] RawTail
);

// id=11 | RakCefLoader group-A packet, exact short name still unresolved
// Same header as id=10 plus one float and optional trailing fields.
public readonly record struct ArzCustomUnknown11(
    uint Value0,
    uint Value1,
    uint Value2,
    uint Value3,
    string MaybeEncodedText,
    string Text,
    float FloatValue,
    byte[] RawTail
);

// id=12 | RakCefLoader group-A packet, exact short name still unresolved
// Same header as id=10 plus u16/u16/float and optional trailing fields.
public readonly record struct ArzCustomUnknown12(
    uint Value0,
    uint Value1,
    uint Value2,
    uint Value3,
    string MaybeEncodedText,
    string Text,
    ushort Short0,
    ushort Short1,
    float FloatValue,
    byte[] RawTail
);

// id=13 | RakCefLoader group-A packet, exact short name still unresolved
// Same layout as id=12, but vorbisFile.dll reads two extra u32 values before the optional tail.
public readonly record struct ArzCustomUnknown13(
    uint Value0,
    uint Value1,
    uint Value2,
    uint Value3,
    string MaybeEncodedText,
    string Text,
    ushort Short0,
    ushort Short1,
    float FloatValue,
    uint Value4,
    uint Value5,
    byte[] RawTail
);

// id=14 | close browser or screen by browser id
public readonly record struct ArzCustomClose(string BrowserId);

// id=15 | move browser/window by browser id and two values
public readonly record struct ArzCustomMove(string BrowserId, uint Value0, uint Value1);

// id=16 | change browser URL
public readonly record struct ArzCustomChangeUrl(string BrowserId, string Url);

// id=17 | inject code/event payload by browser id
public readonly record struct ArzCustomInjectCode(string BrowserId, string Payload);

// id=25 | toggle mouse cursor visibility
public readonly record struct ArzToggleCursor(uint BrowserId, bool Status, ushort Unknown2);

// id=18 | send browser/UI text message with trailing value
public readonly record struct ArzCustomSendMessage(string Text, uint Value);

// id=19 | toggle browser/screen by browser id
public readonly record struct ArzCustomToggleScreen(string BrowserId);

// id=22 | toggle browser show state by browser id
public readonly record struct ArzCustomToggleShow(string BrowserId);

// id=23 | browser click event with two values and one mode byte
public readonly record struct ArzCustomBrowserClick(string BrowserId, uint Value0, uint Value1, byte Value2);

// id=24 | get browser control state by browser id
public readonly record struct ArzGetBrowserControlState(string BrowserId);

// id=25 | set browser control state by browser id
public readonly record struct ArzCustomSetBrowserControlState(string BrowserId, byte State);

// id=28 | browser resize event
public readonly record struct ArzCustomResize(string BrowserId, uint Width, uint Height);

// id=30 | add browser object
public readonly record struct ArzCustomAddObject(string BrowserId, uint Value0, uint Value1);

// id=31 | remove browser object
public readonly record struct ArzCustomRemoveObject(string BrowserId, uint Value0, uint Value1);

// id=27 | unknown player state flag
public readonly record struct ArzSetPlayerUnknownState(ushort PlayerId, bool Unknown1, byte State);

// id=34 | CEF element color/scale override
public readonly record struct ArzUiColorScale(ushort BrowserId, uint Argb, float Scale, ushort U16a, ushort U16b, byte Flags);

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

// id=86 | observed on wire as a single-bit flag packet
public readonly record struct ArzUnknown86(bool State);

// id=91 | auto-drink beer toggle (RP event)
public readonly record struct ArzAutoDrinkBeer(bool State);

// id=92 | day/night material color toggle
public readonly record struct ArzSetDayNightColors(bool NightMode);

// id=93 | compass/minimap visibility toggle
public readonly record struct ArzToggleCompass(bool State);

// id=97 | animation/game function property value
public readonly record struct ArzSetAnimationProperty(uint Value);

// id=101 | map color patch toggle
public readonly record struct ArzToggleMapColors(bool State);

// id=102 | 5-byte CALL/NOP patch toggle at one target site
public readonly record struct ArzToggleUnknown102(bool State);

// id=103 | server transfer / reconnect
// wire format: string32 host, u32 port, string32 nickname, string password, bool connect_mode
public readonly record struct ArzChangeServer(string Host, uint Port, string Nickname, string Password, bool ConnectMode);

// id=104 | Vice City themed load screen control
// if bg_type != 0 then payload continues with optional u32 timeout
public readonly record struct ArzShowLoadScreenVc(byte BgType, uint? Timeout);

// id=105 | target trace / highlight path toggle
public readonly record struct ArzToggleUnknown105(bool State);

// id=108 | player chat window open/close notification
public readonly record struct ArzSwitchChatState(uint PlayerId, bool IsOpen);

// id=110 | CEF UI config packet
public readonly record struct ArzUiConfig(byte Type, byte Len);

// id=112 | spectator/camera patch toggle
// second byte is still not fully named, but both bytes participate in spectator/camera memory patching
public readonly record struct ArzSetSpectatorPatches(byte State, byte Unknown);

// id=114 | unknown GameFunctions state flag
public readonly record struct ArzToggleUnknown114(bool State);

// id=117 | Vice City mode flag
public readonly record struct ArzSetViceCityFlag(bool State);

// id=120 | compact nametag/settings blob; vorbisFile.dll enumerates keys like
// nonametagstatus/timestamp/headmove/hudscalefix/interior/togobjlight/cmpstat/audiomsg/logurls.
// The exact per-field layout is not fully recovered yet, so keep the remaining payload raw.
public readonly record struct ArzSetPlayerNametagFlags(ushort PlayerId, byte Header, byte[] RawTail, bool? TrailingBit);

// id=127 | custom map icon (minimap marker)
public readonly record struct ArzSetMapIcon(byte IconId, byte[] Pad14, ushort IconModel, Vector3 Position, string IconName, byte Pad);

// id=135 | CEF UI scalar value (float property by index)
public readonly record struct ArzUiScalar(ushort BrowserId, byte Index, float Value);

// id=139 | vehicle exhaust smoke color + intensity
// intensity: 0.4 is default
public readonly record struct ArzSetVehicleColorSmoke(ushort VehicleId, float Intensity, byte R, byte G, byte B);

// id=142 | vehicle body color (RGBA)
public readonly record struct ArzVehicleColor(ushort VehicleId, byte R, byte G, byte B, byte A);

// id=144 | skybox image descriptor + code offsets
public readonly record struct ArzSetSkyboxImages(
    byte Tag0, byte Tag1, byte Tag2,
    string Names,
    uint Offset1, uint Offset2, uint Offset3, uint Offset4, uint Offset5,
    ushort End
);

// id=153 | vehicle license plate text + region
public readonly record struct ArzSetVehicleNumberPlate(ushort VehicleId, byte PlateType, string PlateText, string PlateRegion);

// id=155 | attached object on a player (hat, backpack, etc.)
public readonly record struct ArzSetPlayerAttachedObject(
    ushort PlayerId, int Index, bool Create,
    int Bone, int ModelId,
    Vector3 Offset, Vector3 Rotation, Vector3 Scale,
    int Color1, int Color2
);

// id=130 | test drive toggle for a vehicle
public readonly record struct ArzTestDrive(ushort VehicleId, bool State);

// id=150 | vehicle feature flag 1 (headlights rendering)
public readonly record struct ArzVehicleFeatureFlag1(ushort VehicleId, bool State);

// id=151 | vehicle feature flag 0 (nitro base flag)
public readonly record struct ArzVehicleFeatureFlag0(ushort VehicleId, bool State);

// id=152 | vehicle feature flag 2 (nitro color flag)
public readonly record struct ArzVehicleFeatureFlag2(ushort VehicleId, bool State);

// id=156 | vehicle feature reset (reset + flag6)
public readonly record struct ArzVehicleFeatureReset(ushort VehicleId, bool State);

// id=165 | load binary resource by name
public readonly record struct ArzLoadBinary(string Text);

// id=163 | selector-mode hook toggle
// enables the sub_1004C310 hook path and related GameFunctions selector checks
public readonly record struct ArzToggleUnknown163(bool State);

// id=164 | selector gate flag
// gates sub_1004B5E0 checks and suppresses one branch of the selector logic when enabled
public readonly record struct ArzToggleUnknown164(bool State);

// id=172 | current quest/task HUD text
public readonly record struct ArzSetCurrentTask(byte Unused, string Text, string Emoji);

// id=174 | toggle entire CEF interface visibility
public readonly record struct ArzToggleDrawInterface(bool Status);

// id=175 | interior teleport with position
public readonly record struct ArzSetInterior(Vector3 Position, ushort Pad, byte Interior, byte[] Remaining);

// id=176 | toggle specific CEF panel by server_id
public readonly record struct ArzUiToggle(ushort BrowserId, bool State);

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

// id=141 | 3D waypoint/ring buffer entry with timer fields kept as raw wire integers
public readonly record struct ArzCreate3DWaypoint(ushort Id, uint X, uint Y, uint Z, uint TimeOffset, uint Unknown, bool Active);

// id=147 | HUD theme/style selector used by CHudHook
public readonly record struct ArzSetHudStyle(byte Style);

// id=149 | Drone module render target create/destroy toggle
public readonly record struct ArzToggleRenderTarget(bool Create);

// id=166 | portal visibility toggle flag
public readonly record struct ArzTogglePortal(bool State);

// id=169 | create/update portal slot; type: 1=front, 2=back/alternate
public readonly record struct ArzCreatePortal(ushort Id, byte Type, Vector3 Position, Vector3 Rotation);

// id=170 | destroy portal slot by id/type
public readonly record struct ArzDestroyPortal(ushort Id, byte Type);

// id=200 | local player skin/model override
public readonly record struct ArzSetPlayerSkin(byte SkinId);

// id=0xBE | ScaleRadarMapIcons::didRecievedScaling::packet_t
// vorbisFile.dll handler 0x10255FF8 reads: u8 radarIconId, optional float scaleX=1.0, optional float scaleY=1.0
public readonly record struct ArzScaleRadarMapIcon(byte RadarIconId, float ScaleX, float ScaleY);

// id=0xBF | GangZonePoly::zone_t packet
// vorbisFile.dll helper 0x102B5AA4 proves:
//   u8 zoneId
//   u32 pointWordCount, u32[pointWordCount] packedPolygonPoints
//   u8 colorR, u8 colorG, u8 colorB, u8 colorA, u8 style
//   bit-bool enabled
// color/style names are inferred from GangZonePoly usage; packedPolygonPoints name is used because the reader only proves a word array.
public readonly record struct ArzGangZonePoly(
    byte ZoneId,
    uint[] PackedPolygonPoints,
    byte ColorR,
    byte ColorG,
    byte ColorB,
    byte ColorA,
    byte Style,
    bool Enabled
);

public abstract record ArzGpsRoutePoint;
public sealed record ArzGpsRouteWorldPoint(Vector3 Position) : ArzGpsRoutePoint;
public sealed record ArzGpsRouteLinePoint(byte LineType, ushort LineId, Vector3 Position) : ArzGpsRoutePoint;
public sealed record ArzGpsRoutePedBonePoint(bool UseEntitySpace, ushort EntityId, ushort BoneId) : ArzGpsRoutePoint;
public sealed record ArzGpsRouteVehiclePoint(ushort VehicleId, string Label, Vector3 Position) : ArzGpsRoutePoint;

// id=212 | GPS route line packet; action 0 removes slot, action 1 creates/updates it
public readonly record struct ArzSetGpsRoute(byte Action, byte Slot, byte Speed, bool Loop, int Color1, int Color2, ArzGpsRoutePoint? First, ArzGpsRoutePoint? Second)
{
    public bool IsCreate => Action == 1;
    public bool IsRemove => Action == 0;
}

// id=215 | first-person camera mode toggle
public readonly record struct ArzSetFirstPersonCamera(bool State);
// --- Packet 220 outgoing (client -> server) ---

// id=0 | key press notification
public readonly record struct ArzSendKey(byte Key, byte Unknown);

// id=1 | chat window open/close
public readonly record struct ArzSendSwitchChatState(bool IsOpen);

// id=2 | turn lights toggle
// state: 0=off, 1=left, 2=right, 3=hazard
public readonly record struct ArzSendTurnLights(byte State);

// id=17 | open CEF interface by server/menu id pair
public readonly record struct ArzSendOpenInterface(uint BrowserId, uint MenuId);

// id=18 | send text/callback to CEF server handler
public readonly record struct ArzSendText(string Text, uint BrowserId);

// id=20 | report client screen resolution
public readonly record struct ArzSendResolution(uint Width, uint Height);

// id=20 | client -> server custom state pair
public readonly record struct ArzCustomStatePair(uint Value0, uint Value1);

// id=21 | module memory query / request payload
public readonly record struct ArzModuleReadRequest(uint ModuleOffset, string ModuleName, uint Size);

// id=24 | client requests interface hide/show
public readonly record struct ArzSendToggleDrawInterface(uint BrowserId, bool Status);

// id=38 | anti-cheat hash (64 bytes fixed)
public readonly record struct ArzSendHash(byte[] Hash);

// id=51 | switch chat mode from client side
public readonly record struct ArzSendSwitchChatMode(byte Mode);

// id=113 | report float value
public readonly record struct ArzSendFloatValue(float Value);

// id=115 | send action-state toggle
public readonly record struct ArzSendToggleActionState(bool State);

// id=116 | send target position / vec3 report
public readonly record struct ArzSendTargetPosition(Vector3 Position);

// id=140 | client join payload (version/hwid string)
public readonly record struct ArzSendClientJoin(string Text);

// id=148 | drone heading report
public readonly record struct ArzSendDroneHeading(float Heading);

// id=167 | portal state/mode report
public readonly record struct ArzSendPortalToggle(byte State);

// id=184 | mouse wheel weapon scroll
// direction: 0=up, 1=down
public readonly record struct ArzSendWeaponScroll(byte Direction);

// id=195 | damage response weapon id
public readonly record struct ArzSendDamageResponseWeapon(byte WeaponId);

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
public readonly record struct ArzApplyBotAnimation(ushort BotId, string AnimLib, string AnimName, byte[] Tail);

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
// first flag is observed as a single-bit value; remaining fields are still being recovered from core.asi
public readonly record struct ArzSendBotDamage(
    bool GiveOrTake,
    ushort BotId,
    float Damage,
    byte WeaponId,
    byte BodyPart,
    ushort Unknown0,
    ushort Unknown1
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

    // Arizona maybeEncoded format seen on Packet 220 CEF packets:
    // u16 length, i8 encoded_flag, then either plain bytes or encoded/compressed bytes.
    private static string ReadMaybeEncodedString(ref BitStreamReader r)
    {
        ushort decodedLength = r.ReadUInt16();
        byte encodedFlag = r.ReadUInt8();
        int remainingBytes = (r.RemainingBits + 7) / 8;
        if (remainingBytes <= 0)
        {
            return string.Empty;
        }

        if (encodedFlag == 0)
        {
            int plainBytes = Math.Min(decodedLength, remainingBytes);
            return plainBytes > 0 ? r.ReadFixedString(plainBytes) : string.Empty;
        }

        byte[] encodedBytes = r.ReadRemainingBytes();
        int maxCharsToWrite = decodedLength + encodedFlag;
        if (BitStreamReader.RakNetBitStreamDecodeString(encodedBytes, maxCharsToWrite, out string decodedText))
        {
            return decodedText;
        }

        string encodedText = BitConverter.ToString(encodedBytes).Replace("-", string.Empty);
        return $"<encoded len={decodedLength} flag={encodedFlag} bytes={encodedText}>";
    }

    private static ArzInjectCode ReadInjectCodePayload(ref BitStreamReader r)
    {
        ushort declaredLength = r.ReadUInt16();
        byte encodedFlag = r.ReadUInt8();

        if (declaredLength == 0)
        {
            return new(string.Empty, $"len=0 flag={encodedFlag}");
        }

        if (encodedFlag == 0)
        {
            if (declaredLength == 1)
            {
                if (r.RemainingBits < 32)
                {
                    return new(string.Empty, $"len=1 flag=0 bits={r.RemainingBits} numeric-too-short");
                }

                uint numericValue = r.ReadUInt32();
                return new(numericValue.ToString(), "len=1 flag=0 numeric=u32");
            }

            int remainingBytes = (r.RemainingBits + 7) / 8;
            int plainBytes = Math.Min(declaredLength, remainingBytes);
            string text = plainBytes > 0 ? r.ReadFixedString(plainBytes) : string.Empty;
            return new(text, $"len={declaredLength} flag=0 bytes={plainBytes}");
        }

        if (declaredLength <= 1)
        {
            return new(string.Empty, $"len={declaredLength} flag={encodedFlag} empty-encoded");
        }

        byte[] encodedBytes = r.ReadRemainingBytes();
        int maxCharsToWrite = declaredLength + encodedFlag;
        if (BitStreamReader.RakNetBitStreamDecodeString(encodedBytes, maxCharsToWrite, out string decodedText))
        {
            return new(decodedText, $"len={declaredLength} flag={encodedFlag} bytes={encodedBytes.Length} decoder=RakNet.StringCompressor");
        }

        string hex = BitConverter.ToString(encodedBytes).Replace("-", string.Empty);
        if (hex.Length > 96)
        {
            hex = hex.Substring(0, 96) + "...";
        }

        return new(string.Empty, $"len={declaredLength} flag={encodedFlag} bytes={encodedBytes.Length} hex={hex}");
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

    public static ArzLoadHtml ParseLoadHtml(ref BitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        string url = r.ReadStringUInt32Length();
        return new(browserId, url);
    }

    public static ArzInjectCode ParseInjectCode(ref BitStreamReader r)
    {
        r.ReadUInt32(); // reserved u32 present on raw 220/17 wire format
        return ReadInjectCodePayload(ref r);
    }

    public static ArzToggleCursor ParseToggleCursor(ref BitStreamReader r)
    {
        uint browserId = r.ReadUInt32();
        bool status = r.ReadBitBool();
        ushort unknown2 = r.RemainingBits >= 16 ? r.ReadUInt16() : (ushort)0;
        return new(browserId, status, unknown2);
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
    public static ArzUnknown86 ParseUnknown86(ref BitStreamReader r)
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
        return new(r.ReadBool());
    }

    public static ArzSetAnimationProperty ParseSetAnimationProperty(ref BitStreamReader r)
    {
        return new(r.ReadUInt32());
    }

    public static ArzToggleMapColors ParseToggleMapColors(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzToggleUnknown102 ParseToggleUnknown102(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzChangeServer ParseChangeServer(ref BitStreamReader r)
    {
        string host = r.ReadStringUInt32Length();
        uint port = r.ReadUInt32();
        string nickname = r.ReadStringUInt32Length();
        string password = r.ReadStringUInt32Length();
        bool connectMode = r.ReadBool();
        return new(host, port, nickname, password, connectMode);
    }

    public static ArzShowLoadScreenVc ParseShowLoadScreenVc(ref BitStreamReader r)
    {
        byte bgType = r.ReadUInt8();
        uint? timeout = r.RemainingBits >= 32 ? r.ReadUInt32() : null;
        return new(bgType, timeout);
    }

    public static ArzToggleUnknown105 ParseToggleUnknown105(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSwitchChatState ParseSwitchChatState(ref BitStreamReader r)
    {
        uint pid = r.ReadUInt32();
        bool isOpen = r.ReadBitBool();
        return new(pid, isOpen);
    }

    public static ArzUiConfig ParseUiConfig(ref BitStreamReader r)
    {
        byte type = r.ReadUInt8();
        byte len = r.ReadUInt8();
        return new(type, len);
    }

    public static ArzSetSpectatorPatches ParseSetSpectatorPatches(ref BitStreamReader r)
    {
        byte state = r.ReadUInt8();
        byte unknown = r.ReadUInt8();
        return new(state, unknown);
    }

    public static ArzToggleUnknown114 ParseToggleUnknown114(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetViceCityFlag ParseSetViceCityFlag(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzSetPlayerNametagFlags ParseSetPlayerNametagFlags(ref BitStreamReader r)
    {
        ushort pid = r.ReadUInt16();
        byte header = r.ReadUInt8();
        byte[] rawTail = r.ReadBytes(r.RemainingBits / 8).ToArray();
        bool? trailingBit = r.RemainingBits > 0 ? r.ReadBitBool() : null;
        return new(pid, header, rawTail, trailingBit);
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

    public static ArzToggleUnknown163 ParseToggleUnknown163(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
    }

    public static ArzToggleUnknown164 ParseToggleUnknown164(ref BitStreamReader r)
    {
        return new(r.ReadBitBool());
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

    public static ArzCreate3DWaypoint ParseCreate3DWaypoint(ref BitStreamReader r)
    {
        ushort id = r.ReadUInt16();
        uint x = r.ReadUInt32();
        uint y = r.ReadUInt32();
        uint z = r.ReadUInt32();
        uint timeOffset = r.ReadUInt32();
        uint unknown = r.ReadUInt32();
        bool active = r.ReadBool();
        return new(id, x, y, z, timeOffset, unknown, active);
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

    public static ArzSetPlayerSkin ParseSetPlayerSkin(ref BitStreamReader r)
    {
        return new(r.ReadUInt8());
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
        bool loop = r.ReadBool();
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
                    bool useEntitySpace = r.ReadBool();
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

    public static ArzSendFloatValue ParseSendFloatValue(ref BitStreamReader r)
    {
        return new(r.ReadFloat());
    }

    public static ArzSendToggleActionState ParseSendToggleActionState(ref BitStreamReader r)
    {
        return new(r.ReadBool());
    }

    public static ArzSendTargetPosition ParseSendTargetPosition(ref BitStreamReader r)
    {
        return new(ReadVec3(ref r));
    }

    public static ArzSendClientJoin ParseSendClientJoin(ref BitStreamReader r)
    {
        return new(r.ReadStringUInt16Length());
    }

    public static ArzSendDroneHeading ParseSendDroneHeading(ref BitStreamReader r)
    {
        return new(r.ReadFloat());
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
        byte[] tail = r.ReadBytes(r.RemainingBits / 8).ToArray();
        if (r.RemainingBits > 0)
        {
            byte[] tailWithBit = new byte[tail.Length + 1];
            tail.CopyTo(tailWithBit, 0);
            tailWithBit[tail.Length] = r.ReadBitBool() ? (byte)1 : (byte)0;
            tail = tailWithBit;
        }

        return new(botId, animLib, animName, tail);
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
        bool giveOrTake = r.ReadBitBool();
        ushort botId = r.ReadUInt16();
        float damage = r.ReadFloat();
        byte weaponId = r.ReadUInt8();
        byte bodyPart = r.ReadUInt8();
        ushort unknown0 = r.ReadUInt16();
        ushort unknown1 = r.ReadUInt16();
        return new(giveOrTake, botId, damage, weaponId, bodyPart, unknown0, unknown1);
    }
    // ---- Internal Arizona runtime custom packet helpers/parsers ----

    private static string ReadCustomPacketNumericString(ref BitStreamReader reader)
    {
        uint value = reader.ReadUInt32();
        return value.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    private static string ReadCustomPacketString(ref BitStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte encodedFlag = reader.ReadUInt8();

        if (encodedFlag != 0)
        {
            byte[] encodedPayload = reader.ReadRemainingBytes();
            if (BitStreamReader.RakNetBitStreamDecodeString(encodedPayload, length + encodedFlag, out string decoded))
            {
                return decoded;
            }

            return Convert.ToHexString(encodedPayload);
        }

        if (length <= 1)
        {
            return string.Empty;
        }

        return reader.ReadFixedString(length);
    }

    private static string ReadCustomPacketMaybeEncodedString(ref BitStreamReader reader)
    {
        ushort length = reader.ReadUInt16();
        byte encodedFlag = reader.ReadUInt8();

        if (length <= 1)
        {
            if (length == 1 && encodedFlag == 0 && reader.RemainingBits >= 32)
            {
                return reader.ReadUInt32().ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        if (encodedFlag != 0)
        {
            byte[] encodedPayload = reader.ReadRemainingBytes();
            if (BitStreamReader.RakNetBitStreamDecodeString(encodedPayload, length + encodedFlag, out string decoded))
            {
                return decoded;
            }

            return Convert.ToHexString(encodedPayload);
        }

        return reader.ReadFixedString(length);
    }

    public static ArzCustomUnknown10 ParseCustomUnknown10(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        uint value2 = reader.ReadUInt32();
        uint value3 = reader.ReadUInt32();
        string maybeEncodedText = ReadCustomPacketMaybeEncodedString(ref reader);
        string text = ReadCustomPacketString(ref reader);
        byte[] rawTail = reader.ReadRemainingBytes();
        return new(value0, value1, value2, value3, maybeEncodedText, text, rawTail);
    }

    public static ArzCustomUnknown11 ParseCustomUnknown11(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        uint value2 = reader.ReadUInt32();
        uint value3 = reader.ReadUInt32();
        string maybeEncodedText = ReadCustomPacketMaybeEncodedString(ref reader);
        string text = ReadCustomPacketString(ref reader);
        float floatValue = reader.ReadFloat();
        byte[] rawTail = reader.ReadRemainingBytes();
        return new(value0, value1, value2, value3, maybeEncodedText, text, floatValue, rawTail);
    }

    public static ArzCustomUnknown12 ParseCustomUnknown12(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        uint value2 = reader.ReadUInt32();
        uint value3 = reader.ReadUInt32();
        string maybeEncodedText = ReadCustomPacketMaybeEncodedString(ref reader);
        string text = ReadCustomPacketString(ref reader);
        ushort short0 = reader.ReadUInt16();
        ushort short1 = reader.ReadUInt16();
        float floatValue = reader.ReadFloat();
        byte[] rawTail = reader.ReadRemainingBytes();
        return new(value0, value1, value2, value3, maybeEncodedText, text, short0, short1, floatValue, rawTail);
    }

    public static ArzCustomUnknown13 ParseCustomUnknown13(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        uint value2 = reader.ReadUInt32();
        uint value3 = reader.ReadUInt32();
        string maybeEncodedText = ReadCustomPacketMaybeEncodedString(ref reader);
        string text = ReadCustomPacketString(ref reader);
        ushort short0 = reader.ReadUInt16();
        ushort short1 = reader.ReadUInt16();
        float floatValue = reader.ReadFloat();
        uint value4 = reader.ReadUInt32();
        uint value5 = reader.ReadUInt32();
        byte[] rawTail = reader.ReadRemainingBytes();
        return new(value0, value1, value2, value3, maybeEncodedText, text, short0, short1, floatValue, value4, value5, rawTail);
    }

    public static ArzCustomClose ParseCustomClose(ref BitStreamReader reader)
    {
        return new(ReadCustomPacketNumericString(ref reader));
    }

    public static ArzCustomMove ParseCustomMove(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzCustomChangeUrl ParseCustomChangeUrl(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        string url = ReadCustomPacketMaybeEncodedString(ref reader);
        return new(browserId, url);
    }

    public static ArzInjectCode ParseCustomInjectCode(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        string payload = ReadCustomPacketMaybeEncodedString(ref reader);
        return new(browserId, payload);
    }

    public static ArzCustomSendMessage ParseCustomSendMessage(ref BitStreamReader reader)
    {
        string text = ReadCustomPacketMaybeEncodedString(ref reader);
        uint value = reader.ReadUInt32();
        return new(text, value);
    }

    public static ArzCustomToggleScreen ParseCustomToggleScreen(ref BitStreamReader reader)
    {
        return new(ReadCustomPacketNumericString(ref reader));
    }

    public static ArzCustomStatePair ParseCustomStatePair(ref BitStreamReader reader)
    {
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(value0, value1);
    }

    public static ArzModuleReadRequest ParseCustomModuleReadRequest(ref BitStreamReader reader)
    {
        uint moduleOffset = reader.ReadUInt32();
        byte moduleNameLength = reader.ReadUInt8();
        string moduleName = moduleNameLength > 0 ? reader.ReadFixedString(moduleNameLength) : string.Empty;
        uint size = reader.ReadUInt32();
        return new(moduleOffset, moduleName, size);
    }

    public static ArzCustomToggleShow ParseCustomToggleShow(ref BitStreamReader reader)
    {
        return new(ReadCustomPacketNumericString(ref reader));
    }

    public static ArzCustomBrowserClick ParseCustomBrowserClick(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        byte value2 = reader.ReadUInt8();
        return new(browserId, value0, value1, value2);
    }

    public static ArzGetBrowserControlState ParseCustomGetBrowserControlState(ref BitStreamReader reader)
    {
        return new(ReadCustomPacketNumericString(ref reader));
    }

    public static ArzCustomSetBrowserControlState ParseCustomSetBrowserControlState(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        byte state = reader.ReadUInt8();
        return new(browserId, state);
    }

    public static ArzCustomResize ParseCustomResize(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        uint width = reader.ReadUInt32();
        uint height = reader.ReadUInt32();
        return new(browserId, width, height);
    }

    public static ArzCustomAddObject ParseCustomAddObject(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzCustomRemoveObject ParseCustomRemoveObject(ref BitStreamReader reader)
    {
        string browserId = ReadCustomPacketNumericString(ref reader);
        uint value0 = reader.ReadUInt32();
        uint value1 = reader.ReadUInt32();
        return new(browserId, value0, value1);
    }

    public static ArzScaleRadarMapIcon ParseCustomBlipIconRaw(ref BitStreamReader reader)
    {
        byte radarIconId = reader.ReadUInt8();
        float scaleX = reader.RemainingBits >= 32 ? reader.ReadFloat() : 1.0f;
        float scaleY = reader.RemainingBits >= 32 ? reader.ReadFloat() : 1.0f;
        return new(radarIconId, scaleX, scaleY);
    }

    public static ArzGangZonePoly ParseCustomMarkerIconBatchRaw(ref BitStreamReader reader)
    {
        byte zoneId = reader.ReadUInt8();
        uint pointWordCount = reader.ReadUInt32();
        uint[] packedPolygonPoints = new uint[pointWordCount];
        for (int i = 0; i < packedPolygonPoints.Length; i++)
        {
            packedPolygonPoints[i] = reader.ReadUInt32();
        }

        byte colorR = reader.ReadUInt8();
        byte colorG = reader.ReadUInt8();
        byte colorB = reader.ReadUInt8();
        byte colorA = reader.ReadUInt8();
        byte style = reader.ReadUInt8();
        bool enabled = reader.ReadBitBool();
        return new(zoneId, packedPolygonPoints, colorR, colorG, colorB, colorA, style, enabled);
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