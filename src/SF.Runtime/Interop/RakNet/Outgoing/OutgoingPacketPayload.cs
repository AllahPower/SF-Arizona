using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.RakNet.Outgoing;

public readonly record struct OutgoingPacketPayload(RakNetPacketId RakNetPacketId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<OutgoingPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingPacketArgs args = new((int)RakNetPacketId, (nint)dataPtr, DataBitLength);
                return parser(args);
            }
        }
    }

    public void Use(Action<OutgoingPacketArgs> action)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingPacketArgs args = new((int)RakNetPacketId, (nint)dataPtr, DataBitLength);
                action(args);
            }
        }
    }

    public static OutgoingPacketPayload From(OutgoingPacketArgs args)
    {
        int byteLength = args.DataByteLength;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new OutgoingPacketPayload((RakNetPacketId)args.RakNetPacketId, data, args.DataBitLength);
    }
}
