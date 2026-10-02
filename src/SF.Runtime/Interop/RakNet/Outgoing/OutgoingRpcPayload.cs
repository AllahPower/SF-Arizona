using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Outgoing;

public readonly record struct OutgoingRpcPayload(SampRpcId SampRpcId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<OutgoingRpcArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingRpcArgs args = new((int)SampRpcId, (nint)dataPtr, DataBitLength);
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
                OutgoingRpcArgs args = new((int)SampRpcId, (nint)dataPtr, DataBitLength);
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

        return new OutgoingRpcPayload((SampRpcId)args.SampRpcId, data, args.DataBitLength);
    }
}
