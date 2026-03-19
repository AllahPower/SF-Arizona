using System.Runtime.InteropServices;

namespace SFSharp;

public readonly record struct IncomingRpcPayload(RpcId RpcId, byte[] Data, int DataBitOffset, int DataBitLength)
{
    public T Parse<T>(Func<IncomingRpcArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingRpcArgs args = new((int)RpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
                return parser(args);
            }
        }
    }

    public void Use(Action<IncomingRpcArgs> action)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingRpcArgs args = new((int)RpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
                action(args);
            }
        }
    }

    public static IncomingRpcPayload From(IncomingRpcArgs args)
    {
        int totalBitLength = args.DataBitOffset + args.DataBitLength;
        int byteLength = (totalBitLength + 7) / 8;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new IncomingRpcPayload((RpcId)args.RpcId, data, args.DataBitOffset, args.DataBitLength);
    }
}
