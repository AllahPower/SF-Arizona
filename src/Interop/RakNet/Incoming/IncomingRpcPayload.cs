using SFSharp.Abstractions.Interop.RakNet;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Incoming;

public readonly record struct IncomingRpcPayload(ERpcId ERpcId, byte[] Data, int DataBitOffset, int DataBitLength)
{
    public T Parse<T>(Func<IncomingRpcArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingRpcArgs args = new((int)ERpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
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
                IncomingRpcArgs args = new((int)ERpcId, (nint)dataPtr, DataBitOffset, DataBitLength);
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

        return new IncomingRpcPayload((ERpcId)args.ERpcId, data, args.DataBitOffset, args.DataBitLength);
    }
}
