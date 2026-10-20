namespace SFSharp.Protocol.Packets.Parsing;

public delegate TPacket IncomingPacketParseDelegate<TPacket>(IncomingPacketArgs args) where TPacket : IParsedIncomingPacket;
public delegate TPacket OutgoingPacketParseDelegate<TPacket>(OutgoingPacketArgs args) where TPacket : IParsedOutgoingPacket;

internal static class PacketParseHelper
{
    public static bool TryExecuteParse<TArgs>(
        TArgs args,
        int dataBitLength,
        int minimumBitLength,
        int? exactBitLength,
        string parserName,
        Func<TArgs, PacketParseResult> parseFunc,
        string errorContext,
        out PacketParseResult result)
    {
        if (dataBitLength < minimumBitLength)
        {
            result = PacketParseResult.TooShort(parserName);
            return false;
        }

        if (exactBitLength.HasValue && dataBitLength != exactBitLength.Value)
        {
            result = PacketParseResult.SizeMismatch(parserName);
            return false;
        }

        try
        {
            result = parseFunc(args);
            return result.Success;
        }
        catch (Exception ex)
        {
            ProtocolDiagnostics.Error($"Packet parse failed: parser={parserName} bits={dataBitLength} {errorContext}: {ex.Message}");
            result = PacketParseResult.FromException(parserName, ex);
            return false;
        }
    }
}

public abstract class IncomingPacketParserBase<TPacket> : IIncomingPacketParser
    where TPacket : IParsedIncomingPacket
{
    public abstract RakNetPacketId RakNetPacketId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(IncomingPacketArgs args, out PacketParseResult result)
    {
        return PacketParseHelper.TryExecuteParse(
            args,
            args.DataBitLength,
            MinimumBitLength,
            ExactBitLength,
            Name,
            a =>
            {
                TPacket packet = Parse(args);
                return new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            },
            $"packetId={RakNetPacketId}",
            out result);
    }

    protected abstract TPacket Parse(IncomingPacketArgs args);
}

public abstract class OutgoingPacketParserBase<TPacket> : IOutgoingPacketParser
    where TPacket : IParsedOutgoingPacket
{
    public abstract RakNetPacketId RakNetPacketId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(OutgoingPacketArgs args, out PacketParseResult result)
    {
        return PacketParseHelper.TryExecuteParse(
            args,
            args.DataBitLength,
            MinimumBitLength,
            ExactBitLength,
            Name,
            args =>
            {
                TPacket packet = Parse(args);
                return new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            },
            $"packetId={RakNetPacketId}",
            out result);
    }

    protected abstract TPacket Parse(OutgoingPacketArgs args);
}

public sealed class DelegateIncomingPacketParser<TPacket> : IncomingPacketParserBase<TPacket>
    where TPacket : IParsedIncomingPacket
{
    private readonly RakNetPacketId _packetId;
    private readonly string _name;
    private readonly IncomingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateIncomingPacketParser(RakNetPacketId packetId, IncomingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}IncomingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override RakNetPacketId RakNetPacketId => _packetId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TPacket Parse(IncomingPacketArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateOutgoingPacketParser<TPacket> : OutgoingPacketParserBase<TPacket>
    where TPacket : IParsedOutgoingPacket
{
    private readonly RakNetPacketId _packetId;
    private readonly string _name;
    private readonly OutgoingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateOutgoingPacketParser(RakNetPacketId packetId, OutgoingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}OutgoingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override RakNetPacketId RakNetPacketId => _packetId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TPacket Parse(OutgoingPacketArgs args)
    {
        return _parser(args);
    }
}




