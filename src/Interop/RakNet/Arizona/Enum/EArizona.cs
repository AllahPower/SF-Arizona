namespace SFSharp.Interop.RakNet.Arizona.Enum;

// Arizona RP custom sub-packet IDs carried inside raw Packet 220 (ArizonaCef).
// Transport: first byte of Packet 220 payload = this enum value (uint8).
// Remaining bytes = sub-packet payload parsed per-ID.
// Source map: arizona-events init.lua + IDA reverse of core.asi and vorbisFile.dll.

public enum EArizona : byte
{
    #region outgoing (client -> server)

    // u8 key, u8 unknown
    SendKey = 0,

    // bool is_open
    SendSwitchChatState = 1,

    // u8 state (1=left, 2=right, 3=hazard, 0=off)
    SendTurnLights = 2,

    // injectCode reply path in vorbisFile.dll: u32 response_value, u32 request_id
    InjectCodeResponse = 17,

    // string16 text, u32 server_id
    Send = 18,

    // client viewport reply in vorbisFile.dll: u32 width, u32 height
    ClientViewport = 20,

    // module memory read reply in vorbisFile.dll: request header + u8 status + [raw data]
    ModuleReadResponse = 21,

    // browser control state reply in vorbisFile.dll: u32 browser_id, bool state
    BrowserControlStateReply = 24,

    // vorbisFile.dll HWID digest:
    // GetVolumeInformationA->VolumeSerialNumber + volume serial(DeviceIoControl(IOCTL_STORAGE_QUERY_PROPERTY)) -> XOR(volume serial key) -> SHA-256 -> 64-char hex
    SendHWID = 38,

    // core.asi VehicleSpeedLimiter: u8 state (0=disabled/reset, 1=active)
    SendVehicleSpeedLimiterState = 41,

    // u8 mode
    SendSwitchChatMode = 51,

    // SRCursor outgoing sync in vorbisFile.dll: f32 x, f32 y
    SendSrcursorPosition = 57,

    // vorbisFile.dll InCarNanChecks: u8 reportKind(0), u16 vehicleId
    SendInCarNanPosition = 58,

    // vorbisFile.dll InCarNanChecks: u8 reportKind(2), u16 vehicleId
    SendInCarNanQuaternion = 60,

    // vorbisFile.dll InCarNanChecks: u8 reportKind(1), u16 vehicleId
    SendInCarNanTrainSpeed = 61,

    // core.asi KeyboardHandler: u8 keyboardLayoutLowByte, bit-bool capsLockOn
    SendKeyboardLayoutCapsState = 79,

    // float value
    SendFloatValue = 113,

    // bool state
    SendToggleActionState = 115,

    // vec3 position
    SendTargetPosition = 116,

    // core.asi SimpleTuning: u32 accumulatedValue, u8 tierStep
    SendSimpleTuningProgress = 119,

    // vorbisFile.dll: current process command line string from GetCommandLineA
    SendCommandLine = 140,

    // float heading
    SendDroneHeading = 148,

    // u8 mode/state
    SendPortalToggle = 167,

    // core.asi Portal preview sync: u8 portalType, vec3 pointA, vec3 pointB
    SendPortalPlacementPreview = 168,

    // u8 direction (0=up, 1=down)
    SendWeaponScroll = 184,

    // u8 weapon_id (observed 54, 55, 56)
    SendDamageResponseWeapon = 195,

    // navigation_arrow outgoing ack: u8 selected target index
    SendNavigationArrowSelection = 198,

    #endregion

    #region incoming (server -> client)

    // u8 seat_code (always 0x02), bool state
    SetLocalDriver = 0,

    // u16 vehicle_id, u8 state (1=left, 2=right, 3=hazard, 0=off)
    TurnLightUpdate = 2,

    // u8 satiety (0-100)
    SetSatiety = 3,

    // u8 mode (0=off, 1=battleroyale, 2=vicecity) - CHudHook patches HUD rendering
    SetHudMode = 8,

    // u8 mode
    SetRadarMode = 9,

    // i32 billboard_id, byte[12] pad, maybeEncoded link, maybeEncoded user_agent, byte[12] pad
    PlayMediaOnBillboard = 12,

    // numericString browserId
    Close = 14,

    // numericString browserId, u32 value0, u32 value1
    Move = 15,

    // numericString browserId, maybeEncodedString url (vorbisFile.dll browser subsystem)
    ChangeUrl = 16,

    // numericString browserId, maybeEncoded text, u32 requestId
    InjectCode = 17,

    // maybeEncodedString text, u32 value
    SendMessage = 18,

    // numericString browserId
    ToggleScreen = 19,

    // server asks client to send current viewport/resolution back
    RequestClientViewport = 20,

    // u32 moduleOffset, u8 moduleNameLen, char[moduleNameLen], u32 size
    ModuleReadRequest = 21,

    // numericString browserId
    ToggleShow = 22,

    // numericString browserId, u32 value0, u32 value1, u8 value2
    BrowserClick = 23,

    // numericString browserId
    GetBrowserControlState = 24,

    // numericString browserId, bool state
    SetBrowserControlState = 25,

    //SetGreenZone = 26,

    // unknown payload
    Unknown27 = 27,

    // numericString browserId, u32 width, u32 height
    Resize = 28,

    // numericString browserId, u32 value0, u32 value1
    AddObject = 30,

    // numericString browserId, u32 value0, u32 value1
    RemoveObject = 31,

    // u16 server_id, u32 argb, float scale, u16 a, u16 b, u8 flags
    UiColorScale = 34,

    // u8 chat_id, string8 icon, i32 color, string8 chat_name, u8 flags
    // _chat.asi extends this into UpsertDynamicRoom: flags bit 0 = visible
    // color transform: argb = (color >> 8) | 0xFF000000
    SetChatGroup = 36,

    // _chat.asi: u8 room_id - soft-hides a dynamic chat room by setting inactive flag
    HideDynamicRoom = 37,

    // u8 state
    SetLocalInVehicle = 40,

    // u8 mode
    SetNicknameMode = 42,

    // _chat.asi: u8 state - toggles internal chat rendering flag
    SetChatFlag = 43,

    // unknown payload (arizona-events only, not in core.asi)
    Unknown47 = 47,

    // unknown payload (arizona-events only, not in core.asi)
    Unknown48 = 48,

    // u8 mode
    SwitchChatMode = 52,

    // SRCursor: u8 mode, if mode==2 then f32 min cursor delta for outgoing sync
    SrcursorSyncMode = 57,

    // Translate: u16 textDrawId, f32 x, f32 y
    TranslateObservedTextDrawPosition = 58,

    // Waypoint3D: bit-bool enabled, if enabled then f32 radius
    Waypoint3DSetRadius = 64,

    // Waypoint3D: bit-bool enabled, if enabled then f32 x, f32 y, f32 z
    Waypoint3DSetPosition = 65,

    // bool8 status
    ShowPositionInDiscord = 71,

    // ChatCommandHelper: bit-bool enabled
    ChatCommandHelperEnabled = 72,

    // u8 value - shares callback with SetRadarMode(9), exact module logic still being recovered
    Unknown74 = 74,

    // Discord: u32 byteLength, utf8 text bytes
    DiscordSetStateText = 80,

    // Discord: empty payload, clears rich presence state text
    DiscordClearStateText = 82,

    // core.asi CRadarHook: single-bit flag that toggles radar visibility/display state
    SetRadarVisibility = 86,

    // core.asi BattleroyaleCompass: u8 mode (0=off, 1=classic compass)
    SetCompassMode = 87,

    // core.asi BattleroyaleCompass: float x, float y (compass target position)
    SetCompassCoords = 88,

    // core.asi StunIcon: u8 value0, u8 value1, u8 value2 - shows stun icon overlay with 3 counters/values
    ShowStunIcon = 89,

    // core.asi StunIcon: no payload - hides stun icon overlay
    HideStunIcon = 90,

    // bool state - patches VirtualProtect byte in GameFunctions
    AutoDrinkBeer = 91,

    // bit-bool night_mode - toggles day/night color scheme
    SetDayNightColors = 92,

    // bool state - toggles compass/minimap element
    ToggleCompass = 93,

    // u32 value - animation property offset
    SetAnimationProperty = 97,

    // core.asi CGPS: bit-bool enabled - enables/disables custom GPS route processing/rendering
    ToggleCgps = 98,

    // bit-bool state - patches three code locations in GameFunctions
    ToggleMapColors = 101,

    // unknown payload
    Unknown102 = 102,

    // string32 host, u32 port, string32 nickname, optional string password, bool connect_mode
    ChangeServer = 103,

    // u8 bg_type, optional u32 timeout - ViceCityServer load screen
    ShowLoadScreenVc = 104,

    // unknown payload
    Unknown105 = 105,

    // core.asi ChatIcon: u32 playerId, bit-bool active - creates/removes rotating chat icon over a player
    SetChatIconState = 108,

    // core.asi GreenZone: u8 mode (0=off, 1=greenzone active)
    SetGreenZone = 110,

    // core.asi VehicleSpeedLimiter: f32 speedLimitOrMinusOne, u32 modelCount, u16[modelCount] vehicleModels
    SetVehicleModelSpeedLimit = 111,

    // u8 state, u8 unknown - spectator/camera memory patches
    SetSpectatorPatches = 112,

    // unknown payload
    Unknown114 = 114,

    // bit-bool state - ViceCityServer flag
    SetViceCityFlag = 117,

    // core.asi SimpleTuning + CarMods: u8 value (both modules store it globally)
    SetTuningConfig = 118,

    // not handled in core.asi custom-packet subscribers; raw payload only
    SetPlayerNametagFlags = 120,

    // core.asi SharedTxd: u8 stringLen, char[len] texture data, loads into "site16M" TXD
    LoadSharedTexture = 121,

    // core.asi SharedTxd: 1 bit toggle
    ToggleSharedTxdFlag = 122,

    // StreamFix: u8 mode - controls extra 4-byte append path in hooked stream builder
    StreamFixMode = 126,

    // u8 icon_id, byte[14] pad, u16 icon_model, vec3 position, string8 icon_name, u8 pad
    SetMapIcon = 127,

    // core.asi CustomMarker: u32 marker_id - deletes a custom map marker
    DeleteCustomMarker = 128,

    // core.asi CustomMarker: no payload - clears all custom map markers
    ClearCustomMarkers = 129,

    // u16 vehicle_id, bit-bool state
    TestDrive = 130,

    // core.asi VehicleLightsColor: u16 vehicleId, u32 argb
    SetVehicleLightsColor = 132,

    // u16 server_id, u8 index, float value
    // VehicleFeatures also processes 135 as neon color (u16 vid, u8 r, u8 g, u8 b, u8 a)
    UiScalar = 135,

    // core.asi AdminCheats: bit-bool state - toggle drive-on-water cheat, patches 0x969952
    SetDriveOnWater = 136,

    // core.asi AdminCheats: bit-bool state - toggle vehicle flight cheat
    SetVehicleFlight = 137,

    // AttachVehicleToVehicleData:
    // u16 vehicleId, u8 slot, bit-bool hasData
    // if hasData:
    //   vec3 offset
    //   vec3 rotationDegrees
    //   u8[14] componentIds
    //   u8 featureFlags
    //   u8 variantId
    //   u16 modelId
    //   u8 extraByte0
    //   u8 extraByte1
    //   f32 drawDistance
    AttachVehicleToVehicleData = 138,

    // u16 vehicle_id, float intensity, u8 r, u8 g, u8 b
    SetVehicleColorSmoke = 139,

    // core.asi HitInformer: u16 playerId, u32 color, float x, float y, u32 timeout, u32 extra, bit-bool active
    Create3DWaypoint = 141,

    // core.asi VehicleNeon: u16 vehicleId, u8 r, u8 g, u8 b, u8 a
    SetVehicleNeonColor = 142,

    // u8 tag0 (0x21), u8 tag1, u8 tag2, string names, u32[5] offsets, u16 end
    SetSkyboxImages = 144,

    // core.asi MapIconChange: u8 style - loads HUD theme + radar sprites
    SetHudStyle = 147,

    // bit-bool create - creates or destroys a 512x512 render target (Drone module)
    ToggleRenderTarget = 149,

    // u16 vehicle_id, bit-bool state - vehicle feature flag 1 (headlights)
    VehicleFeatureFlag1 = 150,

    // u16 vehicle_id, bit-bool state - vehicle feature flag 0 (nitro base)
    VehicleFeatureFlag0 = 151,

    // u16 vehicle_id, bit-bool state - vehicle feature flag 2 (nitro color)
    VehicleFeatureFlag2 = 152,

    // core.asi NumberPlate: u16 vehicleId, u8 plateStyle; if plateStyle!=0 then string8 plateText, string8 plateRegion
    SetVehicleNumberPlate = 153,

    // u16 player_id, i32 index, bool create, { i32 bone, i32 model, vec3 off, vec3 rot, vec3 scale, i32 c1, i32 c2 }
    SetPlayerAttachedObject = 155,

    // u16 vehicle_id, bit-bool state - vehicle feature reset (reset + flag6)
    VehicleFeatureReset = 156,

    // core.asi WeaponUpgrades: u8 weaponId, u8 upgradeCount, per-upgrade entries
    SetWeaponUpgrade = 157,

    // core.asi PlayerAnimGroup: u16 playerId, u8 count, per-entry: (string group, u32 packed, string anim, u8 selector)
    SetPlayerAnimGroups = 161,

    // unknown payload
    Unknown163 = 163,

    // unknown payload
    Unknown164 = 164,

    // string8 text - loads map IPL from data\maps\ (RemoveBuilding module)
    LoadBinary = 165,

    // bit-bool state - toggles portal visibility (Portal module)
    TogglePortal = 166,

    // u16 id (max 1004), u8 type (0=front, 1=back), vec3 offset, vec3 rotation
    CreatePortal = 169,

    // u16 id (max 1004), u8 type (0=front, 1=back)
    DestroyPortal = 170,

    // core.asi PlayerAnimGroup: u16 playerId, u16 nameLen, string animGroupName
    SetSingleAnimGroup = 171,

    // u8 unused, string8 text, stringUnread emoji
    SetCurrentTask = 172,

    // bool8 status
    ToggleDrawInterface = 174,

    // vec3 position, u16 pad, u8 interior, remaining bytes
    SetInterior = 175,

    // bit-bool state, u16 server_id
    UiToggle = 176,

    // core.asi WaterLevel: u8 mode, float level, [float target, u32 durationMs] when mode=1
    SetWaterLevel = 178,

    // WallHack: bit-bool enabled
    WallHackToggle = 179,

    // u16 vehicle_id, bit-bool state
    VehicleHeadlightsState = 180,

    // u32 world
    SetVirtualWorld = 183,

    // u16 vehicle_id, bool8 state
    SetVehicleDriftMode = 187,

    // core.asi Lines: u8 action (0-5), u16 line_id, variable payload per action
    SetLines = 188,

    // RadarFix: u16 playerIndex, optional u8 style, optional bit-bool lockFlag
    RadarFixPlayerStyle = 192,

    // core.asi VehicleLightsSize: u16 vehicleId, u8 stringLen, string lightName
    SetVehicleLights = 193,

    // SimpleAttachments: u16 playerId, u16 attachIndex, u8 selector, string8 materialName, string8 textureName, u8[4] extra
    SimpleAttachmentsSetMaterial = 194,

    // core.asi: no payload - resets first-person camera state (incoming alias for 195)
    ResetFirstPersonState = 195,

    // core.asi WeaponUpgrades: u8 weaponId, u8 count + per-slot entries; weaponId=0 reapplies all
    UpdateWeaponSlots = 196,

    // navigation_arrow: bit-bool followVertical, bit-bool specialMode, u8 count, count * { u16 x, u16 y, u16 z, u16 radius }
    NavigationArrowTargets = 197,

    // core.asi ViceCityServer: no payload - switches the Vice City load screen into the queue/full-server preset
    ShowLoadScreenVcQueue = 199,

    // core.asi HitInformer: u8 value - local-player feedback/UI path, exact meaning still unresolved
    Unknown200 = 200,

    // GoogleAnalytics / referral tracking bridge: u8 len, bytes[len] text, u32 flags
    GoogleAnalyticsMessage = 202,

    // core.asi VehicleStrobeLights: u16 vehicleId, u8 step, float speed, bit-bool beam
    SetVehicleStrobelights = 209,

    // _chat.asi: u32 color_rgba, u8 chat_type, [rich message segments...]
    // color transform: argb = (color_rgba >> 8) | 0xFF000000
    // Builds a ScreenChatMessage and dispatches it to room handlers.
    // If dword_10095554 is set, also relays as SAMP RPC 93 (ClientMessage).
    ChatMessageRelay = 210,

    // AttachVehicleToVehicle: bit-bool enabled
    AttachVehicleToVehicleToggle = 211,

    // u8 action (0=destroy, 1=create), u8 slot, endpoint data, u8 speed, bool loop, u32 color1, u32 color2
    SetGpsRoute = 212,

    // VehicleDamage: u16 groupId, u16 count, count * { u16 key, f32 value }
    VehicleDamageDoorPanelRules = 213,

    // bit-bool state - toggles first-person camera mode
    SetFirstPersonCamera = 215,

    // core.asi ExtendAnimGroups: player animation group extension data
    SetExtendAnimGroups = 216,

    // DirtySampObjects: bit-bool isAttachedObject, u8 dirtyLevel, u16 objectId, [u8 attachIndex], [u8 extra]
    DirtySampObjectsMakeObjectDirty = 218,

    // core.asi VehicleBrakeCalipers: u16 vehicleId, bit-bool toggle, [bit-bool isSimpleModel, u16 modelId]
    SetVehicleBrakeCalipersModel = 220,

    // core.asi ToggleHeadMove: bit-bool state - toggles head movement
    ToggleHeadMove = 221,

    // core.asi VehicleBrakeCalipers: u16 vehicleId, bit-bool toggle, [bit-bool simpleModel, u16 modelId]
    SetVehicleBrakeCalipers = 222,

    #endregion

    #region multiplexed / aliased packet IDs

    // RakCef simpleCreate packet
    SimpleCreate = 10,

    // byte[16] unknown, maybeEncoded js, maybeEncoded any, u32 server_id
    LoadJs = 10,

    // RakCef createScaled packet
    CreateScaled = 11,

    // RakCef objectCreate packet
    ObjectCreate = 12,

    // RakCef insideObjectCreate packet
    InsideObjectCreate = 13,

    // alias for the matching client reply payload shape
    StatePair = 20,

    // alias for the same incoming request packet
    MemoryQuery = 21,

    // bool status, float distance, u8 pad
    SetVisibleDistance3DMarker = 64,

    // vorbisFile.dll reuses this id for UiConfig (u8 type, u16 len)
    UiConfig = 110,

    // raw payload, cef_loader blip icon bridge
    BlipIcon = 0xBE,

    // raw payload, marker/icon batch path
    MarkerIconBatch = 0xBF,
    #endregion
}
