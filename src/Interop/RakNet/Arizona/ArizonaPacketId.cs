namespace SFSharp;

// Arizona RP custom sub-packet IDs carried inside raw Packet 220 (ArizonaCef).
// Transport: first byte of Packet 220 payload = this enum value (uint8).
// Remaining bytes = sub-packet payload parsed per-ID.
// Source map: arizona-events init.lua + IDA reverse of core.asi and vorbisFile.dll.
// Some ids below are intentionally listed as source-only/unimplemented so the known packet map stays complete
// even when we do not yet have a proven parser.
public enum ArizonaPacketId : byte
{
    // -- incoming (server -> client) --

    // u8 seat_code (always 0x02), bool state
    SetLocalDriver = 0,

    // u16 vehicle_id, u8 state (1=left, 2=right, 3=hazard, 0=off)
    TurnLightUpdate = 2,

    // u8 satiety (0-100)
    SetSatiety = 3,

    // u8 mode
    SetHudMode = 8,

    // u8 mode
    SetRadarMode = 9,

    // byte[16] unknown, maybeEncoded js, maybeEncoded any, u32 server_id
    LoadJs = 10,

    // arizona-events/source map: same common prelude as ids 11/12/13, then unresolved tail.
    // Observed in vorbisFile.dll (RakCefLoader group A), but short semantic name is still not proven.
    Unknown10 = 10,

    // arizona-events/source map: same common prelude as id=10, then float value, optional raw tail.
    // Name not proven yet.
    Unknown11 = 11,

    // i32 billboard_id, byte[12] pad, maybeEncoded link, maybeEncoded user_agent, byte[12] pad
    PlayMediaOnBillboard = 12,

    // arizona-events/source map: same prelude as id=10, then u16 v0, u16 v1, float value, optional raw tail.
    // Name not proven yet.
    Unknown12 = 12,

    // arizona-events/source map: same as id=12, plus two trailing u32 values before the optional raw tail.
    // Name not proven yet.
    Unknown13 = 13,

    // numericString browserId
    Close = 14,

    // numericString browserId, u32 value0, u32 value1
    Move = 15,

    // u32 server_id, string32 url
    // overlaps numerically with the browser/custom ChangeUrl meaning recovered from vorbisFile.dll.
    LoadHtml = 16,

    // numericString browserId, maybeEncodedString url
    // recovered from vorbisFile.dll browser/custom subsystem; same raw id value as LoadHtml.
    ChangeUrl = 16,

    // u32 reserved, maybeEncoded text
    // arizona-events calls this Display; vorbisFile/browser layer uses the internal semantic name InjectCode.
    InjectCode = 17,

    // maybeEncodedString text, u32 value
    SendMessage = 18,

    // numericString browserId
    ToggleScreen = 19,

    // two u32 values; recovered from vorbisFile.dll send/response path.
    // Kept as a known conflicting semantic for the same raw id until exact bridge is proven.
    StatePair = 20,

    // u32 moduleOffset, u8 moduleNameLen, char[moduleNameLen], u32 size
    // Same raw id is used in both directions; keep one enum member and resolve by packet direction.
    ModuleReadRequest = 21,
    MemoryQuery = 21,

    // numericString browserId
    ToggleShow = 22,

    // numericString browserId, u32 value0, u32 value1, u8 value2
    BrowserClick = 23,

    // numericString browserId
    GetBrowserControlState = 24,

    // observed on wire as u32 unknown, status, trailing optional data.
    // arizona-events models this as bool8 + u16 unknown2, but live logs also showed a compact 33-bit variant.
    ToggleCursor = 25,

    // numericString browserId, u8 state
    // shares raw id value with ToggleCursor depending on subsystem/meaning.
    SetBrowserControlState = 25,

    // u16 player_id, u8 state
    SetPlayerUnknownState = 27,

    // numericString browserId, u32 width, u32 height
    Resize = 28,

    // numericString browserId, u32 value0, u32 value1
    AddObject = 30,

    // numericString browserId, u32 value0, u32 value1
    RemoveObject = 31,

    // u16 server_id, u32 argb, float scale, u16 a, u16 b, u8 flags
    UiColorScale = 34,

    // u8 chat_id, string8 command, i32 color, string8 chat_name
    SetChatGroup = 36,

    // u8 state
    SetLocalInVehicle = 40,

    // u8 mode
    SetNicknameMode = 42,

    // arizona-events source-only/unimplemented: string8 fx_name, unread tail.
    // The exact semantic name is not proven yet.
    Unknown47 = 47,

    // arizona-events source-only/unimplemented: string8 fx_name, unread tail.
    // The exact semantic name is not proven yet.
    Unknown48 = 48,

    // u8 mode
    SwitchChatMode = 52,

    // bool status, float distance, u8 pad
    SetVisibleDistance3DMarker = 64,

    // bool8 status
    ShowPositionInDiscord = 71,

    // observed on wire as a single-bit flag, semantics not recovered yet.
    // Likely handled in vorbisFile.dll, but class/name is still not proven.
    Unknown86 = 86,

    // bool state - writes a patched byte through VirtualProtect in GameFunctions_HandleCustomPacket
    AutoDrinkBeer = 91,

    // IDA: bool night_mode - toggles day/night color scheme on materials
    SetDayNightColors = 92,

    // IDA: bool state - toggles compass/minimap element
    ToggleCompass = 93,

    // u32 value - stored into *(dword_101B1784 + 72) in GameFunctions_HandleCustomPacket
    SetAnimationProperty = 97,

    // bool state - patches three code/data locations in GameFunctions_HandleCustomPacket
    ToggleMapColors = 101,

    // bool state - swaps a 5-byte CALL patch with NOPs at one target site in GameFunctions_HandleCustomPacket
    ToggleUnknown102 = 102,

    // string32 host, u32 port, string32 nickname, optional string password, bool connect_mode
    // server transfer - reconnects client to another server
    ChangeServer = 103,

    // IDA: u8 bg_type, optional u32 timeout - ViceCityServer load screen control
    ShowLoadScreenVc = 104,

    // bool state - enables the target/highlight update path in GameFunctions_UpdateTargetTrace and packet 116 emission
    ToggleUnknown105 = 105,

    // observed on wire as u32 player_id, is_open.
    // arizona-events models the trailing field as IO.bool; live logs showed a compact bit-sized variant.
    SwitchChatState = 108,

    // u8 type, u16 len
    UiConfig = 110,

    // IDA: u8 state, u8 unknown - both bytes participate in spectator/camera memory patching
    SetSpectatorPatches = 112,

    // bool state - stored in byte_1028944A and disables GameFunctions_ToggleActionState when false
    ToggleUnknown114 = 114,

    // IDA: bool state - ViceCityServer flag stored in word_101B0C84 high byte
    SetViceCityFlag = 117,

    // compact nametag/settings blob; exact per-field layout not fully recovered yet.
    // arizona-events describes this as bool + 4 strings, but live packets and vorbisFile settings keys do not support that layout.
    SetPlayerNametagFlags = 120,

    // u8 icon_id, byte[14] pad, u16 icon_model, vec3 position, string8 icon_name, u8 pad
    SetMapIcon = 127,

    // u16 server_id, u8 index, float value
    UiScalar = 135,

    // u16 vehicle_id, float intensity, u8 r, u8 g, u8 b
    SetVehicleColorSmoke = 139,

    // IDA: u16 id, u32 x, u32 y, u32 z, u32 time_offset, u32 unknown, bool active
    // stores into a 3D waypoint/marker ring buffer with tick timer
    Create3DWaypoint = 141,

    // u16 vehicle_id, u8 r, u8 g, u8 b, u8 a
    VehicleColor = 142,

    // u8 tag0 (0x21), u8 tag1, u8 tag2, string names, u32[5] offsets, u16 end
    SetSkyboxImages = 144,

    // IDA: u8 style - loads HUD theme ("hud_default" or alt) + all 63 radar sprites
    SetHudStyle = 147,

    // IDA: bool create - creates (true) or destroys (false) a 512x512 render target (Drone module)
    ToggleRenderTarget = 149,

    // u16 vehicle_id, { u8 type, string8 text, stringUnread region }
    SetVehicleNumberPlate = 153,

    // u16 player_id, i32 index, bool create, { i32 bone, i32 model, vec3 off, vec3 rot, vec3 scale, i32 c1, i32 c2 }
    SetPlayerAttachedObject = 155,

    // bool state - selector gate flag stored in byte_1028946C and consumed by GameFunctions_CanUseSelectorSlot
    ToggleUnknown163 = 163,

    // bool state - selector gate flag stored in byte_1028946C and consumed by GameFunctions_CanUseSelectorSlot
    ToggleUnknown164 = 164,

    // string8 text
    LoadBinary = 165,

    // IDA: bool state - toggles portal visibility flag (Portal module)
    TogglePortal = 166,

    // IDA: u16 id (max 1004), u8 type (0=front, 1=back), vec3 offset, vec3 rotation
    // creates a portal render object at slot [id][type]
    CreatePortal = 169,

    // IDA: u16 id (max 1004), u8 type (0=front, 1=back) - destroys portal at slot [id][type]
    DestroyPortal = 170,

    // u8 unused, string8 text, stringUnread emoji
    SetCurrentTask = 172,

    // bool8 status
    ToggleDrawInterface = 174,

    // vec3 position, u16 pad, u8 interior, remaining bytes
    SetInterior = 175,

    // u16 server_id, bool state
    UiToggle = 176,

    // u16 vehicle_id, bool state
    VehicleHeadlightsState = 180,

    // u32 world
    SetVirtualWorld = 183,

    // u16 vehicle_id, bool8 state
    SetVehicleDriftMode = 187,

    // arizona-events source-only/unimplemented: unread raw payload.
    Unknown191 = 191,

    // u16 vehicle_id, string8 light_name
    SetVehicleLights = 193,

    // IDA: u8 skin_id - sets player model/skin via internal function
    SetPlayerSkin = 200,

    // u16 vehicle_id, byte[6] unknown
    SetVehicleStrobelights = 209,

    // raw payload handled by cef_loader blip icon bridge
    BlipIcon = 0xBE,

    // raw payload handled by marker/icon batch path
    MarkerIconBatch = 0xBF,

    // IDA: u8 action (0=destroy, 1=create), u8 slot, per-endpoint point/line/pedbone/vehicle payloads,
    // u8 speed, bool loop, u32 color1, u32 color2 - GPS route line drawing system
    SetGpsRoute = 212,

    // IDA: bool state - toggles first-person camera mode, patches camera control vars
    SetFirstPersonCamera = 215,

    // -- outgoing (client -> server) --

    // u8 key, u8 unknown
    SendKey = 0,

    // bool is_open
    // note: same id=0 is used for incoming SetLocalDriver, direction resolves ambiguity
    SendSwitchChatState = 1,

    // u8 state (1=left, 2=right, 3=hazard, 0=off)
    SendTurnLights = 2,

    // u32 server_id, u32 menu_id
    SendOpenInterface = 17,

    // string16 text, u32 server_id
    Send = 18,

    // u32 width, u32 height
    SendResolution = 20,

    // u32 server_id, bool status
    SendToggleDrawInterface = 24,

    // string[64] hash (fixed 64 bytes)
    SendHash = 38,

    // u8 mode
    SendSwitchChatMode = 51,

    // float value - GameFunctions_ReportFloatValue
    SendFloatValue = 113,

    // bool state - GameFunctions_ToggleActionState
    SendToggleActionState = 115,

    // vec3 position - GameFunctions target/trace report from GameFunctions_UpdateTick
    SendTargetPosition = 116,

    // string16 text - client join payload
    SendClientJoin = 140,

    // float heading - Drone_UpdateTick
    SendDroneHeading = 148,

    // u8 mode/state - Portal_ToggleSendTick
    SendPortalToggle = 167,

    // u8 direction (0=up, 1=down)
    SendWeaponScroll = 184,

    // u8 weapon_id (observed 54, 55, 56) - DamageResponseHook_SendWeaponResponse
    SendDamageResponseWeapon = 195,
}
