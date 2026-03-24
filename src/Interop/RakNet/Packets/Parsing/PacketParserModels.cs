using SFSharp.Interop.RakNet.Packets.Enum;
using System.Numerics;

namespace SFSharp;

public sealed record IncomingConnectionAttemptFailedPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.ConnectionAttemptFailed;
    public string Name => nameof(EPacketId.ConnectionAttemptFailed);
    public string? Detail => null;
}

public sealed record IncomingNoFreeIncomingConnectionsPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.NoFreeIncomingConnections;
    public string Name => nameof(EPacketId.NoFreeIncomingConnections);
    public string? Detail => null;
}

public sealed record IncomingDisconnectionNotificationPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.DisconnectionNotification;
    public string Name => nameof(EPacketId.DisconnectionNotification);
    public string? Detail => null;
}

public sealed record IncomingConnectionRequestAcceptedPacket(int Ip, ushort Port, ushort PlayerId, int Challenge) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.ConnectionRequestAccepted;
    public string Name => nameof(EPacketId.ConnectionRequestAccepted);
    public string Detail => $"ip=0x{Ip:X8} port={Port} pid={PlayerId} challenge={Challenge}";
}

public sealed record IncomingAuthenticationPacket(string Key) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.Authentication;
    public string Name => nameof(EPacketId.Authentication);
    public string Detail => $"key={Key}";
}

public sealed record OutgoingAuthenticationPacket(string Response) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.Authentication;
    public string Name => nameof(EPacketId.Authentication);
    public string Detail => $"response={Response}";
}

public sealed record OutgoingRconCommandPacket(string Command) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.RconCommand;
    public string Name => nameof(EPacketId.RconCommand);
    public string Detail => $"cmd={Command}";
}

public sealed record IncomingRconResponsePacket(string Response) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.RconResponse;
    public string Name => nameof(EPacketId.RconResponse);
    public string Detail => $"response={Response}";
}

public sealed record IncomingConnectionLostPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.ConnectionLost;
    public string Name => nameof(EPacketId.ConnectionLost);
    public string? Detail => null;
}

public sealed record IncomingConnectionBannedPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.ConnectionBanned;
    public string Name => nameof(EPacketId.ConnectionBanned);
    public string? Detail => null;
}

public sealed record IncomingInvalidPasswordPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.InvalidPassword;
    public string Name => nameof(EPacketId.InvalidPassword);
    public string? Detail => null;
}

public sealed record IncomingConnectionFailedPacket() : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.ConnectionFailed;
    public string Name => nameof(EPacketId.ConnectionFailed);
    public string? Detail => null;
}

public sealed record IncomingOnfootPacket(ushort PlayerId, OnfootSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.OnfootData;
    public string Name => nameof(EPacketId.OnfootData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingOnfootPacket(OutgoingOnfootSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.OnfootData;
    public string Name => nameof(EPacketId.OnfootData);
    public string Detail => Data.ToString();
}

public sealed record IncomingIncarPacket(ushort PlayerId, IncarSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.IncarData;
    public string Name => nameof(EPacketId.IncarData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingIncarPacket(OutgoingIncarSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.IncarData;
    public string Name => nameof(EPacketId.IncarData);
    public string Detail => Data.ToString();
}

public sealed record IncomingAimPacket(ushort PlayerId, AimSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.AimData;
    public string Name => nameof(EPacketId.AimData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingAimPacket(AimSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.AimData;
    public string Name => nameof(EPacketId.AimData);
    public string Detail => Data.ToString();
}

public sealed record IncomingBulletPacket(ushort PlayerId, BulletSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.BulletData;
    public string Name => nameof(EPacketId.BulletData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingBulletPacket(BulletSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.BulletData;
    public string Name => nameof(EPacketId.BulletData);
    public string Detail => Data.ToString();
}

public sealed record IncomingPassengerPacket(ushort PlayerId, PassengerSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.PassengerData;
    public string Name => nameof(EPacketId.PassengerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingPassengerPacket(PassengerSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.PassengerData;
    public string Name => nameof(EPacketId.PassengerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingUnoccupiedPacket(ushort PlayerId, UnoccupiedSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.UnoccupiedData;
    public string Name => nameof(EPacketId.UnoccupiedData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingUnoccupiedPacket(UnoccupiedSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.UnoccupiedData;
    public string Name => nameof(EPacketId.UnoccupiedData);
    public string Detail => Data.ToString();
}

public sealed record IncomingTrailerPacket(ushort PlayerId, TrailerSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.TrailerData;
    public string Name => nameof(EPacketId.TrailerData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingTrailerPacket(TrailerSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.TrailerData;
    public string Name => nameof(EPacketId.TrailerData);
    public string Detail => Data.ToString();
}

public sealed record IncomingSpectatorPacket(ushort PlayerId, SpectatorSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.SpectatorData;
    public string Name => nameof(EPacketId.SpectatorData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingSpectatorPacket(SpectatorSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.SpectatorData;
    public string Name => nameof(EPacketId.SpectatorData);
    public string Detail => Data.ToString();
}

public sealed record IncomingWeaponsPacket(ushort PlayerId, WeaponsSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.WeaponsData;
    public string Name => nameof(EPacketId.WeaponsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingWeaponsPacket(WeaponsSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.WeaponsData;
    public string Name => nameof(EPacketId.WeaponsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingStatsPacket(ushort PlayerId, StatsSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.StatsData;
    public string Name => nameof(EPacketId.StatsData);
    public string Detail => $"pid={PlayerId} {Data}";
}

public sealed record OutgoingStatsPacket(StatsSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.StatsData;
    public string Name => nameof(EPacketId.StatsData);
    public string Detail => Data.ToString();
}

public sealed record IncomingMarkersPacket(byte MarkerSource, MarkersSyncData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.MarkersData;
    public string Name => nameof(EPacketId.MarkersData);
    public string Detail => $"src={MarkerSource} {Data}";
}

public sealed record OutgoingMarkersPacket(MarkersSyncData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.MarkersData;
    public string Name => nameof(EPacketId.MarkersData);
    public string Detail => Data.ToString();
}

public sealed record IncomingArizonaPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload) : IParsedIncomingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public sealed record OutgoingArizonaPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload) : IParsedOutgoingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public sealed record IncomingUnknownArizonaPacket(EPacketId EPacketId, int SubId, int PayloadBitLength, string Name) : IParsedIncomingPacket, IParsedArizonaPacket
{
    public string Detail => $"subId={SubId} bits={PayloadBitLength}";
}

public sealed record OutgoingUnknownArizonaPacket(EPacketId EPacketId, int SubId, int PayloadBitLength, string Name) : IParsedOutgoingPacket, IParsedArizonaPacket
{
    public string Detail => $"subId={SubId} bits={PayloadBitLength}";
}

public sealed record IncomingAZVoiceDataPacket(AzvVoiceData Data) : IParsedIncomingPacket
{
    public EPacketId EPacketId => EPacketId.AZVoice;
    public string Name => "AZVoice:VoiceData";
    public string Detail => $"sender={Data.SenderId} pkt={Data.PacketNumber} streams={Data.StreamIds.Length} opus={Data.OpusData.Length}B";
}

public sealed record OutgoingAZVoiceDataPacket(AzvOutgoingVoiceData Data) : IParsedOutgoingPacket
{
    public EPacketId EPacketId => EPacketId.AZVoice;
    public string Name => "AZVoice:VoiceData";
    public string Detail => $"pkt={Data.PacketNumber} streams=1 stream={Data.StreamId} opus={Data.OpusData.Length}B";
}
