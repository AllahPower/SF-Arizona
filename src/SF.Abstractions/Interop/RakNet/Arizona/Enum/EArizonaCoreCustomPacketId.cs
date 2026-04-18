namespace SFSharp.Abstractions.Interop.RakNet;

/// <summary>
/// Internal core.asi custom dispatcher packet identifiers observed in IDA.
/// </summary>
/// <remarks>
/// Assert: this is a post-transport dispatcher layer used by Arizona core modules.
/// Assert: this is not the same thing as raw packet 220 / 221 sub-IDs.
/// Assert: duplicate values are intentional because several core modules subscribe to the same dispatcher ID.
/// Assert: only IDs confirmed in reverse engineering are named here.
/// </remarks>
public enum EArizonaCoreCustomPacketId : ushort
{
    // Assert: u16 vehicleId, bool toggle, and when enabled then bool isSimpleModel + u16 modelId.
    VehicleBrakeCalipers = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    VehicleLightsColor = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    VehicleNeon = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    VehicleSpeedLimiter = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    WeaponUpgrades = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    ModelInstance = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    NumberPlate = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    PedMaterial = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    CustomMarker = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    CustomCrosshairs = 15,

    // Assert: shared dispatcher ID 15; module-specific secondary layout is still being recovered.
    LaunchStart = 15,

    // Assert: VehicleMaterials dispatcher; exact payload layout is still being recovered.
    VehicleMaterials = 63,

    // Assert: VehicleDrift secondary dispatcher; exact payload layout is still being recovered.
    VehicleDriftSecondary = 239,

    // Assert: VehicleDrift primary dispatcher; exact payload layout is still being recovered.
    VehicleDriftPrimary = 240,

    // Assert: shared dispatcher ID 254; TestDrive participates on the same bus.
    TestDrive = 254,

    // Assert: shared dispatcher ID 254; VehicleLightsColor participates on the same bus.
    VehicleLightsColorAlt = 254,

    // Assert: recovered module-local sub-opcodes include 104 = u8 bgType + optional u32 timeout and 117 = bool viceCityFlag.
    ViceCityServer = 254,

    // Assert: shared dispatcher ID 254; ChatIcon participates on the same bus.
    ChatIcon = 254,

    // Assert: shared dispatcher ID 254; Portal participates on the same bus.
    Portal = 254,

    // Assert: shared dispatcher ID 254; StunIcon participates on the same bus.
    StunIcon = 254,

    // Assert: Streamer and Lines both subscribe here; exact payload layout is still being recovered.
    Streamer = 4078,

    // Assert: Streamer and Lines both subscribe here; exact payload layout is still being recovered.
    Lines = 4078,

    // Assert: VehicleFeatures dispatcher; exact payload layout is still being recovered.
    VehicleFeatures = 4095,
}
