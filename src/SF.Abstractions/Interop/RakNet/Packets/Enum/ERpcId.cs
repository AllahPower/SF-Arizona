namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// Known SA:MP 0.3.7 RPC identifiers.
/// </summary>
/// <remarks>
/// Assert: incoming means server -> client.
/// Assert: outgoing means client -> server.
/// Assert: some IDs are intentionally listed in bidirectional groups because the same numeric RPC is reused with different semantics.
/// Assert: reference list is based on https://github.com/Brunoo16/samp-packet-list/wiki/RPC-List
/// </remarks>
public enum ERpcId : byte
{
    #region incoming (server -> client)

    SetPlayerName = 11,
    SetPlayerPos = 12,
    SetPlayerPosFindZ = 13,
    SetPlayerHealth = 14,
    TogglePlayerControllable = 15,
    PlaySound = 16,
    SetWorldBounds = 17,
    GivePlayerMoney = 18,
    SetPlayerFacingAngle = 19,
    ResetPlayerMoney = 20,
    ResetPlayerWeapons = 21,
    GivePlayerWeapon = 22,
    SetVehicleParamsEx = 24,
    CancelEdit = 28,
    SetPlayerTime = 29,
    ToggleClock = 30,
    WorldPlayerAdd = 32,
    SetShopName = 33,
    SetPlayerSkillLevel = 34,
    SetPlayerDrunkLevel = 35,
    Create3DTextLabel = 36,
    DisableCheckpoint = 37,
    SetRaceCheckpoint = 38,
    DisableRaceCheckpoint = 39,
    GameModeRestart = 40,
    PlayAudioStream = 41,
    StopAudioStream = 42,
    RemoveBuildingForPlayer = 43,
    CreateObject = 44,
    SetObjectPos = 45,
    SetObjectRot = 46,
    DestroyObject = 47,
    DeathMessage = 55,
    SetPlayerMapIcon = 56,
    RemoveVehicleComponent = 57,
    Destroy3DTextLabel = 58,
    ChatBubble = 59,
    UpdateTime = 60,
    ShowDialog = 61,
    DestroyPickup = 63,
    LinkVehicleToInterior = 65,
    SetPlayerArmour = 66,
    SetPlayerArmedWeapon = 67,
    SetSpawnInfo = 68,
    SetPlayerTeam = 69,
    PutPlayerInVehicle = 70,
    RemovePlayerFromVehicle = 71,
    SetPlayerColor = 72,
    DisplayGameText = 73,
    ForceClassSelection = 74,
    AttachObjectToPlayer = 75,
    InitMenu = 76,
    ShowMenu = 77,
    HideMenu = 78,
    CreateExplosion = 79,
    ShowPlayerNameTag = 80,
    AttachCameraToObject = 81,
    InterpolateCamera = 82,
    SetObjectMaterial = 84,
    GangZoneStopFlash = 85,
    ApplyPlayerAnimation = 86,
    ClearPlayerAnimations = 87,
    SetPlayerSpecialAction = 88,
    SetPlayerFightingStyle = 89,
    SetPlayerVelocity = 90,
    SetVehicleVelocity = 91,
    SetPlayerDrunkVisuals = 92,
    ClientMessage = 93,
    SetWorldTime = 94,
    CreatePickup = 95,
    SetVehicleTires = 98,
    MoveObject = 99,
    EnableStuntBonus = 104,
    TextDrawSetString = 105,
    SetCheckpoint = 107,
    CreateGangZone = 108,
    ToggleWidescreen = 111,
    PlayCrimeReport = 112,
    SetPlayerAttachedObject = 113,
    GangZoneDestroy = 120,
    GangZoneFlash = 121,
    StopObject = 122,
    SetNumberPlate = 123,
    TogglePlayerSpectating = 124,
    PlayerSpectatePlayer = 126,
    PlayerSpectateVehicle = 127,
    ConnectionRejected = 130,
    SetPlayerWantedLevel = 133,
    ShowTextDraw = 134,
    HideTextDraw = 135,
    ServerJoin = 137,
    ServerQuit = 138,
    InitGame = 139,
    RemovePlayerMapIcon = 144,
    SetPlayerAmmo = 145,
    SetGravity = 146,
    SetVehicleHealth = 147,
    AttachTrailerToVehicle = 148,
    DetachTrailerFromVehicle = 149,
    SetPlayerDrunkHandling = 150,
    SetWeather = 152,
    SetPlayerSkin = 153,
    SetPlayerInterior = 156,
    SetPlayerCameraPos = 157,
    SetPlayerCameraLookAt = 158,
    SetVehiclePos = 159,
    SetVehicleZAngle = 160,
    SetVehicleParamsForPlayer = 161,
    SetCameraBehindPlayer = 162,
    WorldPlayerRemove = 163,
    WorldVehicleAdd = 164,
    WorldVehicleRemove = 165,
    WorldPlayerDeath = 166,
    DisableVehicleCollisions = 167,
    SetPlayerObjectNoCameraCol = 169,
    ToggleCameraTarget = 170,
    CreateActor = 171,
    DestroyActor = 172,
    ApplyActorAnimation = 173,
    ClearActorAnimation = 174,
    SetActorFacingAngle = 175,
    SetActorPos = 176,
    SetActorHealth = 178,

    #endregion

    #region bidirectional (client <-> server)

    // Assert: client notifies server about entering a vehicle; server re-broadcasts the event.
    EnterVehicle = 26,

    // Assert: client reports a textdraw click; server also uses the same ID to toggle select mode.
    ClickTextDraw = 83,

    // Assert: reused for SCM / vehicle tuning notifications.
    ScmEvent = 96,

    // Assert: client sends chat text; server emits chat payloads back on the same RPC ID.
    Chat = 101,

    // Assert: server requests a client check and the client responds on the same ID.
    ClientCheck = 103,

    UpdateVehicleDamageStatus = 106,
    EditAttachedObject = 116,
    EditObject = 117,
    RequestClass = 128,
    RequestSpawn = 129,

    // Assert: client notifies server about exiting a vehicle; server re-broadcasts the event.
    ExitVehicle = 154,

    UpdateScoresAndPings = 155,

    #endregion

    #region bidirectional (different parser per direction)

    // Assert: the same numeric ID is reused in both directions, but incoming and outgoing payloads are parsed differently.
    SelectObject = 27,

    #endregion

    #region outgoing (client -> server)

    ClickPlayer = 23,
    ClientJoin = 25,
    ScriptCash = 31,
    ServerCommand = 50,
    Spawn = 52,
    Death = 53,
    NpcJoin = 54,
    DialogResponse = 62,
    WeaponPickupDestroy = 97,
    SrvNetStats = 102,
    GiveTakeDamage = 115,
    SetInteriorId = 118,
    MapMarker = 119,
    PickedUpPickup = 131,
    MenuSelect = 132,
    VehicleDestroyed = 136,
    MenuQuit = 140,
    DestroyWeaponPickup = 151,
    CameraTargetUpdate = 168,
    GiveActorDamage = 177,

    #endregion
}
