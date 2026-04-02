namespace SFSharp.Interop.RakNet.Packets.Enum;

/// <summary>
/// Sync packet IDs for non-RPC raw packets.
/// Reference: https://github.com/Brunoo16/samp-packet-list/wiki/Packet-List
/// Internal: https://github.com/Brunoo16/samp-packet-list/wiki/Internal-Packet-List
/// </summary>
public enum EPacketId : byte
{
    #region incoming (server -> client)

    RemoteStaticData = 46,
    RemoteExistingConnection = 45,
    RemoteNewIncomingConnection = 44,
    RemoteConnectionLost = 43,
    RemoteDisconnectionNotification = 42,
    ReceivedStaticData = 41,
    Pong = 39,
    InvalidPassword = 37,
    ConnectionBanned = 36,
    InitializeEncryption = 35,
    ConnectionRequestAccepted = 34,
    ConnectionLost = 33,
    DisconnectionNotification = 32,
    NoFreeIncomingConnections = 31,
    NewIncomingConnection = 30,
    RconResponse = 202,

    #endregion

    #region outgoing (client -> server)

    AdvertiseSystem = 47,
    Timestamp = 40,
    RsaPublicKeyMismatch = 28,
    ConnectionCookie = 26,
    OpenConnectionReply = 25,
    OpenConnectionRequest = 24,
    DetectLostConnections = 23,
    RpcReply = 21,
    SetRandomNumberSeed = 19,
    RpcMapping = 18,
    SecuredConnectionConfirmation = 17,
    SecuredConnectionResponse = 16,
    BroadcastPings = 15,
    RequestStaticData = 10,
    PingOpenConnections = 8,
    Ping = 7,
    InternalPing = 6,
    RconCommand = 201,

    #endregion

    #region multiplexed / aliased

    // AZVoice plugin: packet 252 carries a u8 sub-ID (3-23) for control commands,
    // or raw voice data (Opus frames) without sub-ID dispatch.
    AZVoice = 252,

    // Arizona RP custom packet transports.
    ArizonaCefEx = 221,
    ArizonaCef = 220,

    // SA:MP sync packets used in both directions with different payload owners.
    SpectatorData = 212,
    SpectatorSync = 212,
    PassengerData = 211,
    PassengerSync = 211,
    TrailerData = 210,
    TrailerSync = 210,
    UnoccupiedData = 209,
    UnoccupiedSync = 209,
    MarkersData = 208,
    MarkersSync = 208,
    OnfootData = 207,
    PlayerSync = 207,
    BulletData = 206,
    BulletSync = 206,
    StatsData = 205,
    StatsUpdate = 205,
    WeaponsData = 204,
    WeaponsUpdate = 204,
    AimData = 203,
    AimSync = 203,
    IncarData = 200,
    VehicleSync = 200,

    // RakNet / SA:MP transport IDs reused with aliases or direction-dependent semantics.
    ConnectionAttemptFailed = 29,
    ConnectionFailed = 29,
    Rpc = 20,
    Authentication = 12,
    AuthKey = 12,
    ConnectionRequest = 11,
    ModifiedPacket = 38,
    ConnectionClosed = 32,

    #endregion
}
