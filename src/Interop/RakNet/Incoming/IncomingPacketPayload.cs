using System.Runtime.InteropServices;

namespace SFSharp;

public readonly record struct IncomingPacketPayload(PacketId PacketId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<IncomingPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingPacketArgs args = new((int)PacketId, (nint)dataPtr, DataBitLength);
                return parser(args);
            }
        }
    }

    public void Use(Action<IncomingPacketArgs> action)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingPacketArgs args = new((int)PacketId, (nint)dataPtr, DataBitLength);
                action(args);
            }
        }
    }

    public static IncomingPacketPayload From(IncomingPacketArgs args)
    {
        int byteLength = args.DataByteLength;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new IncomingPacketPayload((PacketId)args.PacketId, data, args.DataBitLength);
    }
}
