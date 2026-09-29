namespace SFSharp.Runtime.Network.RakNet.Packets;

public static partial class PacketParserCatalog
{
    #region standard packet registration

    private static void RegisterSync(PacketParserRegistry registry)
    {
        #region incoming (server -> client)

        registry.Register(new DelegateIncomingPacketParser<IncomingRconResponsePacket>(EPacketId.RconResponse, ParseIncomingRconResponse, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingInvalidPasswordPacket>(EPacketId.InvalidPassword, ParseIncomingInvalidPassword, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionBannedPacket>(EPacketId.ConnectionBanned, ParseIncomingConnectionBanned, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionRequestAcceptedPacket>(EPacketId.ConnectionRequestAccepted, ParseIncomingConnectionRequestAccepted, exactBitLength: 104));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionLostPacket>(EPacketId.ConnectionLost, ParseIncomingConnectionLost, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingDisconnectionNotificationPacket>(EPacketId.DisconnectionNotification, ParseIncomingDisconnectionNotification, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingNoFreeIncomingConnectionsPacket>(EPacketId.NoFreeIncomingConnections, ParseIncomingNoFreeIncomingConnections, exactBitLength: 8));

        #endregion

        #region outgoing (client -> server)

        registry.Register(new DelegateOutgoingPacketParser<OutgoingRconCommandPacket>(EPacketId.RconCommand, ParseOutgoingRconCommand, minimumBitLength: 40));

        #endregion

        #region multiplexed / aliased

        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionAttemptFailedPacket>(EPacketId.ConnectionAttemptFailed, ParseIncomingConnectionAttemptFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionFailedPacket>(EPacketId.ConnectionFailed, ParseIncomingConnectionFailed, exactBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingAuthenticationPacket>(EPacketId.Authentication, ParseIncomingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAuthenticationPacket>(EPacketId.Authentication, ParseOutgoingAuthentication, minimumBitLength: 16));

        registry.Register(new DelegateIncomingPacketParser<IncomingSpectatorPacket>(EPacketId.SpectatorData, ParseIncomingSpectator, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingSpectatorPacket>(EPacketId.SpectatorData, ParseOutgoingSpectator, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingPassengerPacket>(EPacketId.PassengerData, ParseIncomingPassenger, minimumBitLength: 24, exactBitLength: 216));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingPassengerPacket>(EPacketId.PassengerData, ParseOutgoingPassenger, minimumBitLength: 8, exactBitLength: 200));

        registry.Register(new DelegateIncomingPacketParser<IncomingTrailerPacket>(EPacketId.TrailerData, ParseIncomingTrailer, minimumBitLength: 24, exactBitLength: 456));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingTrailerPacket>(EPacketId.TrailerData, ParseOutgoingTrailer, minimumBitLength: 8, exactBitLength: 440));

        registry.Register(new DelegateIncomingPacketParser<IncomingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseIncomingUnoccupied, minimumBitLength: 560));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseOutgoingUnoccupied, minimumBitLength: 544));

        registry.Register(new DelegateIncomingPacketParser<IncomingMarkersPacket>(EPacketId.MarkersData, ParseIncomingMarkers, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingMarkersPacket>(EPacketId.MarkersData, ParseOutgoingMarkers, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingOnfootPacket>(EPacketId.OnfootData, ParseIncomingOnfoot, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingOnfootPacket>(EPacketId.OnfootData, ParseOutgoingOnfoot, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingBulletPacket>(EPacketId.BulletData, ParseIncomingBullet, minimumBitLength: 24, exactBitLength: 344));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingBulletPacket>(EPacketId.BulletData, ParseOutgoingBullet, minimumBitLength: 8, exactBitLength: 328));

        registry.Register(new DelegateIncomingPacketParser<IncomingStatsPacket>(EPacketId.StatsData, ParseIncomingStats, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingStatsPacket>(EPacketId.StatsData, ParseOutgoingStats, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingWeaponsPacket>(EPacketId.WeaponsData, ParseIncomingWeapons, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingWeaponsPacket>(EPacketId.WeaponsData, ParseOutgoingWeapons, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingAimPacket>(EPacketId.AimData, ParseIncomingAim, minimumBitLength: 24, exactBitLength: 272));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAimPacket>(EPacketId.AimData, ParseOutgoingAim, minimumBitLength: 8, exactBitLength: 256));

        registry.Register(new DelegateIncomingPacketParser<IncomingIncarPacket>(EPacketId.IncarData, ParseIncomingIncar, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingIncarPacket>(EPacketId.IncarData, ParseOutgoingIncar, minimumBitLength: 8));

        #endregion
    }

    #endregion
}
