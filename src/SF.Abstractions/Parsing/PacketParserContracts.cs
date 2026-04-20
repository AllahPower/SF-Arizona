namespace SFSharp.Abstractions.Parsing;

public interface IParsedPacket
{
    EPacketId EPacketId { get; }
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
