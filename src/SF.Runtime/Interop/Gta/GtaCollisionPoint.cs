using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Gta;

// IDA CColPoint, 1.0 US: 44 bytes. The B surface is the world entity hit by a query line.
[StructLayout(LayoutKind.Explicit, Size = 0x2C)]
internal struct GtaCollisionPoint
{
    [FieldOffset(0)] public Vector3 Position;
    [FieldOffset(0x10)] public Vector3 Normal;
    [FieldOffset(0x23)] public byte SurfaceB;
}
