using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Vehicles;

/// <summary>Borrowed R1 CVehicle prefix containing its revision-specific lights flag.</summary>
/// <remarks>R1 stores lights at +0x5C; R3/R5 use +0x54. Engine and color fields are shared.
/// This is not a complete native object.</remarks>
[StructLayout(LayoutKind.Explicit, Size = 0x60, Pack = 1)]
internal struct VehicleR1
{
    [FieldOffset(0x5C)] internal int LightsOn;
}
