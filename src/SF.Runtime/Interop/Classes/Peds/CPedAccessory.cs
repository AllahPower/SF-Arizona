using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SFSharp.Runtime.Interop.Classes.Entities;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct CPedAccessory
{
    public int ModelId;
    public int BoneId;
    public Vector3 Offset;
    public Vector3 Rotation;
    public Vector3 Scale;
    public uint MaterialColor1;
    public uint MaterialColor2;
}

[InlineArray(10)]
public struct Int32Array10
{
    private int _element0;
}

[InlineArray(10)]
public struct CPedAccessoryArray10
{
    private CPedAccessory _element0;
}

[InlineArray(10)]
public struct NIntArray10
{
    private nint _element0;
}
