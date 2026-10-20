namespace SFSharp.Protocol.Packets.Catalog;

public static partial class PacketParserCatalog
{
    public static PacketParserRegistry CreateDefaultRegistry()
    {
        PacketParserRegistry registry = new();
        RegisterSync(registry);
        return registry;
    }
}

