namespace SFSharp.Runtime.Networking.RakNet.Packets.Catalog;

public static partial class PacketParserCatalog
{
    #region standard packet registration

    private static void RegisterSync(PacketParserRegistry registry)
    {
        #region incoming (server -> client)

        registry.Register(new DelegateIncomingPacketParser<IncomingRconResponsePacket>(RakNetPacketId.RconResponse, ParseIncomingRconResponse, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingInvalidPasswordPacket>(RakNetPacketId.InvalidPassword, ParseIncomingInvalidPassword, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionBannedPacket>(RakNetPacketId.ConnectionBanned, ParseIncomingConnectionBanned, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionRequestAcceptedPacket>(RakNetPacketId.ConnectionRequestAccepted, ParseIncomingConnectionRequestAccepted, exactBitLength: 104));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionLostPacket>(RakNetPacketId.ConnectionLost, ParseIncomingConnectionLost, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingDisconnectionNotificationPacket>(RakNetPacketId.DisconnectionNotification, ParseIncomingDisconnectionNotification, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingNoFreeIncomingConnectionsPacket>(RakNetPacketId.NoFreeIncomingConnections, ParseIncomingNoFreeIncomingConnections, exactBitLength: 8));

        #endregion

        #region outgoing (client -> server)

        registry.Register(new DelegateOutgoingPacketParser<OutgoingRconCommandPacket>(RakNetPacketId.RconCommand, ParseOutgoingRconCommand, minimumBitLength: 40));

        #endregion

        #region multiplexed / aliased

        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionAttemptFailedPacket>(RakNetPacketId.ConnectionAttemptFailed, ParseIncomingConnectionAttemptFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionFailedPacket>(RakNetPacketId.ConnectionFailed, ParseIncomingConnectionFailed, exactBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingAuthenticationPacket>(RakNetPacketId.Authentication, ParseIncomingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAuthenticationPacket>(RakNetPacketId.Authentication, ParseOutgoingAuthentication, minimumBitLength: 16));

        registry.Register(new DelegateIncomingPacketParser<IncomingSpectatorPacket>(RakNetPacketId.SpectatorData, ParseIncomingSpectator, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingSpectatorPacket>(RakNetPacketId.SpectatorData, ParseOutgoingSpectator, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingPassengerPacket>(RakNetPacketId.PassengerData, ParseIncomingPassenger, minimumBitLength: 24, exactBitLength: 216));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingPassengerPacket>(RakNetPacketId.PassengerData, ParseOutgoingPassenger, minimumBitLength: 8, exactBitLength: 200));

        registry.Register(new DelegateIncomingPacketParser<IncomingTrailerPacket>(RakNetPacketId.TrailerData, ParseIncomingTrailer, minimumBitLength: 24, exactBitLength: 456));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingTrailerPacket>(RakNetPacketId.TrailerData, ParseOutgoingTrailer, minimumBitLength: 8, exactBitLength: 440));

        registry.Register(new DelegateIncomingPacketParser<IncomingUnoccupiedPacket>(RakNetPacketId.UnoccupiedData, ParseIncomingUnoccupied, minimumBitLength: 560));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingUnoccupiedPacket>(RakNetPacketId.UnoccupiedData, ParseOutgoingUnoccupied, minimumBitLength: 544));

        registry.Register(new DelegateIncomingPacketParser<IncomingMarkersPacket>(RakNetPacketId.MarkersData, ParseIncomingMarkers, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingMarkersPacket>(RakNetPacketId.MarkersData, ParseOutgoingMarkers, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingOnfootPacket>(RakNetPacketId.OnfootData, ParseIncomingOnfoot, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingOnfootPacket>(RakNetPacketId.OnfootData, ParseOutgoingOnfoot, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingBulletPacket>(RakNetPacketId.BulletData, ParseIncomingBullet, minimumBitLength: 24, exactBitLength: 344));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingBulletPacket>(RakNetPacketId.BulletData, ParseOutgoingBullet, minimumBitLength: 8, exactBitLength: 328));

        registry.Register(new DelegateIncomingPacketParser<IncomingStatsPacket>(RakNetPacketId.StatsData, ParseIncomingStats, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingStatsPacket>(RakNetPacketId.StatsData, ParseOutgoingStats, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingWeaponsPacket>(RakNetPacketId.WeaponsData, ParseIncomingWeapons, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingWeaponsPacket>(RakNetPacketId.WeaponsData, ParseOutgoingWeapons, minimumBitLength: 8));

        registry.Register(new DelegateIncomingPacketParser<IncomingAimPacket>(RakNetPacketId.AimData, ParseIncomingAim, minimumBitLength: 24, exactBitLength: 272));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAimPacket>(RakNetPacketId.AimData, ParseOutgoingAim, minimumBitLength: 8, exactBitLength: 256));

        registry.Register(new DelegateIncomingPacketParser<IncomingIncarPacket>(RakNetPacketId.IncarData, ParseIncomingIncar, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingIncarPacket>(RakNetPacketId.IncarData, ParseOutgoingIncar, minimumBitLength: 8));

        #endregion
    }

    #endregion
}
