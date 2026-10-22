using System.Numerics;
using System.Runtime.InteropServices;
using SFSharp.Abstractions.Samp;
using SFSharp.Abstractions.Samp.Events;
using SFSharp.Runtime.Interop.Classes;

namespace SF.Network.Tests;

public sealed unsafe class SampObservationMemoryTests
{
    [Theory]
    [InlineData(SFSampVersion.R1, 0xAB, 0xAD, 9, 0xA, 0x1C9, 0x1BC, 0x7B, 0x93)]
    [InlineData(SFSampVersion.R3_1, 8, 0xA, 0x10A, 0x10B, 0x1C9, 0x1B0, 0x114, 0x12C)]
    [InlineData(SFSampVersion.R5_1, 0x1E5, 0x1E7, 0x10A, 0x10B, 0x1BD, 0x1B0, 0x17C, 0x194)]
    public void RemoteSnapshotUsesTheExactRevisionLayout(SFSampVersion version, int id, int vehicle, int state, int seat,
        int timestamp, int health, int footTarget, int vehicleTarget)
    {
        byte* data = stackalloc byte[509];
        new Span<byte>(data, 509).Clear();
        Write(data, id, (ushort)73);
        Write(data, vehicle, (ushort)1999);
        data[state] = 18;
        data[seat] = 3;
        Write(data, timestamp, 0xF1234567u);
        Write(data, health, 97.5f);
        Write(data, footTarget, new Vector3(1, 2, 3));
        Write(data, vehicleTarget, new Vector3(4, 5, 6));

        SFSampRemoteSnapshot snapshot = SampObservationMemory.Remote((nint)data, version);

        Assert.Equal((ushort)73, snapshot.PlayerId);
        Assert.Equal((ushort)1999, snapshot.VehicleId);
        Assert.Equal((byte)18, snapshot.State);
        Assert.Equal((byte)3, snapshot.SeatId);
        Assert.Equal(0xF1234567u, snapshot.LastTimestamp);
        Assert.Equal(97.5f, snapshot.Health);
        Assert.Equal(new Vector3(1, 2, 3), snapshot.OnFootTarget);
        Assert.Equal(new Vector3(4, 5, 6), snapshot.VehicleTarget);
    }

    [Theory]
    [InlineData(SFSampVersion.R1, SFSampSyncKind.OnFoot, 0xC8, 68)]
    [InlineData(SFSampVersion.R1, SFSampSyncKind.Vehicle, 0x10C, 63)]
    [InlineData(SFSampVersion.R1, SFSampSyncKind.Passenger, 0x181, 24)]
    [InlineData(SFSampVersion.R3_1, SFSampSyncKind.OnFoot, 0xC5, 68)]
    [InlineData(SFSampVersion.R3_1, SFSampSyncKind.Vehicle, 0x19, 63)]
    [InlineData(SFSampVersion.R3_1, SFSampSyncKind.Passenger, 0xAD, 24)]
    [InlineData(SFSampVersion.R5_1, SFSampSyncKind.OnFoot, 0xC5, 68)]
    [InlineData(SFSampVersion.R5_1, SFSampSyncKind.Vehicle, 0x19, 63)]
    [InlineData(SFSampVersion.R5_1, SFSampSyncKind.Passenger, 0xAD, 24)]
    public void StoredSyncIsAnOwnedCompleteCopy(SFSampVersion version, SFSampSyncKind kind, int offset, int length)
    {
        byte* data = stackalloc byte[509];
        new Span<byte>(data, 509).Clear();
        for (int i = 0; i < length; i++) data[offset + i] = (byte)(i + 1);

        SFSampSyncPayload copy = SampObservationMemory.Stored((nint)data, version, kind);
        new Span<byte>(data, 509).Clear();

        Assert.Equal(length, copy.Length);
        for (int i = 0; i < length; i++) Assert.Equal((byte)(i + 1), copy.ReadByte(i));
    }

    [Theory]
    [InlineData(SFSampVersion.R1, 0x146, 0x30E)]
    [InlineData(SFSampVersion.R3_1, 0x108, 0x312)]
    [InlineData(SFSampVersion.R5_1, 0x108, 0x30E)]
    public void SpectatingReadsActualStateNotRequestedTarget(SFSampVersion version, int enabled, int spec)
    {
        byte* data = stackalloc byte[812];
        new Span<byte>(data, 812).Clear();
        Write(data, enabled, 256);
        data[spec] = 14;
        data[spec + 1] = 2;
        Write(data, spec + 2, 123);

        SFSampSpectatingEvent value = SampObservationMemory.Spectating((nint)data, version);

        Assert.True(value.Enabled);
        Assert.Equal(SFSampSpectatingTarget.Vehicle, value.Target);
        Assert.Equal(123, value.TargetId);
        Assert.Equal((byte)14, value.CameraMode);
    }

    [Fact]
    public void AbsentNativeInputDoesNotCauseAnObserverDereference()
    {
        Assert.Equal(0, SampObservationMemory.Input(0, SFSampSyncKind.OnFoot).Length);
        Assert.Equal(0, SampObservationMemory.Input(0, SFSampSyncKind.Aim).Length);
    }

    [Fact]
    public void PickupLookupIdAndServerSlotAreNotInterchanged()
    {
        byte[] buffer = new byte[0x23004];
        fixed (byte* data = buffer)
        {
            const int slot = 901;
            Write(data, 4 + slot * 4, 456);
            Write(data, 0x4004 + slot * 4, 123456);
            Write(data, 0xF004 + slot * 20, 1274);
            Write(data, 0xF004 + slot * 20 + 4, 2);
            Write(data, 0xF004 + slot * 20 + 8, new Vector3(1, 2, 3));

            int index = SampObservationMemory.PickupIndex((nint)data, 123456);
            SFSampPickupEvent value = SampObservationMemory.Pickup((nint)data, index, SFSampPickupChange.Deleting)!.Value;

            Assert.Equal(slot, index);
            Assert.Equal(slot, value.ServerId);
            Assert.Equal(1274, value.Model);
            Assert.Equal(new Vector3(1, 2, 3), value.Position);
            Assert.Equal(-1, SampObservationMemory.PickupIndex((nint)data, 98765));
            Assert.Null(SampObservationMemory.Pickup((nint)data, 4096, SFSampPickupChange.Created));
            Assert.Null(SampObservationMemory.Pickup((nint)data, 4, SFSampPickupChange.Created));
        }
    }

    [Fact]
    public void PayloadRetainsAllBytesAndBoundsEveryRead()
    {
        Span<byte> original = stackalloc byte[80];
        for (int i = 0; i < original.Length; i++) original[i] = (byte)i;
        var payload = new SFSampSyncPayload(original);
        original.Clear();
        Span<byte> copied = stackalloc byte[80];

        payload.CopyTo(copied);

        for (int i = 0; i < copied.Length; i++) Assert.Equal((byte)i, copied[i]);
        Assert.True(payload.ContentEquals(new(copied)));
        Assert.Throws<IndexOutOfRangeException>(() => payload.ReadByte(80));
        Assert.Throws<ArgumentOutOfRangeException>(() => payload.ReadSingle(79));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SFSampSyncPayload(new byte[81]));
        Assert.Equal(0, default(SFSampSyncPayload).Length);
    }

    [Fact]
    public void UnknownLayoutsFailBeforeDereferencingMemory()
    {
        Assert.Throws<SFSampFeatureUnavailableException>(() => SampObservationMemory.Remote(0, SFSampVersion.Unknown));
        Assert.Throws<SFSampFeatureUnavailableException>(() => SampObservationMemory.Spectating(0, SFSampVersion.Unknown));
        Assert.Throws<SFSampFeatureUnavailableException>(() => SampObservationMemory.Stored(0, SFSampVersion.Unknown, SFSampSyncKind.OnFoot));
    }

    [Fact]
    public void RepeatedSnapshotAndPayloadReadsAllocateNothing()
    {
        byte* data = stackalloc byte[509];
        new Span<byte>(data, 509).Clear();
        Write(data, 8, (ushort)73);
        ReadRepeated((nint)data, 1_000);
        long before = GC.GetAllocatedBytesForCurrentThread();

        long result = ReadRepeated((nint)data, 10_000);
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(730_000, result);
        Assert.Equal(0, allocated);
    }

    private static long ReadRepeated(nint data, int count)
    {
        long result = 0;
        for (int i = 0; i < count; i++)
        {
            result += SampObservationMemory.Remote(data, SFSampVersion.R3_1).PlayerId;
            SFSampSyncPayload payload = SampObservationMemory.Stored(data, SFSampVersion.R3_1, SFSampSyncKind.OnFoot);
            result += payload.ReadByte(0);
            _ = payload.ContentEquals(payload);
        }
        return result;
    }

    private static void Write<T>(byte* data, int offset, T value) where T : unmanaged =>
        MemoryMarshal.Write(new Span<byte>(data + offset, sizeof(T)), in value);
}
