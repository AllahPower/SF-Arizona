using SFSharp.Abstractions.Interop.RakNet;

namespace SFSharp.Runtime.Networking;

public readonly record struct RpcParseResult(
    bool Success,
    IParsedRpc? Rpc,
    string ParserName,
    PacketParseFailureReason FailureReason,
    string? Error = null)
{
    public bool TryGet<TRpc>(out TRpc rpc) where TRpc : IParsedRpc
    {
        if (Success && Rpc is TRpc typed)
        {
            rpc = typed;
            return true;
        }

        rpc = default!;
        return false;
    }

    public static RpcParseResult Unsupported(ERpcId rpcId)
    {
        return new(false, null, rpcId.ToString(), PacketParseFailureReason.Unsupported);
    }

    public static RpcParseResult TooShort(string parserName)
    {
        return new(false, null, parserName, PacketParseFailureReason.TooShort);
    }

    public static RpcParseResult SizeMismatch(string parserName)
    {
        return new(false, null, parserName, PacketParseFailureReason.SizeMismatch);
    }

    public static RpcParseResult FromException(string parserName, Exception ex)
    {
        return new(false, null, parserName, PacketParseFailureReason.Exception, ex.Message);
    }
}
