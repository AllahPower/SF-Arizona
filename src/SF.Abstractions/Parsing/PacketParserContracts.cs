namespace SFSharp.Abstractions.Parsing;

public interface IParsedPacket
{
    RakNetPacketId RakNetPacketId { get; }
    string Name { get; }
    string? Detail { get; }
}

public interface IParsedIncomingPacket : IParsedPacket
{
}

public interface IParsedOutgoingPacket : IParsedPacket
{
}

public interface IParsedArizonaPacket : IParsedPacket
{
    int SubId { get; }
}
