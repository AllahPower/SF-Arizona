using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CVehicle</c>. Pool slots are 0xA18 bytes to fit subclasses, so never index vehicles by this size.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CVehicle.Size, Pack = 1)]
public unsafe struct GtaVehicle
{
    [FieldOffset(0)]
    public GtaPhysical Physical;

    [FieldOffset(GtaOffsets.CVehicle.PrimaryColor)]
    public byte PrimaryColor;

    [FieldOffset(GtaOffsets.CVehicle.SecondaryColor)]
    public byte SecondaryColor;

    [FieldOffset(GtaOffsets.CVehicle.Driver)]
    public GtaPed* Driver;

    [FieldOffset(GtaOffsets.CVehicle.Passengers)]
    public GtaPedPointerArray8 Passengers;

    [FieldOffset(GtaOffsets.CVehicle.MaxPassengers)]
    public byte MaxPassengers;

    [FieldOffset(GtaOffsets.CVehicle.Health)]
    public float Health;

    [FieldOffset(GtaOffsets.CVehicle.DoorLock)]
    public uint DoorLock;
}

[InlineArray(GtaOffsets.CVehicle.PassengerSlots)]
public struct GtaPedPointerArray8
{
    private nint _element;
}
