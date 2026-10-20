namespace SFSharp.Protocol.Packets.Parsing;

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
