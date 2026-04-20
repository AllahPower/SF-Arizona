namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// Arizona RP custom sub-packet identifiers carried inside packet 220.
/// </summary>
/// <remarks>
/// Assert: transport layout is `[u8 packetId=220] [u8 subId] [...payload...]`.
/// Assert: remaining payload shape is sub-ID specific.
/// Assert: names and payload notes are recovered from `arizona-events`, `core.asi`, `vorbisFile.dll`, `libcef.asi`, and live packet captures.
/// Assert: duplicate values are intentional and represent directional or alias semantics.
/// </remarks>
public enum EArizona : byte
{
    #region outgoing (client -> server)

    // Assert: u8 key, u8 unknown.
    SendKey = 0,

    // Assert: bool is_open.
    SendSwitchChatState = 1,

    // Assert: u8 state (1 = left, 2 = right, 3 = hazard, 0 = off).
    SendTurnLights = 2,

    // Assert: injectCode reply path in vorbisFile.dll: u32 response_value, u32 request_id.
    InjectCodeResponse = 17,

    // Assert: string16 text, u32 server_id.
    Send = 18,

    // Assert: client viewport reply in vorbisFile.dll: u32 width, u32 height.
    ClientViewport = 20,

    // Assert: module memory read reply in vorbisFile.dll: request header + u8 status + raw data.
    ModuleReadResponse = 21,

    // Assert: browser control state reply in vorbisFile.dll: u32 browser_id, bool state.
    BrowserControlStateReply = 24,

    // Assert: vorbisFile.dll HWID digest is built from volume serial data and hashed to a 64-char SHA-256 hex string.
    SendHWID = 38,

    // Assert: core.asi VehicleSpeedLimiter uses u8 state (0 = disabled/reset, 1 = active).
    SendVehicleSpeedLimiterState = 41,

    // Assert: u8 mode.
    SendSwitchChatMode = 51,

    // Assert: SRCursor outgoing sync in vorbisFile.dll: f32 x, f32 y.
    SendSrcursorPosition = 57,

    // Assert: vorbisFile.dll InCarNanChecks: u8 reportKind(0), u16 vehicleId.
    SendInCarNanPosition = 58,

    // Assert: vorbisFile.dll InCarNanChecks: u8 reportKind(2), u16 vehicleId.
    SendInCarNanQuaternion = 60,

    // Assert: vorbisFile.dll InCarNanChecks: u8 reportKind(1), u16 vehicleId.
    SendInCarNanTrainSpeed = 61,

    // Assert: core.asi KeyboardHandler sends keyboardLayoutLowByte plus a capsLock bit.
    SendKeyboardLayoutCapsState = 79,

    // Assert: float value.
    SendFloatValue = 113,

    // Assert: bool state.
    SendToggleActionState = 115,

    // Assert: vec3 position.
    SendTargetPosition = 116,

    // Assert: core.asi SimpleTuning sends u32 accumulatedValue plus u8 tierStep.
    SendSimpleTuningProgress = 119,

    // Assert: vorbisFile.dll sends the current process command line string from GetCommandLineA.
    SendCommandLine = 140,

    // Assert: float heading.
    SendDroneHeading = 148,

    // Assert: u8 mode/state.
    SendPortalToggle = 167,

    // Assert: core.asi Portal preview sync: u8 portalType, vec3 pointA, vec3 pointB.
    SendPortalPlacementPreview = 168,

    // Assert: u8 direction (0 = up, 1 = down).
    SendWeaponScroll = 184,

    // Assert: u8 weapon_id; observed values include 54, 55, 56.
    SendDamageResponseWeapon = 195,

    // Assert: navigation_arrow outgoing ack: u8 selected target index.
    SendNavigationArrowSelection = 198,

    #endregion

    #region incoming (server -> client)

    // Assert: u8 seat_code (always 0x02), bool state.
    SetLocalDriver = 0,

    // Assert: u16 vehicle_id, u8 state (1 = left, 2 = right, 3 = hazard, 0 = off).
    TurnLightUpdate = 2,

    // Assert: u8 satiety (0-100).
    SetSatiety = 3,

    // Assert: u8 mode (0 = off, 1 = battleroyale, 2 = vicecity); CHudHook patches HUD rendering.
    SetHudMode = 8,

    // Assert: u8 mode.
    SetRadarMode = 9,

    // Assert: i32 billboard_id, byte[12] pad, maybeEncoded link, maybeEncoded user_agent, byte[12] pad.
    PlayMediaOnBillboard = 12,

    // Assert: numericString browserId.
    Close = 14,

    // Assert: numericString browserId, u32 value0, u32 value1.
    Move = 15,

    // Assert: numericString browserId, maybeEncodedString url.
    ChangeUrl = 16,

    // Assert: numericString browserId, maybeEncoded text, u32 requestId.
    InjectCode = 17,

    // Assert: maybeEncodedString text, u32 value.
    SendMessage = 18,

    // Assert: numericString browserId.
    ToggleScreen = 19,

    // Assert: server asks the client to report the current viewport/resolution back.
    RequestClientViewport = 20,

    // Assert: u32 moduleOffset, u8 moduleNameLen, char[moduleNameLen], u32 size.
    ModuleReadRequest = 21,

    // Assert: numericString browserId.
    ToggleShow = 22,

    // Assert: numericString browserId, u32 value0, u32 value1, u8 value2.
    BrowserClick = 23,

    // Assert: numericString browserId.
    GetBrowserControlState = 24,

    // Assert: numericString browserId, bool state.
    SetBrowserControlState = 25,

    // Assert: unknown payload.
    Unknown27 = 27,

    // Assert: numericString browserId, u32 width, u32 height.
    Resize = 28,

    // Assert: numericString browserId, u32 value0, u32 value1.
    AddObject = 30,

    // Assert: numericString browserId, u32 value0, u32 value1.
    RemoveObject = 31,

    // Assert: vorbisFile.dll BulletTracers uses u16 groupId, u32 rgba, f32 time, bit long_tracers.
    // Assert: groupId is the internal BulletTracers preset-group selector captured from event/entity context, not a vehicle/player network id.
    // Assert: applies a partial group-base tracers preset; radius and laser_gun are not carried by packet 34 and stay inherited/preserved.
    SetBulletTracersGroupPreset = 34,

    // Assert: vorbisFile.dll BulletTracers uses u16 groupId, optional u8 slot, and when slot is present then u32 rgba, f32 time, bit long_tracers.
    // Assert: groupId is the internal BulletTracers preset-group selector captured from event/entity context, not a vehicle/player network id.
    // Assert: if slot byte is absent, packet 35 resets all 255 indexed presets in the group from defaults.
    // Assert: if slot byte exists but the rest of the payload is missing, the selected slot is reset from the default preset table.
    // Assert: like packet 34, this is a partial tracers preset update and does not carry radius or laser_gun.
    SetBulletTracersIndexedPreset = 35,

    // Assert: u8 chat_id, string8 icon, i32 color, string8 chat_name, u8 flags.
    // Assert: _chat.asi extends this into UpsertDynamicRoom where flags bit 0 means visible.
    // Assert: color transform is `(color >> 8) | 0xFF000000`.
    SetChatGroup = 36,

    // Assert: _chat.asi uses u8 room_id to soft-hide a dynamic chat room by setting the inactive flag.
    HideDynamicRoom = 37,

    // Assert: u8 state.
    SetLocalInVehicle = 40,

    // Assert: u8 mode.
    SetNicknameMode = 42,

    // Assert: _chat.asi toggles an internal chat rendering flag with u8 state.
    SetChatFlag = 43,

    // Assert: unknown payload from arizona-events only; not confirmed in core.asi.
    Unknown47 = 47,

    // Assert: unknown payload from arizona-events only; not confirmed in core.asi.
    Unknown48 = 48,

    // Assert: u8 mode.
    SwitchChatMode = 52,

    // Assert: vorbisFile.dll AntiAfk uses a single-bit enabled flag exported through isAntiAfk/setAntiAfk and changes the inactive sleep throttle path.
    SetAntiAfkEnabled = 54,

    // Assert: vorbisFile.dll SRCursor uses u8 mode, and when mode == 2 then f32 min cursor delta for outgoing sync.
    SrcursorSyncMode = 57,

    // Assert: vorbisFile.dll SRCursor observed-textdraw translate path uses u16 textDrawId, f32 x, f32 y.
    TranslateObservedTextDrawPosition = 58,

    // Assert: vorbisFile.dll Waypoint3D uses bit-bool enabled, and when enabled then f32 radius.
    Waypoint3DSetRadius = 64,

    // Assert: vorbisFile.dll Waypoint3D uses bit-bool enabled, and when enabled then f32 x, f32 y, f32 z.
    Waypoint3DSetPosition = 65,

    // Assert: vorbisFile.dll Discord rich presence toggle uses bool8 status.
    ShowPositionInDiscord = 71,

    // Assert: vorbisFile.dll ChatCommandHelper uses a single enable bit.
    // Assert: enables the local chat-input suggestion helper that reacts to typed text and accepts selection with Ctrl+Space.
    ChatCommandHelperEnabled = 72,

    // Assert: vorbisFile.dll RadarFix shared route with packet 9; reads one u8 enable-like value.
    // Assert: 0 disables both RadarFix substates, any non-zero value enables them; the exact non-zero byte is not otherwise interpreted in the handler.
    SetRadarFixEnabled = 74,

    // Assert: vorbisFile.dll Discord payload is u32 byteLength plus UTF-8 text bytes.
    DiscordSetStateText = 80,

    // Assert: vorbisFile.dll empty payload that clears Discord rich presence state text.
    DiscordClearStateText = 82,

    // Assert: core.asi CRadarHook uses a single-bit flag to toggle radar visibility.
    SetRadarVisibility = 86,

    // Assert: core.asi BattleroyaleCompass uses u8 mode (0 = off, 1 = classic compass).
    SetCompassMode = 87,

    // Assert: core.asi BattleroyaleCompass uses float x, float y for the compass target position.
    SetCompassCoords = 88,

    // Assert: core.asi StunIcon uses three u8 values and shows the stun overlay.
    ShowStunIcon = 89,

    // Assert: no payload; hides the stun icon overlay.
    HideStunIcon = 90,

    // Assert: bool state; patches a VirtualProtect byte in GameFunctions.
    AutoDrinkBeer = 91,

    // Assert: single-bit night_mode toggle.
    SetDayNightColors = 92,

    // Assert: bool state; toggles the compass / minimap element.
    ToggleCompass = 93,

    // Assert: u32 value representing an animation property offset.
    SetAnimationProperty = 97,

    // Assert: core.asi CGPS uses a single enable bit.
    ToggleCgps = 98,

    // Assert: single-bit state; patches three code locations in GameFunctions.
    ToggleMapColors = 101,

    // Assert: single-bit enable flag; toggles the gta_sa render/effect routine call at 0x53E170.
    SetRenderRoutineEnabled = 102,

    // Assert: string32 host, u32 port, string32 nickname, optional string password, bool connect_mode.
    ChangeServer = 103,

    // Assert: u8 bg_type plus optional u32 timeout for the ViceCityServer load screen.
    ShowLoadScreenVc = 104,

    // Assert: core.asi enables per-tick vehicle flight forward assist while W is held.
    SetVehicleFlightForwardAssist = 105,

    // Assert: core.asi ChatIcon uses u32 playerId plus bit-bool active to create or remove a rotating icon.
    SetChatIconState = 108,

    // Assert: core.asi GreenZone uses u8 mode (0 = off, 1 = active).
    SetGreenZone = 110,

    // Assert: core.asi VehicleSpeedLimiter uses f32 speedLimitOrMinusOne, u32 modelCount, u16[modelCount] vehicleModels.
    SetVehicleModelSpeedLimit = 111,

    // Assert: u8 state, u8 unknown for spectator / camera memory patches.
    SetSpectatorPatches = 112,

    // Assert: core.asi toggles RMB-driven local action-state handling with a single-bit flag.
    SetActionStateToggleEnabled = 114,

    // Assert: single-bit ViceCityServer flag.
    SetViceCityFlag = 117,

    // Assert: core.asi SimpleTuning + CarMods store a single u8 config value globally.
    SetTuningConfig = 118,

    // Assert: no confirmed packet-reader path yet; kept as raw payload only.
    SetPlayerNametagFlags = 120,

    // Assert: core.asi SharedTxd uses u8 stringLen + char[len] texture data and loads it into site16M TXD.
    LoadSharedTexture = 121,

    // Assert: SharedTxd toggle bit.
    ToggleSharedTxdFlag = 122,

    // Assert: StreamFix uses u8 mode and controls the extra 4-byte append path in the hooked stream builder.
    StreamFixMode = 126,

    // Assert: u8 icon_id, byte[14] pad, u16 icon_model, vec3 position, string8 icon_name, u8 pad.
    SetMapIcon = 127,

    // Assert: core.asi CustomMarker deletes a custom map marker by u32 marker_id.
    DeleteCustomMarker = 128,

    // Assert: core.asi CustomMarker clears all custom map markers.
    ClearCustomMarkers = 129,

    // Assert: u16 vehicle_id, bit-bool state.
    TestDrive = 130,

    // Assert: core.asi VehicleLightsColor uses u16 vehicleId, u32 argb.
    SetVehicleLightsColor = 132,

    // Assert: u16 server_id, u8 index, float value.
    // Assert: VehicleFeatures also reuses 135 as neon color with u16 vehicleId and RGBA bytes.
    UiScalar = 135,

    // Assert: core.asi AdminCheats toggles drive-on-water by patching 0x969952.
    SetDriveOnWater = 136,

    // Assert: core.asi AdminCheats toggles vehicle flight cheat.
    SetVehicleFlight = 137,

    // Assert: AttachVehicleToVehicleData uses u16 vehicleId, u8 slot, bit-bool hasData, and when present then
    // Assert: vec3 offset, vec3 rotationDegrees, u8[14] componentIds, u8 featureFlags, u8 variantId, u16 modelId,
    // Assert: u8 extraByte0, u8 extraByte1, f32 drawDistance.
    AttachVehicleToVehicleData = 138,

    // Assert: u16 vehicle_id, float intensity, u8 r, u8 g, u8 b.
    SetVehicleColorSmoke = 139,

    // Assert: core.asi HitInformer uses u16 playerId, u32 color, float x, float y, u32 timeout, u32 extra, bit-bool active.
    Create3DWaypoint = 141,

    // Assert: core.asi VehicleNeon uses u16 vehicleId plus RGBA bytes.
    SetVehicleNeonColor = 142,

    // Assert: u8 tag0 (0x21), u8 tag1, u8 tag2, string names, u32[5] offsets, u16 end.
    SetSkyboxImages = 144,

    // Assert: core.asi MapIconChange uses a single u8 style to load the HUD theme and radar sprites.
    SetHudStyle = 147,

    // Assert: single-bit create flag; creates or destroys a 512x512 render target for the Drone module.
    ToggleRenderTarget = 149,

    // Assert: u16 vehicle_id, bit-bool state; vehicle feature flag 1 (headlights).
    VehicleFeatureFlag1 = 150,

    // Assert: u16 vehicle_id, bit-bool state; vehicle feature flag 0 (nitro base).
    VehicleFeatureFlag0 = 151,

    // Assert: u16 vehicle_id, bit-bool state; vehicle feature flag 2 (nitro color).
    VehicleFeatureFlag2 = 152,

    // Assert: core.asi NumberPlate uses u16 vehicleId, u8 plateStyle, and when non-zero then string8 plateText + string8 plateRegion.
    SetVehicleNumberPlate = 153,

    // Assert: vorbisFile.dll / SimpleAttachments route: u16 player_id, i32 index, bool create, then attachment data block with model, bone, transforms, and colors.
    SetPlayerAttachedObject = 155,

    // Assert: u16 vehicle_id, bit-bool state; vehicle feature reset path.
    VehicleFeatureReset = 156,

    // Assert: core.asi WeaponUpgrades uses u8 weaponId, u8 upgradeCount, then per-upgrade entries.
    SetWeaponUpgrade = 157,

    // Assert: core.asi PlayerAnimGroup uses u16 playerId, u8 count, then per-entry group / packed / anim / selector data.
    SetPlayerAnimGroups = 161,

    // Assert: core.asi installs selector-aware hook logic with a single-bit enabled flag.
    SetSelectorHookEnabled = 163,

    // Assert: core.asi hard-disables selector-slot usability predicates with a single-bit blocked flag.
    SetSelectorSlotBlocked = 164,

    // Assert: string8 text; loads a map IPL from data\\maps\\ via the RemoveBuilding module.
    LoadBinary = 165,

    // Assert: single-bit flag that toggles portal visibility.
    TogglePortal = 166,

    // Assert: u16 id (max 1004), u8 type (0 = front, 1 = back), vec3 offset, vec3 rotation.
    CreatePortal = 169,

    // Assert: u16 id (max 1004), u8 type (0 = front, 1 = back).
    DestroyPortal = 170,

    // Assert: core.asi PlayerAnimGroup uses u16 playerId, u16 nameLen, string animGroupName.
    SetSingleAnimGroup = 171,

    // Assert: u8 unused, string8 text, stringUnread emoji.
    SetCurrentTask = 172,

    // Assert: bool8 status.
    ToggleDrawInterface = 174,

    // Assert: vec3 position, u16 pad, u8 interior, then remaining bytes.
    SetInterior = 175,

    // Assert: bit-bool state, u16 server_id.
    UiToggle = 176,

    // Assert: core.asi WaterLevel uses u8 mode, float level, and for mode 1 also float target + u32 durationMs.
    SetWaterLevel = 178,

    // Assert: WallHack single-bit toggle.
    WallHackToggle = 179,

    // Assert: u16 vehicle_id, bit-bool state.
    VehicleHeadlightsState = 180,

    // Assert: u32 world.
    SetVirtualWorld = 183,

    // Assert: u16 vehicle_id, bool8 state.
    SetVehicleDriftMode = 187,

    // Assert: core.asi Lines uses u8 action (0-5), u16 line_id, and variable payload per action.
    SetLines = 188,

    // Assert: RadarFix uses u16 playerIndex and may include optional style / lock fields.
    RadarFixPlayerStyle = 192,

    // Assert: core.asi VehicleLightsSize uses u16 vehicleId, u8 stringLen, string lightName.
    SetVehicleLights = 193,

    // Assert: vorbisFile.dll SimpleAttachments material override uses u16 playerId, u16 attachIndex, u8 selector, string8 materialName, string8 textureName, u8[4] extra.
    SimpleAttachmentsSetMaterial = 194,

    // Assert: no payload; resets first-person camera state.
    ResetFirstPersonState = 195,

    // Assert: core.asi WeaponUpgrades uses u8 weaponId, u8 count, then per-slot entries; weaponId = 0 reapplies all.
    UpdateWeaponSlots = 196,

    // Assert: navigation_arrow uses followVertical, specialMode, count, then count target entries.
    NavigationArrowTargets = 197,

    // Assert: special-case queue update path stores the queue position and feeds both chat and UI rendering.
    UpdateQueuePosition = 199,

    // Assert: core.asi HitInformer local-player feedback / UI path; exact meaning still unresolved.
    Unknown200 = 200,

    // Assert: GoogleAnalytics / referral bridge uses u8 len, bytes[len] text, u32 flags.
    GoogleAnalyticsMessage = 202,

    // Assert: core.asi VehicleStrobeLights uses u16 vehicleId, u8 step, float speed, bit-bool beam.
    SetVehicleStrobelights = 209,

    // Assert: _chat.asi uses u32 color_rgba, u8 chat_type, then rich message segments.
    // Assert: color transform is `(color_rgba >> 8) | 0xFF000000`.
    // Assert: when dword_10095554 is set, the message is also relayed as SA:MP RPC 93.
    ChatMessageRelay = 210,

    // Assert: single-bit AttachVehicleToVehicle toggle.
    AttachVehicleToVehicleToggle = 211,

    // Assert: u8 action (0 = destroy, 1 = create), u8 slot, endpoint data, u8 speed, bool loop, u32 color1, u32 color2.
    SetGpsRoute = 212,

    // Assert: VehicleDamage uses u16 groupId, u16 count, then count * { u16 key, f32 value }.
    VehicleDamageDoorPanelRules = 213,

    // Assert: single-bit first-person camera toggle.
    SetFirstPersonCamera = 215,

    // Assert: ExtendAnimGroups payload for player animation group extensions.
    SetExtendAnimGroups = 216,

    // Assert: DirtySampObjects uses bit-bool isAttachedObject, u8 dirtyLevel, u16 objectId, and optional attach / extra bytes.
    DirtySampObjectsMakeObjectDirty = 218,

    // Assert: core.asi VehicleBrakeCalipers uses u16 vehicleId, bit-bool toggle, and optional simple-model data.
    SetVehicleBrakeCalipersModel = 220,

    // Assert: core.asi toggles head movement with a single-bit flag.
    ToggleHeadMove = 221,

    // Assert: core.asi VehicleBrakeCalipers uses u16 vehicleId, bit-bool toggle, and optional simple-model data.
    SetVehicleBrakeCalipers = 222,

    // Assert: confirmed 220/subId route in vorbisFile.dll via direct listener registration.
    // Assert: belongs to the same SimpleAttachments cluster as SetPlayerAttachedObject(155) and SimpleAttachmentsSetMaterial(194).
    // Assert: downstream consumer applies text override to the selected attachment slot, but the exact wire structure is still unresolved.
    SimpleAttachmentsSetText = 226,

    #endregion

    #region multiplexed / aliased packet IDs

    // Assert: RakCef simpleCreate packet.
    SimpleCreate = 10,

    // Assert: byte[16] unknown, maybeEncoded JS, maybeEncoded any, u32 server_id.
    LoadJs = 10,

    // Assert: RakCef createScaled packet.
    CreateScaled = 11,

    // Assert: RakCef objectCreate packet.
    ObjectCreate = 12,

    // Assert: RakCef insideObjectCreate packet.
    InsideObjectCreate = 13,

    // Assert: alias for the matching client reply payload shape.
    StatePair = 20,

    // Assert: alias for the same incoming request packet.
    MemoryQuery = 21,

    // Assert: bool status, float distance, u8 pad.
    SetVisibleDistance3DMarker = 64,

    // Assert: vorbisFile.dll reuses this ID for UiConfig (u8 type, u16 len).
    UiConfig = 110,

    // Assert: vorbisFile.dll ScaleRadarMapIcons packet: u8 radarIconId, optional f32 scaleX, optional f32 scaleY; missing floats default to 1.0.
    ScaleRadarMapIcons = 0xBE,

    // Assert: vorbisFile.dll GangZonePoly packet: u8 zoneId, u32 pointCount, packed polygon points, RGBA, trailing bit flag.
    // Assert: recovered from GangZonePoly_ReadZonePacket in vorbisFile.dll; prior MarkerIconBatch naming was stale.
    GangZonePoly = 0xBF,

    #endregion
}


