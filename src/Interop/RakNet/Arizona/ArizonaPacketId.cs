namespace SFSharp;

// Arizona RP custom sub-packet IDs carried inside raw Packet 220 (ArizonaCef).
// Transport: first byte of Packet 220 payload = this enum value (uint8).
// Remaining bytes = sub-packet payload parsed per-ID.
// Source: arizona-events lib + IDA reverse of core.asi
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

    // byte[16] unknown, encoded_string js, encoded_string any, u32 server_id
    LoadJs = 10,

    // i32 billboard_id, byte[12] pad, encoded link, encoded user_agent, byte[12] pad
    PlayMediaOnBillboard = 12,

    // u32 server_id, string32 url
    LoadHtml = 16,

    // u32 server_id, encoded_string text
    // main CEF event pipe: carries window.executeEvent('eventName', `json`)
    Display = 17,

    // u32 unknown, bool8 status, u16 unknown
    ToggleCursor = 25,

    // u16 player_id, bool unknown, u8 state
    SetPlayerUnknownState = 27,

    // u16 server_id, u32 argb, float scale, u16 a, u16 b, u8 flags
    UiColorScale = 34,

    // u8 chat_id, string8 command, i32 color, string8 chat_name
    SetChatGroup = 36,

    // u8 state
    SetLocalInVehicle = 40,

    // u8 mode
    SetNicknameMode = 42,

    // u8 mode
    SwitchChatMode = 52,

    // bool status, float distance, u8 pad
    SetVisibleDistance3DMarker = 64,

    // bool8 status
    ShowPositionInDiscord = 71,

    // bool state
    AutoDrinkBeer = 91,

    // IDA: bool night_mode — toggles day/night color scheme on materials
    SetDayNightColors = 92,

    // IDA: bool state — toggles compass/minimap element
    ToggleCompass = 93,

    // IDA: u32 value — sets animation timer or object property
    SetAnimationProperty = 97,

    // IDA: bool state — toggles map/texture color mode
    ToggleMapColors = 101,

    // IDA: bool state — unknown toggle (UI-related)
    ToggleUnknown102 = 102,

    // IDA: string32 host, u32 port, string32 password, string32 nickname, bool unknown
    // server transfer — reconnects client to another server
    ChangeServer = 103,

    // IDA: bool state — unknown toggle
    ToggleUnknown105 = 105,

    // u32 player_id, bool is_open
    SwitchChatState = 108,

    // u8 type, u16 len
    UiConfig = 110,

    // IDA: u8 state, u8 unknown — patches game memory for spectator/camera system
    SetSpectatorPatches = 112,

    // IDA: bool state — toggles unknown flag, resets state on disable
    ToggleUnknown114 = 114,

    // u16 player_id, bool unknown, string8[4] flags
    SetPlayerNametagFlags = 120,

    // u8 icon_id, byte[14] pad, u16 icon_model, vec3 position, string8 icon_name, u8 pad
    SetMapIcon = 127,

    // u16 server_id, u8 index, float value
    UiScalar = 135,

    // u16 vehicle_id, float intensity, u8 r, u8 g, u8 b
    SetVehicleColorSmoke = 139,

    // u16 vehicle_id, u8 r, u8 g, u8 b, u8 a
    VehicleColor = 142,

    // u8 tag0 (0x21), u8 tag1, u8 tag2, string names, u32[5] offsets, u16 end
    SetSkyboxImages = 144,

    // u16 vehicle_id, { u8 type, string8 text, stringUnread region }
    SetVehicleNumberPlate = 153,

    // u16 player_id, i32 index, bool create, { i32 bone, i32 model, vec3 off, vec3 rot, vec3 scale, i32 c1, i32 c2 }
    SetPlayerAttachedObject = 155,

    // string8 text
    LoadBinary = 165,

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

    // u16 vehicle_id, string8 light_name
    SetVehicleLights = 193,

    // u16 vehicle_id, byte[6] unknown
    SetVehicleStrobelights = 209,

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

    // string16 text
    SendClientJoin = 140,

    // u8 direction (0=up, 1=down)
    SendWeaponScroll = 184,
}
