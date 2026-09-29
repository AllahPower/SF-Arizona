using System.Numerics;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Entities;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CMatrix
{
    public Vector3 Right;
    public uint Flags;
    public Vector3 Up;
    public float PadU;
    public Vector3 At;
    public float PadA;
    public Vector3 Position;
    public float PadP;
}
