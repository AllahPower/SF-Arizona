using System.Numerics;
using System.Runtime.InteropServices;
using SFSharp.Abstractions.Samp.Events;

namespace SFSharp.Runtime.Interop.Classes;

/// <summary>Borrowed native views used only inside hook invocations. Public snapshots own their data.</summary>
internal static unsafe class SampObservationMemory
{
    internal static T Read<T>(nint pointer, int offset) where T : unmanaged =>
        MemoryMarshal.Read<T>(new ReadOnlySpan<byte>((void*)(pointer + offset), sizeof(T)));

    internal static SFSampGameState SessionState() => SampEnvironment.RequiredProfile.NormalizeState(CNetGame.Instance.State);

    internal static SFSampTextDrawSelectionEvent Selection(nint pointer) => new(
        Read<int>(pointer, 0) != 0, Read<uint>(pointer, 4), Read<ushort>(pointer, 8));

    internal static SFSampSpectatingEvent Spectating(nint pointer, SFSampVersion version)
    {
        (int enabled, int spec) = version switch
        {
            SFSampVersion.R1 => (0x146, 0x30E),
            SFSampVersion.R3_1 => (0x108, 0x312),
            SFSampVersion.R5_1 => (0x108, 0x30E),
            _ => throw new SFSampFeatureUnavailableException(version, "Spectating.Layout"),
        };
        return new(Read<int>(pointer, enabled) != 0, (SFSampSpectatingTarget)Read<byte>(pointer, spec + 1),
            Read<int>(pointer, spec + 2), Read<byte>(pointer, spec));
    }

    internal static SFSampRemoteSnapshot Remote(nint pointer, SFSampVersion version)
    {
        // R1 rearranges the prefix and sync blocks. R5 keeps R3 sync blocks, but moves IDs to
        // the tail and moves timing/target fields. These are field offsets, not module RVAs.
        bool r1 = version == SFSampVersion.R1, r5 = version == SFSampVersion.R5_1;
        if (version is not (SFSampVersion.R1 or SFSampVersion.R3_1 or SFSampVersion.R5_1))
            throw new SFSampFeatureUnavailableException(version, "RemotePlayers.Layout");
        return new(Read<ushort>(pointer, r1 ? 0xAB : r5 ? 0x1E5 : 8),
            Read<ushort>(pointer, r1 ? 0xAD : r5 ? 0x1E7 : 0xA),
            Read<byte>(pointer, r1 ? 9 : 0x10A), Read<byte>(pointer, r1 ? 0xA : 0x10B),
            Read<uint>(pointer, r5 ? 0x1B9 : 0x1C5), Read<uint>(pointer, r5 ? 0x1BD : 0x1C9),
            Read<float>(pointer, r1 ? 0x1BC : 0x1B0), Read<float>(pointer, r1 ? 0x1B8 : 0x1AC),
            Read<Vector3>(pointer, r1 ? 0x7B : r5 ? 0x17C : 0x114),
            Read<Vector3>(pointer, r1 ? 0x93 : r5 ? 0x194 : 0x12C));
    }

    internal static int SyncSize(SFSampSyncKind kind) => kind switch
    {
        SFSampSyncKind.OnFoot => 68, SFSampSyncKind.Vehicle => 63, SFSampSyncKind.Passenger => 24,
        SFSampSyncKind.Aim => 31, SFSampSyncKind.Trailer => 54,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    internal static SFSampSyncPayload Input(nint pointer, SFSampSyncKind kind)
    {
        int size = SyncSize(kind);
        // Rejected stale updates and ped-less aim calls may return without touching their input.
        // An absent native input stays absent; the observer must not dereference it first.
        return pointer == 0 ? default : new(new ReadOnlySpan<byte>((void*)pointer, size));
    }

    internal static SFSampSyncPayload Stored(nint pointer, SFSampVersion version, SFSampSyncKind kind)
    {
        if (version is not (SFSampVersion.R1 or SFSampVersion.R3_1 or SFSampVersion.R5_1))
            throw new SFSampFeatureUnavailableException(version, "RemotePlayers.StoredLayout");
        // Aim and trailer routines act on game objects instead of maintaining these cached fields.
        int offset = (version, kind) switch
        {
            (SFSampVersion.R1, SFSampSyncKind.OnFoot) => 0xC8,
            (SFSampVersion.R1, SFSampSyncKind.Vehicle) => 0x10C,
            (SFSampVersion.R1, SFSampSyncKind.Passenger) => 0x181,
            (SFSampVersion.R3_1 or SFSampVersion.R5_1, SFSampSyncKind.OnFoot) => 0xC5,
            (SFSampVersion.R3_1 or SFSampVersion.R5_1, SFSampSyncKind.Vehicle) => 0x19,
            (SFSampVersion.R3_1 or SFSampVersion.R5_1, SFSampSyncKind.Passenger) => 0xAD,
            _ => -1,
        };
        return offset < 0 ? default : Input(pointer + offset, kind);
    }

    internal static int PickupIndex(nint pool, int lookupId)
    {
        // This array holds GTA lookup IDs, not server IDs. Native GetIndex searches it before sending the slot.
        for (int i = 0; i < 4096; i++)
            if (Read<int>(pool, 0x4004 + i * 4) == lookupId) return i;
        return -1;
    }

    internal static SFSampPickupEvent? Pickup(nint pool, int index, SFSampPickupChange change)
    {
        if ((uint)index >= 4096 || Read<int>(pool, 4 + index * 4) == 0) return null;
        int data = 0xF004 + index * 20;
        return new(index, index, Read<int>(pool, data), Read<int>(pool, data + 4), Read<Vector3>(pool, data + 8), change);
    }
}
