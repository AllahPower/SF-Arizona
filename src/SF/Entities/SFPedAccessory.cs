using System.Numerics;

namespace SFSharp;

public readonly record struct SFPedAccessory(
    int Slot,
    int ModelId,
    int BoneId,
    Vector3 Offset,
    Vector3 Rotation,
    Vector3 Scale,
    uint MaterialColor1,
    uint MaterialColor2,
    nint ObjectPointer);
