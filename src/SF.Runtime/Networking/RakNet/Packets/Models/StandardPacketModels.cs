namespace SFSharp.Runtime.Networking.RakNet.Packets.Models;

#region incoming (server -> client)

public sealed record IncomingRconResponsePacket(string Response) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.RconResponse;
    public string Name => nameof(RakNetPacketId.RconResponse);
    public string Detail => $"response={Response}";
}

public sealed record IncomingInvalidPasswordPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.InvalidPassword;
    public string Name => nameof(RakNetPacketId.InvalidPassword);
    public string? Detail => null;
}

public sealed record IncomingConnectionBannedPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.ConnectionBanned;
    public string Name => nameof(RakNetPacketId.ConnectionBanned);
    public string? Detail => null;
}

public sealed record IncomingConnectionRequestAcceptedPacket(int Ip, ushort Port, ushort PlayerId, int Challenge) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.ConnectionRequestAccepted;
    public string Name => nameof(RakNetPacketId.ConnectionRequestAccepted);
    public string Detail => $"ip=0x{Ip:X8} port={Port} pid={PlayerId} challenge={Challenge}";
}

public sealed record IncomingConnectionLostPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.ConnectionLost;
    public string Name => nameof(RakNetPacketId.ConnectionLost);
    public string? Detail => null;
}

public sealed record IncomingDisconnectionNotificationPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.DisconnectionNotification;
    public string Name => nameof(RakNetPacketId.DisconnectionNotification);
    public string? Detail => null;
}

public sealed record IncomingNoFreeIncomingConnectionsPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.NoFreeIncomingConnections;
    public string Name => nameof(RakNetPacketId.NoFreeIncomingConnections);
    public string? Detail => null;
}

#endregion

#region outgoing (client -> server)

public sealed record OutgoingRconCommandPacket(string Command) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.RconCommand;
    public string Name => nameof(RakNetPacketId.RconCommand);
    public string Detail => $"cmd={Command}";
}

#endregion

#region multiplexed / aliased

public sealed record IncomingConnectionAttemptFailedPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.ConnectionAttemptFailed;
    public string Name => nameof(RakNetPacketId.ConnectionAttemptFailed);
    public string? Detail => null;
}

public sealed record IncomingConnectionFailedPacket() : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.ConnectionFailed;
    public string Name => nameof(RakNetPacketId.ConnectionFailed);
    public string? Detail => null;
}

public sealed record IncomingAuthenticationPacket(string Key) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.Authentication;
    public string Name => nameof(RakNetPacketId.Authentication);
    public string Detail => $"key={Key}";
}

public sealed record OutgoingAuthenticationPacket(string Response) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.Authentication;
    public string Name => nameof(RakNetPacketId.Authentication);
    public string Detail => $"response={Response}";
}

public sealed record IncomingSpectatorPacket(ushort PlayerId, SpectatorSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.SpectatorData;
    public string Name => nameof(RakNetPacketId.SpectatorData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingSpectatorPacket(SpectatorSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.SpectatorData;
    public string Name => nameof(RakNetPacketId.SpectatorData);
    public string Detail => Data.ToString();
}

public sealed record IncomingPassengerPacket(ushort PlayerId, PassengerSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.PassengerData;
    public string Name => nameof(RakNetPacketId.PassengerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingPassengerPacket(PassengerSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.PassengerData;
    public string Name => nameof(RakNetPacketId.PassengerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingTrailerPacket(ushort PlayerId, TrailerSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.TrailerData;
    public string Name => nameof(RakNetPacketId.TrailerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingTrailerPacket(TrailerSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.TrailerData;
    public string Name => nameof(RakNetPacketId.TrailerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingUnoccupiedPacket(ushort PlayerId, UnoccupiedSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.UnoccupiedData;
    public string Name => nameof(RakNetPacketId.UnoccupiedData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingUnoccupiedPacket(UnoccupiedSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.UnoccupiedData;
    public string Name => nameof(RakNetPacketId.UnoccupiedData);
    public string Detail => Data.ToString();
}

public sealed record IncomingMarkersPacket(byte MarkerSource, MarkersSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.MarkersData;
    public string Name => nameof(RakNetPacketId.MarkersData);
    public string Detail => $"src={MarkerSource} {Data}";
}

public sealed record OutgoingMarkersPacket(MarkersSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.MarkersData;
    public string Name => nameof(RakNetPacketId.MarkersData);
    public string Detail => Data.ToString();
}

public sealed record IncomingOnfootPacket(ushort PlayerId, OnfootSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.OnfootData;
    public string Name => nameof(RakNetPacketId.OnfootData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingOnfootPacket(OutgoingOnfootSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.OnfootData;
    public string Name => nameof(RakNetPacketId.OnfootData);
    public string Detail => Data.ToString();
}

public sealed record IncomingBulletPacket(ushort PlayerId, BulletSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.BulletData;
    public string Name => nameof(RakNetPacketId.BulletData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingBulletPacket(BulletSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.BulletData;
    public string Name => nameof(RakNetPacketId.BulletData);
    public string Detail => Data.ToString();
}

public sealed record IncomingStatsPacket(ushort PlayerId, StatsSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.StatsData;
    public string Name => nameof(RakNetPacketId.StatsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingStatsPacket(StatsSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.StatsData;
    public string Name => nameof(RakNetPacketId.StatsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingWeaponsPacket(ushort PlayerId, WeaponsSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.WeaponsData;
    public string Name => nameof(RakNetPacketId.WeaponsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingWeaponsPacket(WeaponsSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.WeaponsData;
    public string Name => nameof(RakNetPacketId.WeaponsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingAimPacket(ushort PlayerId, AimSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.AimData;
    public string Name => nameof(RakNetPacketId.AimData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingAimPacket(AimSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.AimData;
    public string Name => nameof(RakNetPacketId.AimData);
    public string Detail => Data.ToString();
}

public sealed record IncomingIncarPacket(ushort PlayerId, IncarSyncData Data) : IParsedIncomingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.IncarData;
    public string Name => nameof(RakNetPacketId.IncarData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingIncarPacket(OutgoingIncarSyncData Data) : IParsedOutgoingPacket
{
    public RakNetPacketId RakNetPacketId => RakNetPacketId.IncarData;
    public string Name => nameof(RakNetPacketId.IncarData);
    public string Detail => Data.ToString();
}

#endregion
