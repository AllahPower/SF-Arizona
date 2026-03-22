using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    private static void RegisterSync(PacketParserRegistry registry)
    {
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionRequestAcceptedPacket>(EPacketId.ConnectionRequestAccepted, ParseIncomingConnectionRequestAccepted, exactBitLength: 104));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionAttemptFailedPacket>(EPacketId.ConnectionAttemptFailed, ParseIncomingConnectionAttemptFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingNoFreeIncomingConnectionsPacket>(EPacketId.NoFreeIncomingConnections, ParseIncomingNoFreeIncomingConnections, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingDisconnectionNotificationPacket>(EPacketId.DisconnectionNotification, ParseIncomingDisconnectionNotification, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAuthenticationPacket>(EPacketId.Authentication, ParseIncomingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAuthenticationPacket>(EPacketId.Authentication, ParseOutgoingAuthentication, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingRconCommandPacket>(EPacketId.RconCommand, ParseOutgoingRconCommand, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingRconResponsePacket>(EPacketId.RconResponse, ParseIncomingRconResponse, minimumBitLength: 40));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionLostPacket>(EPacketId.ConnectionLost, ParseIncomingConnectionLost, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionBannedPacket>(EPacketId.ConnectionBanned, ParseIncomingConnectionBanned, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingInvalidPasswordPacket>(EPacketId.InvalidPassword, ParseIncomingInvalidPassword, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingConnectionFailedPacket>(EPacketId.ConnectionFailed, ParseIncomingConnectionFailed, exactBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingOnfootPacket>(EPacketId.OnfootData, ParseIncomingOnfoot, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingOnfootPacket>(EPacketId.OnfootData, ParseOutgoingOnfoot, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingIncarPacket>(EPacketId.IncarData, ParseIncomingIncar, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingIncarPacket>(EPacketId.IncarData, ParseOutgoingIncar, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingAimPacket>(EPacketId.AimData, ParseIncomingAim, minimumBitLength: 24, exactBitLength: 272));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingAimPacket>(EPacketId.AimData, ParseOutgoingAim, minimumBitLength: 8, exactBitLength: 256));
        registry.Register(new DelegateIncomingPacketParser<IncomingBulletPacket>(EPacketId.BulletData, ParseIncomingBullet, minimumBitLength: 24, exactBitLength: 344));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingBulletPacket>(EPacketId.BulletData, ParseOutgoingBullet, minimumBitLength: 8, exactBitLength: 328));
        registry.Register(new DelegateIncomingPacketParser<IncomingPassengerPacket>(EPacketId.PassengerData, ParseIncomingPassenger, minimumBitLength: 24, exactBitLength: 216));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingPassengerPacket>(EPacketId.PassengerData, ParseOutgoingPassenger, minimumBitLength: 8, exactBitLength: 200));
        registry.Register(new DelegateIncomingPacketParser<IncomingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseIncomingUnoccupied, minimumBitLength: 560));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingUnoccupiedPacket>(EPacketId.UnoccupiedData, ParseOutgoingUnoccupied, minimumBitLength: 544));
        registry.Register(new DelegateIncomingPacketParser<IncomingTrailerPacket>(EPacketId.TrailerData, ParseIncomingTrailer, minimumBitLength: 24, exactBitLength: 456));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingTrailerPacket>(EPacketId.TrailerData, ParseOutgoingTrailer, minimumBitLength: 8, exactBitLength: 440));
        registry.Register(new DelegateIncomingPacketParser<IncomingSpectatorPacket>(EPacketId.SpectatorData, ParseIncomingSpectator, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingSpectatorPacket>(EPacketId.SpectatorData, ParseOutgoingSpectator, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingWeaponsPacket>(EPacketId.WeaponsData, ParseIncomingWeapons, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingWeaponsPacket>(EPacketId.WeaponsData, ParseOutgoingWeapons, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingStatsPacket>(EPacketId.StatsData, ParseIncomingStats, minimumBitLength: 24));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingStatsPacket>(EPacketId.StatsData, ParseOutgoingStats, minimumBitLength: 8));
        registry.Register(new DelegateIncomingPacketParser<IncomingMarkersPacket>(EPacketId.MarkersData, ParseIncomingMarkers, minimumBitLength: 16));
        registry.Register(new DelegateOutgoingPacketParser<OutgoingMarkersPacket>(EPacketId.MarkersData, ParseOutgoingMarkers, minimumBitLength: 8));
    }

    private static IncomingConnectionAttemptFailedPacket ParseIncomingConnectionAttemptFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionAttemptFailedPacket();
    }

    private static IncomingNoFreeIncomingConnectionsPacket ParseIncomingNoFreeIncomingConnections(IncomingPacketArgs args)
    {
        return new IncomingNoFreeIncomingConnectionsPacket();
    }

    private static IncomingDisconnectionNotificationPacket ParseIncomingDisconnectionNotification(IncomingPacketArgs args)
    {
        return new IncomingDisconnectionNotificationPacket();
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

    private static OutgoingRconCommandPacket ParseOutgoingRconCommand(OutgoingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new OutgoingRconCommandPacket(reader.ReadStringUInt32Length());
    }

    private static IncomingRconResponsePacket ParseIncomingRconResponse(IncomingPacketArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        reader.SkipBytes(1);
        return new IncomingRconResponsePacket(reader.ReadStringUInt32Length());
    }

    private static IncomingConnectionLostPacket ParseIncomingConnectionLost(IncomingPacketArgs args)
    {
        return new IncomingConnectionLostPacket();
    }

    private static IncomingConnectionBannedPacket ParseIncomingConnectionBanned(IncomingPacketArgs args)
    {
        return new IncomingConnectionBannedPacket();
    }

    private static IncomingInvalidPasswordPacket ParseIncomingInvalidPassword(IncomingPacketArgs args)
    {
        return new IncomingInvalidPasswordPacket();
    }

    private static IncomingConnectionFailedPacket ParseIncomingConnectionFailed(IncomingPacketArgs args)
    {
        return new IncomingConnectionFailedPacket();
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


}


