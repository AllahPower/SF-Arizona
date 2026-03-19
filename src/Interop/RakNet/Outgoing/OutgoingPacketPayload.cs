using System.Runtime.InteropServices;

namespace SFSharp;

public readonly record struct OutgoingPacketPayload(PacketId PacketId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<OutgoingPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingPacketArgs args = new((int)PacketId, (nint)dataPtr, DataBitLength);
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
                OutgoingPacketArgs args = new((int)PacketId, (nint)dataPtr, DataBitLength);
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

        return new OutgoingPacketPayload((PacketId)args.PacketId, data, args.DataBitLength);
    }
}
