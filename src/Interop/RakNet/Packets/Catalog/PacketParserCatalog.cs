using SFSharp.Interop.RakNet.Arizona.Enum;
using SFSharp.Interop.RakNet.Packets.Enum;

namespace SFSharp;

public static partial class PacketParserCatalog
{
    public static PacketParserRegistry CreateDefaultRegistry()
    {
        PacketParserRegistry registry = new();
        RegisterTransportRouters(registry);
        RegisterSync(registry);
        RegisterArizona220(registry);
        RegisterArizona221(registry);
        RegisterAZVoice(registry);
        return registry;
    }

    private static void Register220Incoming<TPayload>(PacketParserRegistry registry, EArizona subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingSubPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ArizonaPacketTransportParsing.ParseIncoming220(args, subId, packetName, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register220Outgoing<TPayload>(PacketParserRegistry registry, EArizona subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingSubPacket<TPayload>>(EPacketId.ArizonaCef, (int)subId, args => ArizonaPacketTransportParsing.ParseOutgoing220(args, subId, packetName, parser), name: $"Arizona220:{packetName}"));
    }

    private static void Register221Incoming<TPayload>(PacketParserRegistry registry, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingSubPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ArizonaPacketTransportParsing.ParseIncoming221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void Register221Outgoing<TPayload>(PacketParserRegistry registry, EArizonaEx subId, ArizonaReaderParser<TPayload> parser)
    {
        registry.Register(new DelegateOutgoingArizonaPacketParser<OutgoingSubPacket<TPayload>>(EPacketId.ArizonaCefEx, (int)subId, args => ArizonaPacketTransportParsing.ParseOutgoing221(args, subId, parser), name: $"Arizona221:{subId}"));
    }

    private static void RegisterAZVoiceIncoming<TPayload>(PacketParserRegistry registry, EAZVoice subId, ArizonaReaderParser<TPayload> parser, string? name = null)
    {
        string packetName = name ?? subId.ToString();
        registry.Register(new DelegateIncomingArizonaPacketParser<IncomingSubPacket<TPayload>>(EPacketId.AZVoice, (int)subId, args => AZVoiceTransportParsing.ParseIncomingControl(args, subId, packetName, parser), name: $"AZVoice:{packetName}"));
    }

    private static void RegisterTransportRouters(PacketParserRegistry registry)
    {
        Arizona220PacketTransportRouter arizona220 = new();
        registry.Register((IIncomingPacketTransportRouter)arizona220);
        registry.Register((IOutgoingPacketTransportRouter)arizona220);

        Arizona221PacketTransportRouter arizona221 = new();
        registry.Register((IIncomingPacketTransportRouter)arizona221);
        registry.Register((IOutgoingPacketTransportRouter)arizona221);

        registry.Register((IIncomingPacketTransportRouter)new AZVoiceIncomingPacketTransportRouter());
    }
}

