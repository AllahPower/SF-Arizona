using System.Text;
using SFSharp.Runtime.Networking.RakNet.Arizona;
using SFSharp.Runtime.Networking.RakNet.Arizona.Models;
using SFSharp.Runtime.Interop.RakNet;

namespace SF.Network.Tests;

public sealed class ArizonaChat210Tests
{
    [Fact]
    public unsafe void IncomingChatParsesTextSlotAndModifiedItemFields()
    {
        byte[] data =
        [
            0x44, 0x33, 0x22, 0x11, 7, 3,
            0, 2, 0, 0xCF, 0xF0,
            1, 2, 0x34, 0x12,
            2, 0x56, 0x04, 3,
            0, 5, 0, 0, 0,
            14, 3, 0xE8, 0xF2, 0xE5,
            200, 0xDD, 0xCC, 0xBB, 0xAA
        ];
        fixed (byte* bytes = data)
        {
            SampBitStreamReader reader = new(bytes, 0, data.Length * 8);
            ArzChatMessageRelay chat = ArizonaPacket.ParseChatMessageRelay(ref reader);
            Assert.Equal((byte)7, chat.SenderSlot);
            Assert.Equal(0xFF112233u, chat.ArgbColor);
            Assert.Equal(data[5..], chat.RawPayload);
            Assert.Equal("Пр", Assert.IsType<ArzChatTextSegment>(chat.Segments[0]).Text);
            Assert.Equal(new ArzChatSlotSegment(2, 0x1234), chat.Segments[1]);
            ArzChatItemSegment item = Assert.IsType<ArzChatItemSegment>(chat.Segments[2]);
            Assert.Equal((ushort)0x0456, item.ItemId);
            Assert.Equal(new ArzChatItemField(0, 5, null), item.Fields[0]);
            Assert.Equal(new ArzChatItemField(14, null, "ите"), item.Fields[1]);
            Assert.Equal(new ArzChatItemField(200, 0xAABBCCDD, null), item.Fields[2]);
            Assert.Equal(0, reader.RemainingBits);
        }
    }

    [Fact]
    public unsafe void OutgoingLinkedChatHasSeparateBodyFormat()
    {
        byte[] data = [0, 4, 3, 3, 0, (byte)'/', (byte)'b', (byte)' ', 0, 2, 0, (byte)'h', (byte)'i', 1, 2, 0x34, 0x12, 2, 0x56, 0x04];
        fixed (byte* bytes = data)
        {
            SampBitStreamReader reader = new(bytes, 0, data.Length * 8);
            ArzLinkedChatSend chat = ArizonaPacket.ParseLinkedChatSend(ref reader);
            Assert.Equal((byte)0, chat.Reserved);
            Assert.Equal("/b ", Assert.IsType<ArzLinkedChatCommandSegment>(chat.Segments[0]).Command);
            Assert.Equal("hi", Assert.IsType<ArzLinkedChatTextSegment>(chat.Segments[1]).Text);
            Assert.Equal(new ArzLinkedChatSlotSegment(2, 0x1234), chat.Segments[2]);
            Assert.Equal(new ArzLinkedChatItemSegment(0x0456), chat.Segments[3]);
            Assert.Equal(0, reader.RemainingBits);
        }
    }

    [Fact]
    public unsafe void IncomingChatStopsAtUnknownSegmentKindLikeTheClient()
    {
        byte[] data = [0, 0, 0, 0, 0, 2, 0, 1, 0, (byte)'A', 3, 0xFF];
        fixed (byte* bytes = data)
        {
            SampBitStreamReader reader = new(bytes, 0, data.Length * 8);
            ArzChatMessageRelay chat = ArizonaPacket.ParseChatMessageRelay(ref reader);
            Assert.Single(chat.Segments);
            Assert.Equal((byte)3, chat.UnsupportedSegmentKind);
            Assert.Equal(data[5..], chat.RawPayload);
        }
    }

    [Fact]
    public unsafe void IncomingChatCanUseCp1252()
    {
        byte[] data = [0, 0, 0, 0, 0, 1, 0, 1, 0, 0xE9];
        fixed (byte* bytes = data)
        {
            SampBitStreamReader reader = new(bytes, 0, data.Length * 8);
            ArzChatMessageRelay chat = ArizonaPacket.ParseChatMessageRelay(ref reader, Encoding.GetEncoding(1252));
            Assert.Equal("é", Assert.IsType<ArzChatTextSegment>(chat.Segments[0]).Text);
        }
    }
}
