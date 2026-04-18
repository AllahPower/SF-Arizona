using SFSharp.Abstractions.Interop.RakNet;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Network.RakNet.Outgoing;

public readonly record struct OutgoingPacketPayload(EPacketId EPacketId, byte[] Data, int DataBitLength)
{
    public T Parse<T>(Func<OutgoingPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingPacketArgs args = new((int)EPacketId, (nint)dataPtr, DataBitLength);
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
                OutgoingPacketArgs args = new((int)EPacketId, (nint)dataPtr, DataBitLength);
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

        return new OutgoingPacketPayload((EPacketId)args.EPacketId, data, args.DataBitLength);
    }
}
