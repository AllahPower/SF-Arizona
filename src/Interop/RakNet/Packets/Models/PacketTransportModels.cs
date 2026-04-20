namespace SFSharp.Runtime.Network.RakNet.Packets;

public record IncomingSubPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload) : IParsedIncomingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public record OutgoingSubPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload) : IParsedOutgoingPacket, IParsedArizonaPacket
{
    public string? Detail => Payload is null ? null : Payload.ToString();
}

public sealed record IncomingArizonaPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload)
    : IncomingSubPacket<TPayload>(EPacketId, SubId, Name, Payload);

public sealed record OutgoingArizonaPacket<TPayload>(EPacketId EPacketId, int SubId, string Name, TPayload Payload)
    : OutgoingSubPacket<TPayload>(EPacketId, SubId, Name, Payload);

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
