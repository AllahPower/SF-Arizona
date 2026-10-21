using System.Runtime.InteropServices;
using SFSharp.Abstractions.Samp;
using SFSharp.Runtime.Interop.Classes.Game;
using SFSharp.Runtime.Interop.Classes.Networking;
using SFSharp.Runtime.Interop.Classes.Players;
using SFSharp.Runtime.Interop.Classes.Pools;
using SFSharp.Runtime.Interop.Native;

namespace SF.Network.Tests;

public sealed unsafe class SampClientProfileTests
{
    [Theory]
    [InlineData(0xCC4D0u, true, false, SampVersion.R3_037)]
    [InlineData(0xCBC90u, false, true, SampVersion.R5_037)]
    [InlineData(0xCBC90u, true, false, SampVersion.Unknown)]
    [InlineData(0xCC4D0u, false, true, SampVersion.Unknown)]
    [InlineData(0xCC4D0u, true, true, SampVersion.Unknown)]
    [InlineData(0u, true, false, SampVersion.Unknown)]
    [InlineData(0x31DF13u, false, false, SampVersion.Unknown)]
    public void RecognitionRequiresMatchingEntryPointAndMarker(uint entry, bool r3, bool r5, SampVersion expected)
        => Assert.Equal(expected, SampEnvironment.Identify(entry, 0x400000, r3, r5));

    [Fact]
    public void TruncatedImageCannotSelectKnownProfile()
        => Assert.Equal(SampVersion.Unknown, SampEnvironment.Identify(0xCBC90, 0x100000, false, true));

    [Theory]
    [InlineData(1, SFSampGameState.WaitingToConnect)]
    [InlineData(2, SFSampGameState.Connecting)]
    [InlineData(5, SFSampGameState.Connected)]
    [InlineData(6, SFSampGameState.WaitingToJoin)]
    [InlineData(11, SFSampGameState.Restarting)]
    [InlineData(999, SFSampGameState.Unknown)]
    public void StateNormalizationIsIndependentOfClientRevision(int raw, SFSampGameState expected)
    {
        Assert.Equal(expected, SampClientProfile.R3.NormalizeState(raw));
        Assert.Equal(expected, SampClientProfile.R5.NormalizeState(raw));
    }

    [Fact]
    public void R5ViewsMatchVerifiedNativeLayouts()
    {
        Assert.Equal(804, sizeof(LocalPlayerR5));
        Assert.Equal(509, sizeof(RemotePlayerR5));
        Assert.Equal(12094, sizeof(PlayerPoolR5));
        Assert.Equal(48, sizeof(PlayerInfoR5));
        Assert.Equal(36, sizeof(NetGamePoolsR5));
        Assert.Equal(0x3D3, sizeof(MenuR5));
        Assert.Equal(0x104, Marshal.OffsetOf<LocalPlayerR5>(nameof(LocalPlayerR5.ped)).ToInt32());
        Assert.Equal(0x1DD, Marshal.OffsetOf<RemotePlayerR5>(nameof(RemotePlayerR5.ped)).ToInt32());
        Assert.Equal(0x17C, Marshal.OffsetOf<RemotePlayerR5>(nameof(RemotePlayerR5.onfootTargetPosition)).ToInt32());
        Assert.Equal(4, Marshal.OffsetOf<NetGamePoolsR5>(nameof(NetGamePoolsR5.Player)).ToInt32());
        Assert.Equal(25 * sizeof(nint), Marshal.OffsetOf<CRakClientInterfaceVTable>(nameof(CRakClientInterfaceVTable.RpcBitStream)).ToInt32());
    }

    [Fact]
    public void BorrowedR5ViewReadsAllocateNothing()
    {
        byte* bytes = stackalloc byte[509];
        new Span<byte>(bytes, 509).Clear();
        *(ushort*)(bytes + 485) = 73;
        var player = (RemotePlayerR5*)bytes;
        ReadRepeated(player, 10_000);
        long before = GC.GetAllocatedBytesForCurrentThread();
        long total = ReadRepeated(player, 1_000_000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
        Assert.Equal(73_000_000, total);
        Assert.Equal(0, allocated);
    }

    private static long ReadRepeated(RemotePlayerR5* player, int count)
    {
        long total = 0;
        for (int i = 0; i < count; i++) total += player->id;
        return total;
    }
}
