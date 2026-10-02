using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Engine;

/// <summary>GTA <c>CPlaceable</c>: the vtable plus either a simple transform or an allocated matrix.</summary>
[StructLayout(LayoutKind.Explicit, Size = GtaOffsets.CPlaceable.Size, Pack = 1)]
public unsafe struct GtaPlaceable
{
    [FieldOffset(GtaOffsets.CPlaceable.VTable)]
    public nint* VTable;

    [FieldOffset(GtaOffsets.CPlaceable.SimplePosition)]
    public Vector3 SimplePosition;

    [FieldOffset(GtaOffsets.CPlaceable.SimpleAngle)]
    public float SimpleAngle;

    [FieldOffset(GtaOffsets.CPlaceable.Matrix)]
    public GtaMatrix* Matrix;

    /// <summary>Mirrors <c>CEntity::GetPosition</c> (0x4043A0): the matrix position when allocated.</summary>
    public readonly Vector3 Position => Matrix is not null ? Matrix->Position : SimplePosition;

    /// <summary>Heading in degrees; inverse of <c>CMatrix::SetRotateZOnly</c>, which sets up = (-sin a, cos a).</summary>
    public readonly float HeadingDegrees
    {
        get
        {
            float radians = Matrix is not null
                ? MathF.Atan2(-Matrix->Up.X, Matrix->Up.Y)
                : SimpleAngle;
            return radians * (180f / MathF.PI);
        }
    }

    public readonly Matrix4x4 GetMatrix()
    {
        if (Matrix is not null)
        {
            return Matrix->ToMatrix4x4();
        }

        Matrix4x4 rotation = Matrix4x4.CreateRotationZ(SimpleAngle);
        rotation.Translation = SimplePosition;
        return rotation;
    }
}
