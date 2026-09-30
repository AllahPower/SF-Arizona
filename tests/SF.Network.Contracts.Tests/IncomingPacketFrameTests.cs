using System.Buffers.Binary;
using System.Reflection;

namespace SF.Network.Contracts.Tests;

public sealed class IncomingPacketFrameTests
{
    public static IEnumerable<object[]> TimestampCases()
    {
        foreach (uint timestamp in new uint[] { 0, 1, 0x12345678, 0x80000000, uint.MaxValue })
        {
            foreach (byte packetId in new byte[] { 200, 207, 203, 220, 40 })
            {
                yield return [timestamp, packetId];
            }
        }
    }

    [Theory]
    [MemberData(nameof(TimestampCases))]
    public void CompleteEnvelopePreservesRawFrameAndExposesInnerBitWindow(uint timestamp, byte packetId)
    {
        byte[] bytes = [40, 0, 0, 0, 0, packetId, 0x12, 0x34];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(1), timestamp);
        IncomingPacketFrame frame = new(40, bytes, 61);

        Assert.True(frame.TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope));
        Assert.Equal(new RakNetTimestampEnvelope(timestamp, packetId), envelope);
        Assert.Equal(timestamp, frame.RakNetTimestamp);
        Assert.Equal(packetId, frame.EffectivePacketId);
        Assert.Equal(40, frame.PacketDataBitOffset);
        Assert.Equal(21, frame.PacketDataBitLength);
        Assert.Equal(frame.Data[5..], frame.PacketData);
        Assert.Equal(packetId, frame.PacketData.Span[0]);
        (int rawId, ReadOnlyMemory<byte> rawData, int rawBits) = frame;
        Assert.Equal(40, rawId);
        Assert.Equal(bytes.AsMemory(), rawData);
        Assert.Equal(61, rawBits);
        Assert.Equal(new IncomingPacketFrame(40, bytes, 61), frame);
    }

    [Fact]
    public void FixedWireFixtureUsesLittleEndianTimestamp()
    {
        // Independent fixture: no writer shared with the production decoder.
        byte[] bytes = [0x28, 0x78, 0x56, 0x34, 0x12, 0xCF];
        Assert.True(RakNetTimestampEnvelope.TryRead(bytes, 48, out RakNetTimestampEnvelope envelope));
        Assert.Equal(0x12345678u, envelope.Timestamp);
        Assert.Equal(207, envelope.PacketId);
    }

    public static IEnumerable<object[]> InvalidEnvelopeCases()
    {
        byte[] header = [40, 1, 2, 3, 4, 207];
        for (int length = 0; length < header.Length; length++)
        {
            yield return [new IncomingPacketFrame(40, header[..length], length * 8)];
        }
        for (int bits = 0; bits < 48; bits++)
        {
            yield return [new IncomingPacketFrame(40, header, bits)];
        }
        foreach (int bits in new[] { 49, int.MaxValue, -1 })
        {
            yield return [new IncomingPacketFrame(40, header, bits)];
        }
        yield return [new IncomingPacketFrame(207, header, 48)];
        yield return [new IncomingPacketFrame(40, new byte[] { 207, 1, 2, 3, 4, 207 }, 48)];
        yield return [new IncomingPacketFrame(207, new byte[] { 207, 1, 2, 3 }, 32)];
        yield return [default(IncomingPacketFrame)];
    }

    [Theory]
    [MemberData(nameof(InvalidEnvelopeCases))]
    public void InvalidOrUnwrappedFrameRemainsRaw(IncomingPacketFrame frame)
    {
        Assert.False(frame.TryGetTimestampEnvelope(out RakNetTimestampEnvelope envelope));
        Assert.Equal(default, envelope);
        Assert.Null(frame.RakNetTimestamp);
        Assert.Equal(frame.PacketId, frame.EffectivePacketId);
        Assert.Equal(0, frame.PacketDataBitOffset);
        Assert.Equal(frame.Data, frame.PacketData);
        Assert.Equal(frame.DataBitLength, frame.PacketDataBitLength);
    }

    [Fact]
    public void FrameKeepsItsOriginalThreeInstanceFields()
    {
        Assert.Equal(3, typeof(IncomingPacketFrame).GetFields(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Length);
    }
}
