using System.Runtime.InteropServices;

namespace SFSharp;

public readonly record struct IncomingArizonaPacketPayload(EPacketId EPacketId, int SubId, byte[] Data, int PayloadBitOffset, int PayloadBitLength)
{
    public T Parse<T>(Func<IncomingArizonaPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                IncomingArizonaPacketArgs args = new((int)EPacketId, SubId, (nint)dataPtr, PayloadBitOffset, PayloadBitLength);
                return parser(args);
            }
        }
    }

    public static IncomingArizonaPacketPayload From(IncomingArizonaPacketArgs args)
    {
        int byteLength = (args.PayloadBitOffset + args.PayloadBitLength + 7) / 8;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new IncomingArizonaPacketPayload((EPacketId)args.EPacketId, args.SubId, data, args.PayloadBitOffset, args.PayloadBitLength);
    }
}

public readonly record struct OutgoingArizonaPacketPayload(EPacketId EPacketId, int SubId, byte[] Data, int PayloadBitOffset, int PayloadBitLength)
{
    public T Parse<T>(Func<OutgoingArizonaPacketArgs, T> parser)
    {
        unsafe
        {
            fixed (byte* dataPtr = Data)
            {
                OutgoingArizonaPacketArgs args = new((int)EPacketId, SubId, (nint)dataPtr, PayloadBitOffset, PayloadBitLength);
                return parser(args);
            }
        }
    }

    public static OutgoingArizonaPacketPayload From(OutgoingArizonaPacketArgs args)
    {
        int byteLength = (args.PayloadBitOffset + args.PayloadBitLength + 7) / 8;
        byte[] data = new byte[byteLength];
        if (byteLength > 0)
        {
            Marshal.Copy(args.DataPtr, data, 0, byteLength);
        }

        return new OutgoingArizonaPacketPayload((EPacketId)args.EPacketId, args.SubId, data, args.PayloadBitOffset, args.PayloadBitLength);
    }
}
