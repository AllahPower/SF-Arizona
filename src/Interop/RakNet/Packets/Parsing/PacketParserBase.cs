namespace SFSharp;

public delegate TPacket IncomingPacketParseDelegate<TPacket>(IncomingPacketArgs args) where TPacket : class, IParsedIncomingPacket;
public delegate TPacket OutgoingPacketParseDelegate<TPacket>(OutgoingPacketArgs args) where TPacket : class, IParsedOutgoingPacket;
public delegate TPacket IncomingArizonaPacketParseDelegate<TPacket>(IncomingArizonaPacketArgs args) where TPacket : class, IParsedIncomingPacket;
public delegate TPacket OutgoingArizonaPacketParseDelegate<TPacket>(OutgoingArizonaPacketArgs args) where TPacket : class, IParsedOutgoingPacket;

public abstract class IncomingPacketParserBase<TPacket> : IIncomingPacketParser
    where TPacket : class, IParsedIncomingPacket
{
    public abstract PacketId PacketId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(IncomingPacketArgs args, out PacketParseResult result)
    {
        if (args.DataBitLength < MinimumBitLength)
        {
            result = PacketParseResult.TooShort(Name);
            return false;
        }

        if (ExactBitLength.HasValue && args.DataBitLength != ExactBitLength.Value)
        {
            result = PacketParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TPacket packet = Parse(args);
            result = new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Packet parse exception parser={Name} packetId={PacketId} bits={args.DataBitLength}: {ex}");
            result = PacketParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TPacket Parse(IncomingPacketArgs args);
}

public abstract class OutgoingPacketParserBase<TPacket> : IOutgoingPacketParser
    where TPacket : class, IParsedOutgoingPacket
{
    public abstract PacketId PacketId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(OutgoingPacketArgs args, out PacketParseResult result)
    {
        if (args.DataBitLength < MinimumBitLength)
        {
            result = PacketParseResult.TooShort(Name);
            return false;
        }

        if (ExactBitLength.HasValue && args.DataBitLength != ExactBitLength.Value)
        {
            result = PacketParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TPacket packet = Parse(args);
            result = new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Packet parse exception parser={Name} packetId={PacketId} bits={args.DataBitLength}: {ex}");
            result = PacketParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TPacket Parse(OutgoingPacketArgs args);
}

public abstract class IncomingArizonaPacketParserBase<TPacket> : IIncomingArizonaPacketParser
    where TPacket : class, IParsedIncomingPacket
{
    public abstract PacketId PacketId { get; }
    public abstract int SubId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumPayloadBitLength => 0;
    protected virtual int? ExactPayloadBitLength => null;

    public bool TryParse(IncomingArizonaPacketArgs args, out PacketParseResult result)
    {
        if (args.PayloadBitLength < MinimumPayloadBitLength)
        {
            result = PacketParseResult.TooShort(Name);
            return false;
        }

        if (ExactPayloadBitLength.HasValue && args.PayloadBitLength != ExactPayloadBitLength.Value)
        {
            result = PacketParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TPacket packet = Parse(args);
            result = new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Arizona packet parse exception parser={Name} packetId={PacketId} subId={SubId} bits={args.PayloadBitLength}: {ex}");
            result = PacketParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TPacket Parse(IncomingArizonaPacketArgs args);
}

public abstract class OutgoingArizonaPacketParserBase<TPacket> : IOutgoingArizonaPacketParser
    where TPacket : class, IParsedOutgoingPacket
{
    public abstract PacketId PacketId { get; }
    public abstract int SubId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TPacket);
    protected virtual int MinimumPayloadBitLength => 0;
    protected virtual int? ExactPayloadBitLength => null;

    public bool TryParse(OutgoingArizonaPacketArgs args, out PacketParseResult result)
    {
        if (args.PayloadBitLength < MinimumPayloadBitLength)
        {
            result = PacketParseResult.TooShort(Name);
            return false;
        }

        if (ExactPayloadBitLength.HasValue && args.PayloadBitLength != ExactPayloadBitLength.Value)
        {
            result = PacketParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TPacket packet = Parse(args);
            result = new PacketParseResult(true, packet, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"Arizona packet parse exception parser={Name} packetId={PacketId} subId={SubId} bits={args.PayloadBitLength}: {ex}");
            result = PacketParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TPacket Parse(OutgoingArizonaPacketArgs args);
}

public sealed class DelegateIncomingPacketParser<TPacket> : IncomingPacketParserBase<TPacket>
    where TPacket : class, IParsedIncomingPacket
{
    private readonly PacketId _packetId;
    private readonly string _name;
    private readonly IncomingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateIncomingPacketParser(PacketId packetId, IncomingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}IncomingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override PacketId PacketId => _packetId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TPacket Parse(IncomingPacketArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateOutgoingPacketParser<TPacket> : OutgoingPacketParserBase<TPacket>
    where TPacket : class, IParsedOutgoingPacket
{
    private readonly PacketId _packetId;
    private readonly string _name;
    private readonly OutgoingPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateOutgoingPacketParser(PacketId packetId, OutgoingPacketParseDelegate<TPacket> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _packetId = packetId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}OutgoingParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override PacketId PacketId => _packetId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TPacket Parse(OutgoingPacketArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateIncomingArizonaPacketParser<TPacket> : IncomingArizonaPacketParserBase<TPacket>
    where TPacket : class, IParsedIncomingPacket
{
    private readonly PacketId _packetId;
    private readonly int _subId;
    private readonly string _name;
    private readonly IncomingArizonaPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumPayloadBitLength;
    private readonly int? _exactPayloadBitLength;

    public DelegateIncomingArizonaPacketParser(PacketId packetId, int subId, IncomingArizonaPacketParseDelegate<TPacket> parser, string? name = null, int minimumPayloadBitLength = 0, int? exactPayloadBitLength = null)
    {
        _packetId = packetId;
        _subId = subId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}:{subId}IncomingParser" : name;
        _minimumPayloadBitLength = minimumPayloadBitLength;
        _exactPayloadBitLength = exactPayloadBitLength;
    }

    public override PacketId PacketId => _packetId;
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
    where TPacket : class, IParsedOutgoingPacket
{
    private readonly PacketId _packetId;
    private readonly int _subId;
    private readonly string _name;
    private readonly OutgoingArizonaPacketParseDelegate<TPacket> _parser;
    private readonly int _minimumPayloadBitLength;
    private readonly int? _exactPayloadBitLength;

    public DelegateOutgoingArizonaPacketParser(PacketId packetId, int subId, OutgoingArizonaPacketParseDelegate<TPacket> parser, string? name = null, int minimumPayloadBitLength = 0, int? exactPayloadBitLength = null)
    {
        _packetId = packetId;
        _subId = subId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{packetId}:{subId}OutgoingParser" : name;
        _minimumPayloadBitLength = minimumPayloadBitLength;
        _exactPayloadBitLength = exactPayloadBitLength;
    }

    public override PacketId PacketId => _packetId;
    public override int SubId => _subId;
    public override string Name => _name;
    protected override int MinimumPayloadBitLength => _minimumPayloadBitLength;
    protected override int? ExactPayloadBitLength => _exactPayloadBitLength;

    protected override TPacket Parse(OutgoingArizonaPacketArgs args)
    {
        return _parser(args);
    }
}




