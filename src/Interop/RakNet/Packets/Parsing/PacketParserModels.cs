using System.Numerics;

namespace SFSharp;

public sealed record IncomingConnectionAttemptFailedPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.ConnectionAttemptFailed;
    public string Name => nameof(PacketId.ConnectionAttemptFailed);
    public string? Detail => null;
}

public sealed record IncomingNoFreeIncomingConnectionsPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.NoFreeIncomingConnections;
    public string Name => nameof(PacketId.NoFreeIncomingConnections);
    public string? Detail => null;
}

public sealed record IncomingDisconnectionNotificationPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.DisconnectionNotification;
    public string Name => nameof(PacketId.DisconnectionNotification);
    public string? Detail => null;
}

public sealed record IncomingConnectionRequestAcceptedPacket(int Ip, ushort Port, ushort PlayerId, int Challenge) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.ConnectionRequestAccepted;
    public string Name => nameof(PacketId.ConnectionRequestAccepted);
    public string Detail => $"ip=0x{Ip:X8} port={Port} pid={PlayerId} challenge={Challenge}";
}

public sealed record IncomingAuthenticationPacket(string Key) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.Authentication;
    public string Name => nameof(PacketId.Authentication);
    public string Detail => $"key={Key}";
}

public sealed record OutgoingAuthenticationPacket(string Response) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.Authentication;
    public string Name => nameof(PacketId.Authentication);
    public string Detail => $"response={Response}";
}

public sealed record OutgoingRconCommandPacket(string Command) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.RconCommand;
    public string Name => nameof(PacketId.RconCommand);
    public string Detail => $"cmd={Command}";
}

public sealed record IncomingConnectionLostPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.ConnectionLost;
    public string Name => nameof(PacketId.ConnectionLost);
    public string? Detail => null;
}

public sealed record IncomingConnectionBannedPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.ConnectionBanned;
    public string Name => nameof(PacketId.ConnectionBanned);
    public string? Detail => null;
}

public sealed record IncomingInvalidPasswordPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.InvalidPassword;
    public string Name => nameof(PacketId.InvalidPassword);
    public string? Detail => null;
}

public sealed record IncomingConnectionFailedPacket() : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.ConnectionFailed;
    public string Name => nameof(PacketId.ConnectionFailed);
    public string? Detail => null;
}

public sealed record IncomingOnfootPacket(ushort PlayerId, OnfootSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.OnfootData;
    public string Name => nameof(PacketId.OnfootData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingOnfootPacket(OutgoingOnfootSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.OnfootData;
    public string Name => nameof(PacketId.OnfootData);
    public string Detail => Data.ToString();
}

public sealed record IncomingIncarPacket(ushort PlayerId, IncarSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.IncarData;
    public string Name => nameof(PacketId.IncarData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingIncarPacket(OutgoingIncarSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.IncarData;
    public string Name => nameof(PacketId.IncarData);
    public string Detail => Data.ToString();
}

public sealed record IncomingAimPacket(ushort PlayerId, AimSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.AimData;
    public string Name => nameof(PacketId.AimData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingAimPacket(AimSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.AimData;
    public string Name => nameof(PacketId.AimData);
    public string Detail => Data.ToString();
}

public sealed record IncomingBulletPacket(ushort PlayerId, BulletSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.BulletData;
    public string Name => nameof(PacketId.BulletData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingBulletPacket(BulletSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.BulletData;
    public string Name => nameof(PacketId.BulletData);
    public string Detail => Data.ToString();
}

public sealed record IncomingPassengerPacket(ushort PlayerId, PassengerSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.PassengerData;
    public string Name => nameof(PacketId.PassengerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingPassengerPacket(PassengerSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.PassengerData;
    public string Name => nameof(PacketId.PassengerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingUnoccupiedPacket(ushort PlayerId, UnoccupiedSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.UnoccupiedData;
    public string Name => nameof(PacketId.UnoccupiedData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingUnoccupiedPacket(UnoccupiedSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.UnoccupiedData;
    public string Name => nameof(PacketId.UnoccupiedData);
    public string Detail => Data.ToString();
}

public sealed record IncomingTrailerPacket(ushort PlayerId, TrailerSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.TrailerData;
    public string Name => nameof(PacketId.TrailerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingTrailerPacket(TrailerSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.TrailerData;
    public string Name => nameof(PacketId.TrailerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingSpectatorPacket(ushort PlayerId, SpectatorSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.SpectatorData;
    public string Name => nameof(PacketId.SpectatorData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingSpectatorPacket(SpectatorSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.SpectatorData;
    public string Name => nameof(PacketId.SpectatorData);
    public string Detail => Data.ToString();
}

public sealed record IncomingWeaponsPacket(ushort PlayerId, WeaponsSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.WeaponsData;
    public string Name => nameof(PacketId.WeaponsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingWeaponsPacket(WeaponsSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.WeaponsData;
    public string Name => nameof(PacketId.WeaponsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingStatsPacket(ushort PlayerId, StatsSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.StatsData;
    public string Name => nameof(PacketId.StatsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingStatsPacket(StatsSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.StatsData;
    public string Name => nameof(PacketId.StatsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingMarkersPacket(byte MarkerSource, MarkersSyncData Data) : IParsedIncomingPacket
{
    public PacketId PacketId => PacketId.MarkersData;
    public string Name => nameof(PacketId.MarkersData);
    public string Detail => $"src={MarkerSource} {Data}";
}

public sealed record OutgoingMarkersPacket(MarkersSyncData Data) : IParsedOutgoingPacket
{
    public PacketId PacketId => PacketId.MarkersData;
    public string Name => nameof(PacketId.MarkersData);
    public string Detail => Data.ToString();
}

public sealed record IncomingArizonaPacket<TPayload>(PacketId PacketId, int SubId, string Name, TPayload Payload) : IParsedIncomingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public sealed record OutgoingArizonaPacket<TPayload>(PacketId PacketId, int SubId, string Name, TPayload Payload) : IParsedOutgoingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public sealed record IncomingUnknownArizonaPacket(PacketId PacketId, int SubId, int PayloadBitLength, string Name) : IParsedIncomingPacket, IParsedArizonaPacket
{
    public string Detail => $"subId={SubId} bits={PayloadBitLength}";
}

public sealed record OutgoingUnknownArizonaPacket(PacketId PacketId, int SubId, int PayloadBitLength, string Name) : IParsedOutgoingPacket, IParsedArizonaPacket
{
    public string Detail => $"subId={SubId} bits={PayloadBitLength}";
}
