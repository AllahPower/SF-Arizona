namespace SFSharp;

public interface IParsedPacket
{
    PacketId PacketId { get; }
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

public interface IIncomingPacketParser
{
    PacketId PacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingPacketParser
{
    PacketId PacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingPacketArgs args, out PacketParseResult result);
}

public interface IIncomingArizonaPacketParser
{
    PacketId PacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingArizonaPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingArizonaPacketParser
{
    PacketId PacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingArizonaPacketArgs args, out PacketParseResult result);
}
