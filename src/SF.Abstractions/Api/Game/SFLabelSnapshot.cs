using System.Numerics;

namespace SFSharp.Abstractions.Game;

public readonly record struct SFLabelSnapshot(
    string? Text,
    uint Color,
    Vector3 Position,
    float DrawDistance,
    bool BehindWalls,
    ushort AttachedToPlayer,
    ushort AttachedToVehicle);
