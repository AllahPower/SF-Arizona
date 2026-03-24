using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

internal delegate TPayload ArizonaReaderParser<TPayload>(ref BitStreamReader reader);

public static partial class PacketParserCatalog
{
    public static PacketParserRegistry CreateDefaultRegistry()
    {
        PacketParserRegistry registry = new();
        RegisterSync(registry);
        RegisterArizona220(registry);
        RegisterArizona221(registry);
        RegisterAZVoice(registry);
        return registry;
    }

    private static void Register220Incoming<TPayload>(PacketParserRegistry registry, EArizona subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ParseIncoming220(args, subId, packetName, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register220Outgoing<TPayload>(PacketParserRegistry registry, EArizona subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ParseOutgoing220(args, subId, packetName, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register221Incoming<TPayload>(PacketParserRegistry registry, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ParseIncoming221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void Register221Outgoing<TPayload>(PacketParserRegistry registry, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingArizonaPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ParseOutgoing221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void RegisterAZVoiceIncoming<TPayload>(PacketParserRegistry registry, EAZVoice subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingArizonaPacket<TPayload>>(EPacketId.AZVoice, (int)subId, args => ParseIncomingAZVoice(args, subId, packetName, parser), name: $"AZVoice:{packetName}"));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncomingAZVoice<TPayload>(IncomingArizonaPacketArgs args, EAZVoice subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(EPacketId.AZVoice, (int)subId, packetName, parser(ref reader));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming220<TPayload>(IncomingArizonaPacketArgs args, EArizona subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing220<TPayload>(OutgoingArizonaPacketArgs args, EArizona subId, string packetName, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(EPacketId.ArizonaCef, (int)subId, packetName, parser(ref reader));
    }

    private static IncomingArizonaPacket<TPayload> ParseIncoming221<TPayload>(IncomingArizonaPacketArgs args, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new IncomingArizonaPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }

    private static OutgoingArizonaPacket<TPayload> ParseOutgoing221<TPayload>(OutgoingArizonaPacketArgs args, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        BitStreamReader reader = args.CreateReader();
        return new OutgoingArizonaPacket<TPayload>(EPacketId.ArizonaCefEx, (int)subId, subId.ToString(), parser(ref reader));
    }
}

