namespace SFSharp;

// Internal core.asi custom packet IDs observed in IDA.
// This is a post-transport dispatcher layer used by Arizona core modules.
// It is not the same thing as raw Packet 220/221 sub-ids.
// Duplicate values are intentional - several core modules subscribe to the same dispatcher ID.
public enum ArizonaCoreCustomPacketId : ushort
{
    // u16 vehicleId, bool toggle, if toggle then bool isSimpleModel, u16 modelId
    VehicleBrakeCalipers = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    VehicleLightsColor = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    VehicleNeon = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    VehicleSpeedLimiter = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    WeaponUpgrades = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    ModelInstance = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    NumberPlate = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    PedMaterial = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    CustomMarker = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    CustomCrosshairs = 15,

    // shared dispatcher id 15 - module-specific secondary layout, exact payload still being recovered
    LaunchStart = 15,

    // VehicleMaterials module dispatcher - exact payload layout still being recovered
    VehicleMaterials = 63,

    // VehicleDrift secondary dispatcher - exact payload layout still being recovered
    VehicleDriftSecondary = 239,

    // VehicleDrift primary dispatcher - exact payload layout still being recovered
    VehicleDriftPrimary = 240,

    // shared dispatcher id 254 - TestDrive module participates on the same bus, exact payload still being recovered
    TestDrive = 254,

    // shared dispatcher id 254 - VehicleLightsColor module participates on the same bus, exact payload still being recovered
    VehicleLightsColorAlt = 254,

    // module-local sub-opcodes recovered: 104 = u8 bgType, optional u32 timeout; 117 = bool viceCityFlag
    ViceCityServer = 254,

    // shared dispatcher id 254 - ChatIcon module participates on the same bus, exact payload still being recovered
    ChatIcon = 254,

    // shared dispatcher id 254 - Portal module participates on the same bus, exact payload still being recovered
    Portal = 254,

    // shared dispatcher id 254 - StunIcon module participates on the same bus, exact payload still being recovered
    StunIcon = 254,

    // Streamer and Lines modules subscribe here - exact payload layout still being recovered
    Streamer = 4078,

    // Streamer and Lines modules subscribe here - exact payload layout still being recovered
    Lines = 4078,

    // VehicleFeatures dispatcher - exact payload layout still being recovered
    VehicleFeatures = 4095,
}
