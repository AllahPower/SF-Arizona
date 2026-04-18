namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// Known non-RPC RakNet and SA:MP packet identifiers.
/// </summary>
/// <remarks>
/// Assert: packet direction is grouped by how SA:MP 0.3.7 uses the ID on the wire.
/// Assert: duplicate numeric values are intentional and represent aliases or direction-specific semantics.
/// Assert: reference lists are based on https://github.com/Brunoo16/samp-packet-list/wiki/Packet-List
/// Assert: internal transport notes are based on https://github.com/Brunoo16/samp-packet-list/wiki/Internal-Packet-List
/// </remarks>
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

    // Assert: packet 252 carries AZVoice control sub-IDs 3-23 or raw voice frames without sub-ID dispatch.
    AZVoice = 252,

    // Assert: Arizona RP custom packet transports.
    ArizonaCefEx = 221,
    ArizonaCef = 220,

    // Assert: SA:MP sync packets reuse the same ID in both directions with different payload owners.
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

    // Assert: RakNet / SA:MP transport IDs reused with aliases or direction-specific naming.
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
