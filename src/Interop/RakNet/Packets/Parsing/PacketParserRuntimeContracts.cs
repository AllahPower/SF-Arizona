using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Networking;

public interface IIncomingPacketParser
{
    EPacketId EPacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingPacketParser
{
    EPacketId EPacketId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingPacketArgs args, out PacketParseResult result);
}

public interface IIncomingArizonaPacketParser
{
    EPacketId EPacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(IncomingArizonaPacketArgs args, out PacketParseResult result);
}

public interface IOutgoingArizonaPacketParser
{
    EPacketId EPacketId { get; }
    int SubId { get; }
    Type ParsedType { get; }
    string Name { get; }

    bool TryParse(OutgoingArizonaPacketArgs args, out PacketParseResult result);
}
