namespace SFSharp.Runtime.Network.RakNet.Rpc;

public delegate TRpc IncomingRpcParseDelegate<TRpc>(IncomingRpcArgs args) where TRpc : IParsedIncomingRpc;
public delegate TRpc OutgoingRpcParseDelegate<TRpc>(OutgoingRpcArgs args) where TRpc : IParsedOutgoingRpc;

public abstract class IncomingRpcParserBase<TRpc> : IIncomingRpcParser
    where TRpc : IParsedIncomingRpc
{
    public abstract ERpcId ERpcId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TRpc);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(IncomingRpcArgs args, out RpcParseResult result)
    {
        if (args.DataBitLength < MinimumBitLength)
        {
            result = RpcParseResult.TooShort(Name);
            return false;
        }

        if (ExactBitLength.HasValue && args.DataBitLength != ExactBitLength.Value)
        {
            result = RpcParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TRpc rpc = Parse(args);
            result = new RpcParseResult(true, rpc, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"RPC parse exception parser={Name} rpcId={ERpcId} bits={args.DataBitLength}: {ex}");
            result = RpcParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TRpc Parse(IncomingRpcArgs args);
}

public abstract class OutgoingRpcParserBase<TRpc> : IOutgoingRpcParser
    where TRpc : IParsedOutgoingRpc
{
    public abstract ERpcId ERpcId { get; }
    public virtual string Name => GetType().Name;
    public Type ParsedType => typeof(TRpc);
    protected virtual int MinimumBitLength => 0;
    protected virtual int? ExactBitLength => null;

    public bool TryParse(OutgoingRpcArgs args, out RpcParseResult result)
    {
        if (args.DataBitLength < MinimumBitLength)
        {
            result = RpcParseResult.TooShort(Name);
            return false;
        }

        if (ExactBitLength.HasValue && args.DataBitLength != ExactBitLength.Value)
        {
            result = RpcParseResult.SizeMismatch(Name);
            return false;
        }

        try
        {
            TRpc rpc = Parse(args);
            result = new RpcParseResult(true, rpc, Name, PacketParseFailureReason.None);
            return true;
        }
        catch (Exception ex)
        {
            SFLog.Error($"RPC parse exception parser={Name} rpcId={ERpcId} bits={args.DataBitLength}: {ex}");
            result = RpcParseResult.FromException(Name, ex);
            return false;
        }
    }

    protected abstract TRpc Parse(OutgoingRpcArgs args);
}

public sealed class DelegateIncomingRpcParser<TRpc> : IncomingRpcParserBase<TRpc>
    where TRpc : IParsedIncomingRpc
{
    private readonly ERpcId _rpcId;
    private readonly string _name;
    private readonly IncomingRpcParseDelegate<TRpc> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateIncomingRpcParser(ERpcId rpcId, IncomingRpcParseDelegate<TRpc> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _rpcId = rpcId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{rpcId}IncomingRpcParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override ERpcId ERpcId => _rpcId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TRpc Parse(IncomingRpcArgs args)
    {
        return _parser(args);
    }
}

public sealed class DelegateOutgoingRpcParser<TRpc> : OutgoingRpcParserBase<TRpc>
    where TRpc : IParsedOutgoingRpc
{
    private readonly ERpcId _rpcId;
    private readonly string _name;
    private readonly OutgoingRpcParseDelegate<TRpc> _parser;
    private readonly int _minimumBitLength;
    private readonly int? _exactBitLength;

    public DelegateOutgoingRpcParser(ERpcId rpcId, OutgoingRpcParseDelegate<TRpc> parser, string? name = null, int minimumBitLength = 0, int? exactBitLength = null)
    {
        _rpcId = rpcId;
        _parser = parser;
        _name = string.IsNullOrWhiteSpace(name) ? $"{rpcId}OutgoingRpcParser" : name;
        _minimumBitLength = minimumBitLength;
        _exactBitLength = exactBitLength;
    }

    public override ERpcId ERpcId => _rpcId;
    public override string Name => _name;
    protected override int MinimumBitLength => _minimumBitLength;
    protected override int? ExactBitLength => _exactBitLength;

    protected override TRpc Parse(OutgoingRpcArgs args)
    {
        return _parser(args);
    }
}
