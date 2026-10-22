using System.Numerics;
using System.Runtime.InteropServices;
using SFSharp.Abstractions.Samp;
using SFSharp.Runtime.Interop.Classes.Game;
using SFSharp.Runtime.Interop.Classes.Players;
using SFSharp.Runtime.Interop.Native;
using SFSharp.Runtime.Interop.Offsets;
using SFSharp.Runtime.Interop.Hooking.Hooks;
using SFSharp.Runtime.Interop.Classes.Vehicles;

namespace SF.Network.Tests;

public sealed unsafe class SampR1ProfileTests
{
    [Theory]
    [InlineData(0x31DF13u, "0.3.7", SampVersion.R1_037)]
    [InlineData(0x31DF13u, "0.3.7-R1", SampVersion.Unknown)]
    [InlineData(0x31DF13u, "0.3.7-R2", SampVersion.Unknown)]
    [InlineData(0x31DF13u, "0.3.7-extra", SampVersion.Unknown)]
    [InlineData(0x31DF13u, "", SampVersion.Unknown)]
    [InlineData(0xCC4D0u, "0.3.7", SampVersion.Unknown)]
    public void ClassificationReadsTheExactR1MarkerFromTheImage(uint entryPoint, string marker, SampVersion expected)
    {
        // Clean R1 (MD5 1d22eaa2605717ddf215f68e861de378) stores "0.3.7\0" at RVA 0xD3B84.
        byte[] image = new byte[0x330000];
        System.Text.Encoding.ASCII.GetBytes(marker).CopyTo(image, 0xD3B84);

        fixed (byte* moduleBase = image)
        {
            Assert.Equal(expected, SampEnvironment.ClassifySampVersion((uint)moduleBase, entryPoint, (uint)image.Length));
        }
    }

    [Fact]
    public void R1SpawnDelegateCallsSixSlotNativeThiscall()
    {
        // XOR ECX and each stack slot, then ret 18h. This exercises the actual x86 call boundary.
        ReadOnlySpan<byte> code = [0x8B, 0xC1,
            0x33, 0x44, 0x24, 0x04, 0x33, 0x44, 0x24, 0x08,
            0x33, 0x44, 0x24, 0x0C, 0x33, 0x44, 0x24, 0x10,
            0x33, 0x44, 0x24, 0x14, 0x33, 0x44, 0x24, 0x18,
            0xC2, 0x18, 0x00];
        uint memory = Win32.VirtualAlloc(0, (uint)code.Length,
            MemoryAllocationType.COMMIT | MemoryAllocationType.RESERVE, MemoryProtection.READWRITE);
        Assert.NotEqual(0u, memory);
        try
        {
            code.CopyTo(new Span<byte>((void*)memory, code.Length));
            Assert.True(Win32.VirtualProtect(memory, (uint)code.Length, MemoryProtection.EXECUTE_READ, out _));
            Assert.True(Win32.FlushInstructionCache(-1, memory, (uint)code.Length));
            var spawn = Marshal.GetDelegateForFunctionPointer<RemotePlayerSpawnR1Hook.Native>((nint)memory);
            int expected = 0x1110 ^ -7 ^ 200 ^ 0x123400 ^ BitConverter.SingleToInt32Bits(1.25f) ^ unchecked((int)0xFF112233) ^ 4;

            for (int i = 0; i < 1_000; i++)
            {
                Assert.Equal(expected, spawn(0x1110, -7, 200, 0x123400, 1.25f, 0xFF112233, 4));
            }
        }
        finally
        {
            Win32.VirtualFree(memory, 0, MemoryAllocationType.RELEASE);
        }
    }

    [Fact]
    public void SpawnHooksKeepRevisionSpecificStackSignatures()
    {
        var r1 = typeof(RemotePlayerSpawnR1Hook.Native).GetMethod("Invoke")!;
        var r3 = typeof(RemotePlayerSpawnHook.Native).GetMethod("Invoke")!;

        Assert.Equal([typeof(nint), typeof(int), typeof(int), typeof(nint), typeof(float), typeof(uint), typeof(int)],
            r1.GetParameters().Select(parameter => parameter.ParameterType).ToArray());
        Assert.Equal(8, r3.GetParameters().Length);
        Assert.Equal(typeof(int), r1.ReturnType);
        var abi = (UnmanagedFunctionPointerAttribute)Attribute.GetCustomAttribute(
            typeof(RemotePlayerSpawnR1Hook.Native), typeof(UnmanagedFunctionPointerAttribute))!;
        Assert.Equal(CallingConvention.ThisCall, abi.CallingConvention);
    }

    [Fact]
    public void VehiclePrefixReadsR1LightsWithoutUsingTheR5Field()
    {
        byte* bytes = stackalloc byte[0x80];
        new Span<byte>(bytes, 0x80).Clear();
        *(int*)(bytes + 0x5C) = 1;
        *(int*)(bytes + 0x54) = 0;

        VehicleR1* vehicle = (VehicleR1*)bytes;

        Assert.Equal(0x60, sizeof(VehicleR1));
        Assert.Equal(1, vehicle->LightsOn);
        Assert.Equal(0x5C, Marshal.OffsetOf<VehicleR1>(nameof(VehicleR1.LightsOn)).ToInt32());
    }

    [Theory]
    [InlineData(0x31DF13u, 0x400000u, false, false, true, SampVersion.R1_037)]
    [InlineData(0x31DF13u, 0x400000u, false, false, false, SampVersion.Unknown)]
    [InlineData(0x31DF13u, 0x400000u, true, false, true, SampVersion.Unknown)]
    [InlineData(0x31DF13u, 0x400000u, false, true, true, SampVersion.Unknown)]
    [InlineData(0x31DF13u, 0x300000u, false, false, true, SampVersion.Unknown)]
    [InlineData(0xCC4D0u, 0x400000u, true, false, true, SampVersion.Unknown)]
    [InlineData(0xCBC90u, 0x400000u, false, true, true, SampVersion.Unknown)]
    [InlineData(0u, 0x400000u, false, false, true, SampVersion.Unknown)]
    public void RecognitionRejectsConflictingMarkersAndTruncatedImages(
        uint entryPoint, uint imageSize, bool r3, bool r5, bool r1, SampVersion expected)
    {
        SampVersion actual = SampEnvironment.Identify(entryPoint, imageSize, r3, r5, r1);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(9, SFSampGameState.WaitingToConnect)]
    [InlineData(13, SFSampGameState.Connecting)]
    [InlineData(14, SFSampGameState.Connected)]
    [InlineData(15, SFSampGameState.WaitingToJoin)]
    [InlineData(18, SFSampGameState.Restarting)]
    [InlineData(5, SFSampGameState.Unknown)]
    public void StateNormalizationUsesR1Values(int native, SFSampGameState expected)
        => Assert.Equal(expected, SampClientProfile.R1.NormalizeState(native));

    [Fact]
    public void ProfileContainsVerifiedNetGameFields()
    {
        SampClientProfile profile = Assert.IsType<SampClientProfile>(SampClientProfile.FromEntryPoint(0x31DF13));

        Assert.Equal(SFSampVersion.R1, profile.Version);
        Assert.Equal(0x21A0F8u, profile.NetGameRva);
        Assert.Equal(0x3C9, profile.RakClientOffset);
        Assert.Equal(0x3CD, profile.PoolsOffset);
        Assert.Equal(0x3BD, profile.StateOffset);
        Assert.True(profile.IsRuntimeSupported);
    }

    [Theory]
    [InlineData(0x31DF13u)]
    [InlineData(0xCC4D0u)]
    [InlineData(0xCBC90u)]
    public void ReadinessUsesActualPointersRatherThanTheFirstDword(uint entryPoint)
    {
        SampClientProfile profile = SampClientProfile.FromEntryPoint(entryPoint)!;
        Span<byte> memory = stackalloc byte[profile.NetGameReadinessSize];
        memory.Clear();
        nint client = 0x123400;
        nint pools = 0x567800;

        Assert.False(profile.IsNetGameReady(memory));
        MemoryMarshal.Write(memory[profile.RakClientOffset..], in client);
        Assert.False(profile.IsNetGameReady(memory));
        MemoryMarshal.Write(memory[profile.PoolsOffset..], in pools);

        Assert.True(profile.IsNetGameReady(memory));
        Assert.False(profile.IsNetGameReady(memory[..^1]));
    }

    [Theory]
    [InlineData(SFSampVersion.R1, 0x307F0)]
    [InlineData(SFSampVersion.R3_1, 0x33BA0)]
    [InlineData(SFSampVersion.R5_1, 0x342E0)]
    public void ResolverSelectsOnlyTheRequestedRevision(SFSampVersion version, int expected)
        => Assert.Equal(expected, SampOffsets.Resolve(version, 0x307F0, 0x33BA0, 0x342E0));

    [Theory]
    [InlineData(SFSampVersion.Unknown)]
    [InlineData((SFSampVersion)999)]
    public void UnknownRevisionHasNoAddressFallback(SFSampVersion version)
        => Assert.Throws<NotSupportedException>(() => SampOffsets.Resolve(version, 1, 2, 3));

    [Theory]
    [InlineData(SampVersion.R1_037, 0x31DF13u, 0x400000u, SFSampVersion.R1)]
    [InlineData(SampVersion.R3_037, 0xCC4D0u, 0x400000u, SFSampVersion.R3_1)]
    [InlineData(SampVersion.R5_037, 0xCBC90u, 0x400000u, SFSampVersion.R5_1)]
    [InlineData(SampVersion.R3_037, 0x31DF13u, 0x400000u, SFSampVersion.Unknown)]
    [InlineData(SampVersion.Unknown, 0xCC4D0u, 0x400000u, SFSampVersion.Unknown)]
    [InlineData(SampVersion.R1_037, 0x31DF13u, 0x300000u, SFSampVersion.Unknown)]
    public void PublishedCapabilitiesMatchValidatedIdentity(
        SampVersion version, uint entryPoint, uint imageSize, SFSampVersion expected)
    {
        SampVersionInfo info = new(version, 0x10000000, entryPoint, imageSize, 0, 0);

        SampEnvironment.DetectionSnapshot snapshot = SampEnvironment.DetectionSnapshot.Create(info);

        Assert.Same(info, snapshot.Info);
        Assert.Equal(expected, snapshot.Client.Version);
        Assert.Equal(expected != SFSampVersion.Unknown, info.IsSupported);
        Assert.Equal(expected == SFSampVersion.Unknown, snapshot.Client.Capabilities == SFSampCapabilities.None);
    }

    [Fact]
    public void RepeatedProfileValidationAllocatesNothing()
    {
        SampVersionInfo info = new(SampVersion.R1_037, 0x10000000, 0x31DF13, 0x400000, 0, 0);
        ReadProfileRepeated(info, 10_000);
        long before = GC.GetAllocatedBytesForCurrentThread();

        int supported = ReadProfileRepeated(info, 1_000_000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(1_000_000, supported);
        Assert.Equal(0, allocated);
    }

    private static int ReadProfileRepeated(SampVersionInfo info, int count)
    {
        int supported = 0;
        for (int i = 0; i < count; i++)
        {
            if (info.IsSupported) supported++;
        }
        return supported;
    }

    [Fact]
    public void ViewsMatchPackedR1Structures()
    {
        Assert.Equal(804, sizeof(LocalPlayerR1));
        Assert.Equal(509, sizeof(RemotePlayerR1));
        Assert.Equal(12094, sizeof(PlayerPoolR1));
        Assert.Equal(44, sizeof(PlayerInfoR1));
        Assert.Equal(36, sizeof(NetGamePoolsR1));
        Assert.Equal(0x18, Marshal.OffsetOf<LocalPlayerR1>(nameof(LocalPlayerR1.onfootData)).ToInt32());
        Assert.Equal(0xAA, Marshal.OffsetOf<LocalPlayerR1>(nameof(LocalPlayerR1.incarData)).ToInt32());
        Assert.Equal(0x161, Marshal.OffsetOf<LocalPlayerR1>(nameof(LocalPlayerR1.weaponsData)).ToInt32());
        Assert.Equal(0x1B0, Marshal.OffsetOf<LocalPlayerR1>(nameof(LocalPlayerR1.cameraTarget)).ToInt32());
        Assert.Equal(0xC8, Marshal.OffsetOf<RemotePlayerR1>(nameof(RemotePlayerR1.onfootData)).ToInt32());
        Assert.Equal(0x10C, Marshal.OffsetOf<RemotePlayerR1>(nameof(RemotePlayerR1.incarData)).ToInt32());
        Assert.Equal(0x18, Marshal.OffsetOf<NetGamePoolsR1>(nameof(NetGamePoolsR1.Player)).ToInt32());
    }

    [Fact]
    public void NativeSyncAndPlayerMetadataRemainDistinct()
    {
        byte* bytes = stackalloc byte[509];
        new Span<byte>(bytes, 509).Clear();
        *(ushort*)(bytes + 0xAB) = 73;
        *(Vector3*)(bytes + 0xC8 + 6) = new(1, 2, 3);
        *(Vector3*)(bytes + 0x10C + 24) = new(4, 5, 6);
        *(float*)(bytes + 0x1BC) = 97;

        var player = (RemotePlayerR1*)bytes;

        Assert.Equal((ushort)73, player->id);
        Assert.Equal(new Vector3(1, 2, 3), player->onfootData.Position);
        Assert.Equal(new Vector3(4, 5, 6), player->incarData.Position);
        Assert.Equal(97, player->reportedHealth);
    }

    [Fact]
    public void RepeatedNativeReadsAndAddressSelectionAllocateNothing()
    {
        RemotePlayerR1 player = default;
        player.id = 73;
        ReadRepeated(&player, 10_000);
        long before = GC.GetAllocatedBytesForCurrentThread();

        long result = ReadRepeated(&player, 1_000_000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(73_000_000, result);
        Assert.Equal(0, allocated);
    }

    private static long ReadRepeated(RemotePlayerR1* player, int count)
    {
        long total = 0;
        for (int i = 0; i < count; i++)
        {
            total += player->id;
            _ = SampOffsets.Resolve(SFSampVersion.R1, 0x307F0, 0x33BA0, 0x342E0);
        }
        return total;
    }
}
