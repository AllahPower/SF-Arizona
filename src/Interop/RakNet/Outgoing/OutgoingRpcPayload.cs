using SFSharp.Interop.RakNet.Packets.Enum;
using System.Runtime.InteropServices;

namespace SFSharp;

public readonly record struct OutgoingRpcPayload(ERpcId ERpcId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<OutgoingRpcArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingRpcArgs args = new((int)ERpcId, (nint)dataPtr, DataBitLength);
                return parser(args);
            }
        }
    }

    public void Use(Action<OutgoingRpcArgs> action)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingRpcArgs args = new((int)ERpcId, (nint)dataPtr, DataBitLength);
                action(args);
            }
        }
    }

    public static OutgoingRpcPayload From(OutgoingRpcArgs args)
    {
        int byteLength = (args.DataBitLength + 7) / 8;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new OutgoingRpcPayload((ERpcId)args.ERpcId, data, args.DataBitLength);
    }
}
