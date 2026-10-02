namespace SFSharp.Runtime.Networking.RakNet.Packets.Parsing;

public interface IIncomingPacketParser
{
    RakNetPacketId RakNetPacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingPacketParser
{
    RakNetPacketId RakNetPacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingPacketArgs args, out PacketParseResult result);
}

public interface IIncomingArizonaPacketParser
{
    RakNetPacketId RakNetPacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingArizonaPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingArizonaPacketParser
{
    RakNetPacketId RakNetPacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingArizonaPacketArgs args, out PacketParseResult result);
}
