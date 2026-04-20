namespace SFSharp.Runtime.Network.RakNet.Packets;

public delegate TPacket IncomingPacketParseDelegate<TPacket>(IncomingPacketArgs args) where TPacket : IParsedIncomingPacket;
public delegate TPacket OutgoingPacketParseDelegate<TPacket>(OutgoingPacketArgs args) where TPacket : IParsedOutgoingPacket;
public delegate TPacket IncomingArizonaPacketParseDelegate<TPacket>(IncomingArizonaPacketArgs args) where TPacket : IParsedIncomingPacket;
public delegate TPacket OutgoingArizonaPacketParseDelegate<TPacket>(OutgoingArizonaPacketArgs args) where TPacket : IParsedOutgoingPacket;

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
            SFLog.Error($"Packet parse failed: parser={parserName} bits={dataBitLength} {errorContext}: {ex.Message}");
            result = PacketParseResult.FromException(parserName, ex);
            return false;
        }
    }
}

public abstract class IncomingPacketParserBase<TPacket> : IIncomingPacketParser
    where TPacket : IParsedIncomingPacket
{
    public abstract EPacketId EPacketId { get; }
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
            $"packetId={EPacketId}",
            out result);
    }

    protected abstract TPacket Parse(IncomingPacketArgs args);
}

public abstract class OutgoingPacketParserBase<TPacket> : IOutgoingPacketParser
    where TPacket : IParsedOutgoingPacket
{
    public abstract EPacketId EPacketId { get; }
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
            $"packetId={EPacketId}",
            out result);
    }

    protected abstract TPacket Parse(OutgoingPacketArgs args);
}

public abstract class IncomingArizonaPacketParserBase<TPacket> : IIncomingArizonaPacketParser
    where TPacket : IParsedIncomingPacket
{
    public abstract EPacketId EPacketId { get; }
    public abstract int SubId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumPayloadBitLength => 0;
    protected virtual int? ExactPayloadBitLength => null;

    public bool TryParse(IncomingArizonaPacketArgs args, out PacketParseResult result)
    {
        return PacketParseHelper.TryExecuteParse(
            args,
            args.PayloadBitLength,
            MinimumPayloadBitLength,
            ExactPayloadBitLength,
            Name,
            args =>
            {
                TPacket packet = Parse(args);
                return new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            },
            $"packetId={EPacketId} subId={SubId}",
            out result);
    }

    protected abstract TPacket Parse(IncomingArizonaPacketArgs args);
}

public abstract class OutgoingArizonaPacketParserBase<TPacket> : IOutgoingArizonaPacketParser
    where TPacket : IParsedOutgoingPacket
{
    public abstract EPacketId EPacketId { get; }
    public abstract int SubId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumPayloadBitLength => 0;
    protected virtual int? ExactPayloadBitLength => null;

    public bool TryParse(OutgoingArizonaPacketArgs args, out PacketParseResult result)
    {
        return PacketParseHelper.TryExecuteParse(
            args,
            args.PayloadBitLength,
            MinimumPayloadBitLength,
            ExactPayloadBitLength,
            Name,
            args =>
            {
                TPacket packet = Parse(args);
                return new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            },
            $"packetId={EPacketId} subId={SubId}",
            out result);
    }

    protected abstract TPacket Parse(OutgoingArizonaPacketArgs args);
}

public sealed class DelegateIncomingPacketParser<TPacket> : IncomingPacketParserBase<TPacket>
    where TPacket : IParsedIncomingPacket
{
    private readonly EPacketId _packetId;
    private readonly string _name;
    private readonly IncomingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateIncomingPacketParser(EPacketId packetId, IncomingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}IncomingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override EPacketId EPacketId => _packetId;
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
    private readonly EPacketId _packetId;
    private readonly string _name;
    private readonly OutgoingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateOutgoingPacketParser(EPacketId packetId, OutgoingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}OutgoingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override EPacketId EPacketId => _packetId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TPacket Parse(OutgoingPacketArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateIncomingArizonaPacketParser<TPacket> : IncomingArizonaPacketParserBase<TPacket>
    where TPacket : IParsedIncomingPacket
{
    private readonly EPacketId _packetId;
    private readonly int _subId;
    private readonly string _name;
    private readonly IncomingArizonaPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumPayloadBitLength;
    private readonly int? _exactPayloadBitLength;

    public DelegateIncomingArizonaPacketParser(EPacketId packetId, int subId, IncomingArizonaPacketParseDelegate<TPacket> parser, string? name = null, int minimumPayloadBitLength = 0, int? exactPayloadBitLength = null)
    {
        _packetId = packetId;
        _subId = subId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}:{subId}IncomingParser" : name;
        _minimumPayloadBitLength = minimumPayloadBitLength;
        _exactPayloadBitLength = exactPayloadBitLength;
    }

    public override EPacketId EPacketId => _packetId;
    public override int SubId => _subId;
    public override string Name => _name;
    protected override int MinimumPayloadBitLength => _minimumPayloadBitLength;
    protected override int? ExactPayloadBitLength => _exactPayloadBitLength;

    protected override TPacket Parse(IncomingArizonaPacketArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateOutgoingArizonaPacketParser<TPacket> : OutgoingArizonaPacketParserBase<TPacket>
    where TPacket : IParsedOutgoingPacket
{
    private readonly EPacketId _packetId;
    private readonly int _subId;
    private readonly string _name;
    private readonly OutgoingArizonaPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumPayloadBitLength;
    private readonly int? _exactPayloadBitLength;

    public DelegateOutgoingArizonaPacketParser(EPacketId packetId, int subId, OutgoingArizonaPacketParseDelegate<TPacket> parser, string? name = null, int minimumPayloadBitLength = 0, int? exactPayloadBitLength = null)
    {
        _packetId = packetId;
        _subId = subId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}:{subId}OutgoingParser" : name;
        _minimumPayloadBitLength = minimumPayloadBitLength;
        _exactPayloadBitLength = exactPayloadBitLength;
    }

    public override EPacketId EPacketId => _packetId;
    public override int SubId => _subId;
    public override string Name => _name;
    protected override int MinimumPayloadBitLength => _minimumPayloadBitLength;
    protected override int? ExactPayloadBitLength => _exactPayloadBitLength;

    protected override TPacket Parse(OutgoingArizonaPacketArgs args)
    {
        return _parser(args);
    }
}




