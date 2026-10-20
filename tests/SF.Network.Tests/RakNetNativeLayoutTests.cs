using System.Runtime.InteropServices;
using SFSharp.Runtime.Interop.Classes.Networking;
using SFSharp.Runtime.Interop.Offsets;

namespace SF.Network.Tests;

public sealed unsafe class RakNetNativeLayoutTests
{
    [Fact]
    public void NativeLayoutsMatchTheVerifiedR3Client()
    {
        Assert.Equal(0, Marshal.OffsetOf<CRakNetBitStream>(nameof(CRakNetBitStream.NumberOfBitsUsed)).ToInt32());
        Assert.Equal(4, Marshal.OffsetOf<CRakNetBitStream>(nameof(CRakNetBitStream.NumberOfBitsAllocated)).ToInt32());
        Assert.Equal(8, Marshal.OffsetOf<CRakNetBitStream>(nameof(CRakNetBitStream.ReadOffset)).ToInt32());
        Assert.Equal(12, Marshal.OffsetOf<CRakNetBitStream>(nameof(CRakNetBitStream.Data)).ToInt32());
        Assert.Equal(6 * sizeof(int), Marshal.OffsetOf<CRakClientInterfaceVTable>(nameof(CRakClientInterfaceVTable.SendBitStream)).ToInt32());
        Assert.Equal(7 * sizeof(int), Marshal.OffsetOf<CRakClientInterfaceVTable>(nameof(CRakClientInterfaceVTable.SendData)).ToInt32());
        Assert.Equal(SampOffsets.RakNetBitStream.NumberOfBitsUsed,
            Marshal.OffsetOf<CRakNetBitStream>(nameof(CRakNetBitStream.NumberOfBitsUsed)).ToInt32());
    }

    [Theory]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(15)]
    [InlineData(16)]
    public void HookReaderUsesValidBitsInsteadOfAllocatedCapacity(int usedBits)
    {
        byte* payload = stackalloc byte[] { 220, 18, 0xEE, 0xEE };
        byte* image = stackalloc byte[16];
        *(int*)(image + 0) = usedBits;
        *(int*)(image + 4) = 2048;
        *(int*)(image + 8) = 0;
        *(byte**)(image + 12) = payload;

        Assert.True(RakNetHookPayloadReader.TryReadPacketBitStream((CRakNetBitStream*)image, out int id, out int bits, out ReadOnlySpan<byte> data));
        Assert.Equal(220, id);
        Assert.Equal(usedBits, bits);
        Assert.Equal((usedBits + 7) / 8, data.Length);
        Assert.Equal(new ReadOnlySpan<byte>(payload, data.Length).ToArray(), data.ToArray());

        int rpcId = 93;
        Assert.True(RakNetHookPayloadReader.TryReadOutgoingRpcBitStream(&rpcId, (CRakNetBitStream*)image, out int rpc, out int rpcBits, out ReadOnlySpan<byte> rpcData));
        Assert.Equal(93, rpc);
        Assert.Equal(usedBits, rpcBits);
        Assert.Equal(data.ToArray(), rpcData.ToArray());
    }
}
