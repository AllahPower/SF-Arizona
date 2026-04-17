namespace SFSharp.Abstractions.Game;

public readonly record struct SFVehicleSnapshot(
    ushort Id,
    bool Exists,
    int Handle,
    bool IsInvulnerable,
    bool IsLightsOn,
    bool IsLocked,
    bool EngineState,
    byte PrimaryColor,
    byte SecondaryColor
);
