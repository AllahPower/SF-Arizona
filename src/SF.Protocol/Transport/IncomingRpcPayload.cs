using System.Runtime.InteropServices;

namespace SFSharp.Protocol.Transport;

public readonly record struct IncomingRpcPayload(SampRpcId SampRpcId, byte[] Data, int DataBitOffset, int DataBitLength)
{
    public T Parse<T>(Func<IncomingRpcArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingRpcArgs args = new((int)SampRpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
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
                IncomingRpcArgs args = new((int)SampRpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
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

        return new IncomingRpcPayload((SampRpcId)args.SampRpcId, data, args.DataBitOffset, args.DataBitLength);
    }
}
