namespace SFSharp.Abstractions.Ui;

public readonly record struct SFLocalPlayerCameraTarget(
    ushort ObjectId,
    ushort VehicleId,
    ushort PlayerId,
    ushort ActorId);
