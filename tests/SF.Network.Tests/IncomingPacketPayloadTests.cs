using SFSharp.Runtime.Interop.RakNet.Incoming;

namespace SF.Network.Tests;

public sealed class IncomingPacketPayloadTests
{
    [Fact]
    public unsafe void PayloadCopyOutlivesSourceAndKeepsRawReaderContract()
    {
        byte[] source = [40, 0x78, 0x56, 0x34, 0x12, 207, 0xAA, 0xBB];
        IncomingPacketPayload payload;
        fixed (byte* data = source)
        {
            payload = IncomingPacketPayload.From(new IncomingPacketArgs(40, (nint)data, 61));
        }
        IncomingPacketFrame frame = payload.ToFrame();
        Assert.NotSame(source, payload.Data);
        Assert.Equal(source, payload.Data);
        Assert.Equal(payload.Data.AsMemory(), frame.Data);
        Assert.Equal(61, frame.DataBitLength);
        payload.Use(args =>
        {
            var reader = args.CreateReader();
            Assert.Equal(40, args.EPacketId);
            Assert.Equal(40, reader.ReadUInt8());
            Assert.Equal(0x12345678u, reader.ReadUInt32());
        });
        source[1] ^= 0xFF;
        Assert.Equal(0x12345678u, frame.RakNetTimestamp);
    }
}
