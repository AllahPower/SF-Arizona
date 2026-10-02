using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CMatrix</c>: row vectors padded to 16 bytes, followed by the RW attach link.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CMatrix.Size, Pack = 1)]
public struct GtaMatrix
{
    [FieldOffset(GtaOffsets.CMatrix.Right)]
    public Vector3 Right;

    [FieldOffset(GtaOffsets.CMatrix.Up)]
    public Vector3 Up;

    [FieldOffset(GtaOffsets.CMatrix.At)]
    public Vector3 At;

    [FieldOffset(GtaOffsets.CMatrix.Position)]
    public Vector3 Position;

    public readonly Matrix4x4 ToMatrix4x4()
    {
        return new Matrix4x4(
            Right.X, Right.Y, Right.Z, 0f,
            Up.X, Up.Y, Up.Z, 0f,
            At.X, At.Y, At.Z, 0f,
            Position.X, Position.Y, Position.Z, 1f);
    }
}
