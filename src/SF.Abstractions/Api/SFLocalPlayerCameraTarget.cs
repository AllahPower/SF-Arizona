namespace SFSharp;

public readonly record struct SFLocalPlayerCameraTarget(
    ushort ObjectId,
    ushort VehicleId,
    ushort PlayerId,
    ushort ActorId);
