using System.Numerics;

namespace SFSharp;

public readonly record struct SFPlayerHeadState(Vector3 Direction, uint LastUpdate, uint LastLook);

public readonly record struct SFLocalPlayerSurfingState(
    ushort EntityId,
    uint LastUpdate,
    nint EntityPointer,
    bool IsStuck,
    bool IsActive,
    Vector3 Position,
    int Unknown0,
    int Mode);

public readonly record struct SFRemotePlayerMarkerState(
    bool Active,
    Vector3 Position,
    int Handle);

public readonly record struct SFRemotePlayerTargetState(
    Vector3 OnfootTargetPosition,
    Vector3 OnfootTargetSpeed,
    Vector3 IncarTargetPosition,
    Vector3 IncarTargetSpeed,
    Vector3 PositionDifference,
    float IncarRotationReal,
    Vector3 IncarRotationImaginary);
