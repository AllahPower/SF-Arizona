namespace SFSharp;

public static partial class PacketParserCatalog
{
    #region incoming (server -> client)

    private static IncomingRconResponsePacket ParseIncomingRconResponse(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingRconResponsePacket(reader.ReadStringUInt32Length());
    }

    private static IncomingInvalidPasswordPacket ParseIncomingInvalidPassword(IncomingPacketArgs args)
    {
        return new IncomingInvalidPasswordPacket();
    }

    private static IncomingConnectionBannedPacket ParseIncomingConnectionBanned(IncomingPacketArgs args)
    {
        return new IncomingConnectionBannedPacket();
    }

    private static IncomingConnectionRequestAcceptedPacket ParseIncomingConnectionRequestAccepted(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        int ip = reader.ReadInt32();
        ushort port = reader.ReadUInt16();
        ushort playerId = reader.ReadUInt16();
        int challenge = reader.ReadInt32();
        return new IncomingConnectionRequestAcceptedPacket(ip, port, playerId, challenge);
    }

    private static IncomingConnectionLostPacket ParseIncomingConnectionLost(IncomingPacketArgs args)
    {
        return new IncomingConnectionLostPacket();
    }

    private static IncomingDisconnectionNotificationPacket ParseIncomingDisconnectionNotification(IncomingPacketArgs args)
    {
        return new IncomingDisconnectionNotificationPacket();
    }

    private static IncomingNoFreeIncomingConnectionsPacket ParseIncomingNoFreeIncomingConnections(IncomingPacketArgs args)
    {
        return new IncomingNoFreeIncomingConnectionsPacket();
    }

    #endregion

    #region outgoing (client -> server)

    private static OutgoingRconCommandPacket ParseOutgoingRconCommand(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingRconCommandPacket(reader.ReadStringUInt32Length());
    }

    #endregion

    #region multiplexed / aliased

    private static IncomingConnectionAttemptFailedPacket ParseIncomingConnectionAttemptFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionAttemptFailedPacket();
    }

    private static IncomingConnectionFailedPacket ParseIncomingConnectionFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionFailedPacket();
    }

    private static IncomingAuthenticationPacket ParseIncomingAuthentication(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static OutgoingAuthenticationPacket ParseOutgoingAuthentication(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAuthenticationPacket(reader.ReadStringUInt8Length());
    }

    private static IncomingSpectatorPacket ParseIncomingSpectator(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingSpectatorPacket(playerId, SpectatorSyncData.Parse(ref reader));
    }

    private static OutgoingSpectatorPacket ParseOutgoingSpectator(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingSpectatorPacket(SpectatorSyncData.Parse(ref reader));
    }

    private static IncomingPassengerPacket ParseIncomingPassenger(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingPassengerPacket(playerId, PassengerSyncData.Parse(ref reader));
    }

    private static OutgoingPassengerPacket ParseOutgoingPassenger(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingPassengerPacket(PassengerSyncData.Parse(ref reader));
    }

    private static IncomingTrailerPacket ParseIncomingTrailer(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingTrailerPacket(playerId, TrailerSyncData.Parse(ref reader));
    }

    private static OutgoingTrailerPacket ParseOutgoingTrailer(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingTrailerPacket(TrailerSyncData.Parse(ref reader));
    }

    private static IncomingUnoccupiedPacket ParseIncomingUnoccupied(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingUnoccupiedPacket(playerId, UnoccupiedSyncData.Parse(ref reader));
    }

    private static OutgoingUnoccupiedPacket ParseOutgoingUnoccupied(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingUnoccupiedPacket(UnoccupiedSyncData.Parse(ref reader));
    }

    private static IncomingMarkersPacket ParseIncomingMarkers(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        byte markerSource = reader.ReadUInt8();
        return new IncomingMarkersPacket(markerSource, MarkersSyncData.Parse(ref reader));
    }

    private static OutgoingMarkersPacket ParseOutgoingMarkers(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingMarkersPacket(MarkersSyncData.Parse(ref reader));
    }

    private static IncomingOnfootPacket ParseIncomingOnfoot(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingOnfootPacket(playerId, OnfootSyncData.Parse(ref reader));
    }

    private static OutgoingOnfootPacket ParseOutgoingOnfoot(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingOnfootPacket(OutgoingOnfootSyncData.Parse(ref reader));
    }

    private static IncomingBulletPacket ParseIncomingBullet(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingBulletPacket(playerId, BulletSyncData.Parse(ref reader));
    }

    private static OutgoingBulletPacket ParseOutgoingBullet(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingBulletPacket(BulletSyncData.Parse(ref reader));
    }

    private static IncomingStatsPacket ParseIncomingStats(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingStatsPacket(playerId, StatsSyncData.Parse(ref reader));
    }

    private static OutgoingStatsPacket ParseOutgoingStats(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingStatsPacket(StatsSyncData.Parse(ref reader));
    }

    private static IncomingWeaponsPacket ParseIncomingWeapons(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingWeaponsPacket(playerId, WeaponsSyncData.Parse(ref reader));
    }

    private static OutgoingWeaponsPacket ParseOutgoingWeapons(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingWeaponsPacket(WeaponsSyncData.Parse(ref reader));
    }

    private static IncomingAimPacket ParseIncomingAim(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingAimPacket(playerId, AimSyncData.Parse(ref reader));
    }

    private static OutgoingAimPacket ParseOutgoingAim(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingAimPacket(AimSyncData.Parse(ref reader));
    }

    private static IncomingIncarPacket ParseIncomingIncar(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        ushort playerId = reader.ReadUInt16();
        return new IncomingIncarPacket(playerId, IncarSyncData.Parse(ref reader));
    }

    private static OutgoingIncarPacket ParseOutgoingIncar(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingIncarPacket(OutgoingIncarSyncData.Parse(ref reader));
    }

    #endregion
}
