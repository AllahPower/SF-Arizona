namespace SFSharp.Runtime.Interop.Offsets;

/// <summary>Verified SA-MP addresses and native field offsets, grouped by the owning native type.</summary>
/// <remarks>
/// Each <c>Select(r1, r3, r5)</c> call lists R1, R3-1 and R5-1 values in that order.
/// Keep revisions beside each other so a binding can be checked without comparing three tables.
/// Function and global values are RVAs in samp.dll, not absolute addresses. ModuleResolver adds
/// the loaded module base. Field offsets are relative to an object and must not receive that base.
/// Constants describe layouts shared by all supported revisions; differing layouts use Select or a profile.
/// The detector checks both the PE entry point and the exact version marker before selecting a profile.
/// Unknown clients are rejected rather than guessed as R3, since a wrong address can corrupt native memory.
/// Wrappers bind function pointers in static readonly fields after detection, not on every packet.
/// </remarks>
public static class SampOffsets
{
    /// <summary>Selects a value for the process-wide verified client profile without allocating.</summary>
    /// <param name="r1">Value for SA-MP 0.3.7 R1.</param>
    /// <param name="r3">Value for SA-MP 0.3.7 R3-1.</param>
    /// <param name="r5">Value for SA-MP 0.3.7 R5-1.</param>
    /// <example><code>Select(r1: 0x307F0, r3: 0x33BA0, r5: 0x342E0)</code></example>
    /// <exception cref="NotSupportedException">The loaded client has no verified native profile.</exception>
    internal static int Select(int r1, int r3, int r5) =>
        Resolve(SampEnvironment.RequiredProfile.Version, r1, r3, r5);

    /// <summary>Pure revision selection, also used by tests without loading samp.dll.</summary>
    internal static int Resolve(SFSampVersion version, int r1, int r3, int r5) => version switch
    {
        SFSampVersion.R1 => r1,
        SFSampVersion.R3_1 => r3,
        SFSampVersion.R5_1 => r5,
        _ => throw new NotSupportedException("No native address profile for this SA-MP client."),
    };
    public static class CNetGame
    {
        public static int Instance => checked((int)SampEnvironment.RequiredProfile.NetGameRva);
        public static int GetPlayerPool => Select(0x1160, 0x1160, 0x1170);
        public static int GetVehiclePool => Select(0x1170, 0x1170, 0x1180);
        public static int GetObjectPool => Select(0x2E00, 0x2DF0, 0x2E10);
        public static int GetActorPool => Select(0x2E10, 0x2E00, 0x2E20);
        public static int GetState => checked((int)SampEnvironment.RequiredProfile.GetStateRva);
        public static int LanMode => Select(0x2E30, 0x2E20, 0x2E40);
        public static int GetPickupPool => Select(0x8130, 0x8170, 0x84E0);
        public static int GetMenuPool => Select(0x8140, 0x8180, 0x84F0);
        public static int SetState => Select(0x8150, 0x8190, 0x8500);
        public static int InitializePools => Select(0x8160, 0x81D0, 0x8540);
        public static int InitialNotification => Select(0x8380, 0x83F0, 0x8760);
        public static int GetCounter => Select(0x8500, 0x8570, 0x88E0);
        public static int InitializeGameLogic => Select(0x8510, 0x8580, 0x88F0);
        public static int GetRakClient => Select(0x1A40, 0x1A40, 0xBBC0);
        public static int UpdatePlayers => Select(0x8A10, 0x8BA0, 0x8F10);
        public static int ResetPlayerPool => Select(0x8AE0, 0x8C70, 0x8FE0);
        public static int ResetVehiclePool => Select(0x8B80, 0x8D10, 0x9080);
        public static int ResetTextDrawPool => Select(0x8C20, 0x8DB0, 0x9110);
        public static int ResetObjectPool => Select(0x8CC0, 0x8E50, 0x91B0);
        public static int ResetGangZonePool => Select(0x8D60, 0x8EF0, 0x9250);
        public static int ResetPickupPool => Select(0x8E00, 0x8F90, 0x92F0);
        public static int ResetMenuPool => Select(0x8E60, 0x8FF0, 0x9350);
        // R5 SAMP-API labels are swapped. Native stores target pool +0x18 (labels) / +0x10 (actors).
        public static int ResetLabelPool => Select(0x8F00, 0x9080, 0x93F0);
        public static int ResetActorPool => Select(0x8FA0, 0x9120, 0x9490);
        public static int ResetMarkers => Select(0x9DE0, 0x9F50, 0xA2C0);
        public static int ResetPools => Select(0xA010, 0xA190, 0xA4F0);
        public static int ShutdownForRestart => Select(0xA060, 0xA1E0, 0xA540);
    }

    public static class CVehiclePool
    {
        public const int MaxVehicles = 2000;
        public static int Get => Select(0x1110, 0x1110, 0x1120);
        public static int DoesExist => Select(0x1140, 0x1140, 0x1150);
        public static int UpdateCount => Select(0x1AEC0, 0x1E260, 0x1E9B0);
        public static int Create => Select(0x1B590, 0x1E930, 0x1F080);
        public static int Delete => Select(0x1AF90, 0x1E330, 0x1EA80);
        public static int ChangeInterior => Select(0x1B010, 0x1E3B0, 0x1EB00);
        public static int SetParams => Select(0x1B040, 0x1E3E0, 0x1EB30);
        public static int Find => Select(0x1B0A0, 0x1E440, 0x1EB90);
        public static int GetNearest => Select(0x1B110, 0x1E4B0, 0x1EC00);
        public static int ProcessWaitingList => Select(0x1B810, 0x1EBC0, 0x1F300);
        public static int Process => Select(0x1B8D0, 0x1EC80, 0x1F3C0);
    }

    public static class CObjectPool
    {
        public const int MaxObjects = 1000;
        public const int LargestId = 0x0000;
        public const int OccupancyArray = 0x0004;
        public const int ObjectArray = 0x0FA4;
        public static int Get => Select(0x2DD0, 0x2DC0, 0x2DE0);
        public static int GetCount => Select(0xF3D0, 0x124E0, 0x12830);
        public static int Delete => Select(0xF3F0, 0x12500, 0x12850);
        public static int Create => Select(0xF470, 0x12580, 0x128D0);
        public static int GetId => Select(0xF560, 0x126C0, 0x12A10);
        public static int Process => Select(0xF5A0, 0x12700, 0x12A50);
        public static int ConstructMaterials => Select(0xF660, 0x127C0, 0x12B10);
        public static int ShutdownMaterials => Select(0xF6A0, 0x12800, 0x12B50);
        public static int Draw => Select(0xF6E0, 0x12840, 0x12B90);
        public static int DrawLast => Select(0xF720, 0x12880, 0x12BD0);
    }

    public static class CObject
    {
        public static int Constructor => Select(0xA3AB0, 0xA8880, 0xA9110);
        public static int GetDistance => Select(0xA2960, 0xA7730, 0xA7FC0);
        public static int Stop => Select(0xA29D0, 0xA77A0, 0xA8030);
        public static int SetRotation => Select(0xA2A40, 0xA7810, 0xA80A0);
        public static int SetAttachedToVehicle => Select(0xA2AB0, 0xA7880, 0xA8110);
        public static int SetAttachedToObject => Select(0xA2B40, 0xA7910, 0xA81A0);
        public static int AttachToVehicle => Select(0xA2BE0, 0xA79B0, 0xA8240);
        public static int AttachToObject => Select(0xA2C60, 0xA7A30, 0xA82C0);
        public static int Rotate => Select(0xA2D60, 0xA7B30, 0xA83C0);
        public static int AttachedToMovingEntity => Select(0xA2E60, 0xA7C30, 0xA84C0);
        public static int SetMaterial => Select(0xA2ED0, 0xA7CA0, 0xA8530);
        public static int SetMaterialText => Select(0xA3050, 0xA7E20, 0xA86B0);
        public static int GetMaterialSize => Select(0xA3620, 0xA83F0, 0xA8C80);
        public static int ShutdownMaterialText => Select(0xA3870, 0xA8640, 0xA8ED0);
        public static int Render => Select(0xA3900, 0xA86D0, 0xA8F60);
        public static int Process => Select(0xA3F70, 0xA8DC0, 0xA9650);
        public static int ConstructMaterialText => Select(0xA47F0, 0xA9650, 0xA9EE0);
        public static int Draw => Select(0xA48A0, 0xA9700, 0xA9F90);
    }

    public static class CActorPool
    {
        public const int MaxActors = 1000;
        public const int LargestId = 0x0000;
        public const int ObjectArray = 0x0004;
        public const int OccupancyArray = 0x0FA4;
        public static int Get => Select(0x1600, 0x1600, 0x1610);
        public static int DoesExist => Select(0x1630, 0x1630, 0x1640);
        public static int Delete => Select(0x16E0, 0x16E0, 0x16F0);
        public static int Find => Select(0x18A0, 0x18A0, 0x18B0);
        public static int Create => Select(0x18F0, 0x18F0, 0x1900);
    }

    public static class CActor
    {
        public static int Constructor => Select(0x97C60, 0x9BBA0, 0x9C2B0);
        public static int Destroy => Select(0x97DA0, 0x9BCF0, 0x9C400);
        public static int PerformAnimation => Select(0x97E00, 0x9BD50, 0x9C460);
        public static int SetRotation => Select(0x97F10, 0x9BE60, 0x9C570);
        public static int GetHealth => Select(0x97F50, 0x9BEA0, 0x9C5B0);
        public static int SetHealth => Select(0x97F70, 0x9BEC0, 0x9C5D0);
        public static int SetInvulnerable => Select(0x980A0, 0x9BFF0, 0x9C700);
    }

    public static class CPickupPool
    {
        public const int MaxPickups = 4096;
        public const int Count = 0x0000;
        public const int Handles = 0x0004;
        public const int ServerIds = 0x4004;
        public const int Timers = 0x8004;
        public const int Weapons = 0xC004;
        public const int Pickups = 0xF004;
        public static int Constructor => Select(0x80F0, 0x8130, 0x84A0);
        public static int Destructor => Select(0xFF60, 0x130C0, 0x13410);
        public static int CreateWeapon => Select(0xFCD0, 0x12E30, 0x13180);
        public static int Create => Select(0xFDC0, 0x12F20, 0x13270);
        public static int Delete => Select(0xFE70, 0x12FD0, 0x13320);
        public static int DeleteWeapon => Select(0xFED0, 0x13030, 0x13380);
        public static int GetIndex => Select(0xFF30, 0x13090, 0x133E0);
        public static int SendNotification => Select(0xFF90, 0x130F0, 0x13440);
        public static int Process => Select(0x10070, 0x131D0, 0x13520);
    }

    public static class CMenuPool
    {
        public const int MaxMenus = 128;
        public const int MenuArray = 0x0000;
        public const int OccupancyArray = 0x0200;
        public const int CurrentMenu = 0x0400;
        public const int Cancelled = 0x0401;
        public static int Constructor => Select(0x7AB0, 0x7AE0, 0x7E60);
        public static int Destructor => Select(0x7E20, 0x7E50, 0x81C0);
        public static int Create => Select(0x7B00, 0x7B30, 0x7EB0);
        public static int Delete => Select(0x7BD0, 0x7C00, 0x7F70);
        public static int Show => Select(0x7C20, 0x7C50, 0x7FC0);
        public static int Hide => Select(0x7C80, 0x7CB0, 0x8020);
        public static int GetTextPointer => Select(0x7CC0, 0x7CF0, 0x8060);
        public static int Process => Select(0x7E60, 0x7E90, 0x8200);
    }

    public static class CMenu
    {
        public static int SetTitle => SampEnvironment.IsR5 ? 0xA7530
            : throw new NotSupportedException("CMenu::SetTitle is an R5-only native method.");
        public static int Constructor => Select(0xA23C0, 0xA6CE0, 0xA7420);
        public static int AddItem => Select(0xA2460, 0xA6D80, 0xA7580);
        public static int SetColumnTitle => Select(0xA2490, 0xA6DB0, 0xA7600);
        public static int Hide => Select(0xA24C0, 0xA6DE0, 0xA7670);
        public static int GetItem => Select(0xA24E0, 0xA6E00, 0xA7690);
        public static int GetTitle => Select(0xA2500, 0xA6E20, 0xA76B0);
        public static int GetString => Select(0xA2530, 0xA6E50, 0xA76E0);
        public static int GetActiveRow => Select(0xA2560, 0xA6E80, 0xA7710);
        public static int Show => Select(0xA2590, 0xA6EB0, 0xA7740);
    }

    public static class CGangZonePool
    {
        public const int MaxGangZones = 1024;
        public const int ObjectArray = 0x0000;
        public const int OccupancyArray = 0x1000;
        public static int Constructor => Select(0x2110, 0x2100, 0x2120);
        public static int Destructor => Select(0x2140, 0x2130, 0x2150);
        public static int Create => Select(0x2170, 0x2160, 0x2180);
        public static int StartFlashing => Select(0x21F0, 0x21E0, 0x2200);
        public static int StopFlashing => Select(0x2210, 0x2200, 0x2220);
        public static int Delete => Select(0x2230, 0x2220, 0x2240);
        public static int Draw => Select(0x2260, 0x2250, 0x2270);
    }

    public static class CLabelPool
    {
        public const int MaxLabels = 2048;
        public const int ObjectArray = 0x0000;
        public const int OccupancyArray = 0xE800;
        public static int Constructor => Select(0x1180, 0x1180, 0x1190);
        public static int Destructor => Select(0x15D0, 0x15D0, 0x15E0);
        public static int Create => Select(0x11C0, 0x11C0, 0x11D0);
        public static int Delete => Select(0x12D0, 0x12D0, 0x12E0);
        public static int Draw => Select(0x1340, 0x1340, 0x1350);
    }

    public static class CTextDrawPool
    {
        public const int MaxTextDraws = 2048;
        public const int MaxLocalTextDraws = 256;
        public const int TotalTextDraws = MaxTextDraws + MaxLocalTextDraws;
        public const int OccupancyArray = 0x0000;
        public const int ObjectArray = 0x2400;
        public static int Constructor => Select(0x1ACB0, 0x1E050, 0x1E7A0);
        public static int Destructor => Select(0x1ADE0, 0x1E180, 0x1E8D0);
        public static int Delete => Select(0x1AD00, 0x1E0A0, 0x1E7F0);
        public static int Draw => Select(0x1AD40, 0x1E0E0, 0x1E830);
        public static int Create => Select(0x1AE20, 0x1E1C0, 0x1E910);
    }

    public static class CTextDraw
    {
        public static int Constructor => Select(0xACF10, 0xB2E50, 0xB36E0);
        public static int Destructor => Select(0xAC860, 0xB26C0, 0xB2F50);
        public static int SetText => Select(0xAC870, 0xB26D0, 0xB2F60);
        public static int Draw => Select(0xACD90, 0xB2BF0, 0xB3480);
    }

    public static class CInput
    {
        public static int Instance => Select(0x21A0E8, 0x26E8CC, 0x26EB84);
        public static int Send => Select(0x65C60, 0x69190, 0x69900);
        // SAMP-API R3-1, packed x86 CInput::m_bEnabled (BOOL).
        public const int Enabled = 0x14E0;
    }

    public static class CDialog
    {
        public static int Instance => Select(0x21A0B8, 0x26E898, 0x26EB50);
        public const int Active = 0x28;
        public static int Show => Select(0x6B9C0, 0x6F8C0, 0x6FFB0);
        public static int Hide => Select(0x6B210, 0x6F110, 0x6F860);
        public static int Close => Select(0x6C040, 0x6FF40, 0x70630);
    }

    public static class CLocalPlayer
    {
        public static int GetPed => Select(0x2D60, 0x2D50, 0x2D70);
        public static int GetSpecialAction => Select(0x3340, 0x3340, 0x3410);
        public static int SetColor => Select(0x3D40, 0x3D50, 0x3ED0);
        public static int GetColorAsRgba => Select(0x3D70, 0x3D80, 0x3F00);
        public static int GetColorAsArgb => Select(0x3D90, 0x3DA0, 0x3F20);
        public static int RequestSpawn => Select(0x3EC0, 0x3ED0, 0x4060);
        // SAMP-API multiver 6d4db99, CLocalPlayer.cpp for each revision.
        // R3 entries and thiscall stack cleanup also checked in sampR3.dll disassembly.
        public static int Spawn => Select(0x3AD0, 0x3AD0, 0x3C20);
        public static int WastedNotification => Select(0x55E0, 0x5620, 0x5810);
        public static int RequestClass => Select(0x56A0, 0x56E0, 0x58D0);
        public static int ChangeInterior => Select(0x5740, 0x5780, 0x5970);
        public static int EnterVehicle => Select(0x58C0, 0x58E0, 0x5AD0);
        public static int ExitVehicle => Select(0x59E0, 0x5A00, 0x5BF0);
        public static int Chat => Select(0x57F0, 0x5820, 0x5A10);
    }

    public static class CEntity
    {
        public static int GetMatrix => Select(0x9A150, 0x9E400, 0x9EB10);
        public static int SetMatrix => Select(0x9A200, 0x9E4B0, 0x9EBC0);
        public static int UpdateRwFrame => Select(0x9A2C0, 0x9E570, 0x9EC80);
        public static int GetSpeed => Select(0x9A320, 0x9E5D0, 0x9ECE0);
        public static int SetSpeed => Select(0x9A350, 0x9E600, 0x9ED10);
        public static int GetTurnSpeed => Select(0x9A470, 0x9E720, 0x9EE30);
        public static int SetTurnSpeed => Select(0x9A4A0, 0x9E750, 0x9EE60);
        public static int ApplyTurnSpeed => Select(0x9A4D0, 0x9E780, 0x9EE90);
        public static int SetModelIndex => Select(0x9A590, 0x9E840, 0x9EF50);
        public static int GetModelIndex => Select(0x9A670, 0x9E920, 0x9F030);
        public static int Teleport => Select(0x9A680, 0x9E930, 0x9F040);
        public static int GetDistanceToLocalPlayer => Select(0x9A700, 0x9E9B0, 0x9F0C0);
        public static int GetDistanceToCamera => Select(0x9A7D0, 0x9EA80, 0x9F190);
        public static int GetDistanceToLocalPlayerNoHeight => Select(0x9A830, 0x9EAE0, 0x9F1F0);
        public static int GetDistanceToPoint => Select(0x9A8F0, 0x9EBA0, 0x9F2B0);
        public static int DoesExist => Select(0x9AA10, 0x9ECC0, 0x9F3D0);
        public static int GetCollisionFlag => Select(0x9ACA0, 0x9EF50, 0x9F660);
        public static int SetCollisionFlag => Select(0x9AC70, 0x9EF20, 0x9F630);
        public static int SetCollisionProcessed => Select(0x9ACC0, 0x9EF70, 0x9F680);
        public static int GetEulerInverted => Select(0x9AF30, 0x9F1E0, 0x9F8F0);
    }

    public static class CPed
    {
        public static int GetHealth => Select(0xA6610, 0xAB4C0, 0xABD50);
        public static int SetHealth => Select(0xA6630, 0xAB4E0, 0xABD70);
        public static int GetArmour => Select(0xA6650, 0xAB500, 0xABD90);
        public static int SetArmour => Select(0xA6670, 0xAB520, 0xABDB0);
        public static int GetState => Select(0xA6700, 0xAB5B0, 0xABE40);
        public static int SetState => Select(0xA6710, 0xAB5C0, 0xABE50);
        public static int GetRotation => Select(0xA6750, 0xAB600, 0xABE90);
        public static int ForceRotation => Select(0xA6820, 0xAB680, 0xABF10);
        public static int SetRotation => Select(0xA67D0, 0xAB6D0, 0xABF60);
        public static int IsPassenger => Select(0xA6880, 0xAB730, 0xABFC0);
        public static int GetVehicle => Select(0xA68C0, 0xAB770, 0xAC000);
        public static int GetCurrentWeapon => Select(0xA6510, 0xAB3C0, 0xABC50);
        public static int GetCurrentWeaponAmmo => Select(0xA7A10, 0xAC8C0, 0xAD150);
        public static int GetVehicleSeatIndex => Select(0xA6AC0, 0xAB970, 0xAC200);
        public static int ClearWeapons => Select(0xA68D0, 0xAB780, 0xAC010);
        public static int EnableJetpack => Select(0xA75D0, 0xAC480, 0xACD10);
        public static int DisableJetpack => Select(0xA7620, 0xAC4D0, 0xACD60);
        public static int HasJetpack => Select(0xA7680, 0xAC530, 0xACDC0);
        public static int GetAimZ => Select(0xA81B0, 0xAD060, 0xAD8F0);
        public static int GetBonePosition => Select(0xA8D70, 0xADBF0, 0xAE480);
        public static int SetAimZ => Select(0xA81F0, 0xAD0A0, 0xAD930);
        public static int HasAccessory => Select(0xA96D0, 0xAE5A0, 0xAEE30);
        public static int DeleteAccessory => Select(0xA96F0, 0xAE5C0, 0xAEE50);
        public static int GetAccessoryState => Select(0xA9750, 0xAE620, 0xAEEB0);
        public static int GetAccessory => Select(0x101E0, 0x13330, 0x13680);
        public static int DeleteAllAccessories => Select(0xAB380, 0xB0220, 0xB0AB0);
        public static int AddAccessory => Select(0xAB3E0, 0xB0280, 0xB0B10);
    }

    public static class CRemotePlayer
    {
        public static int GetDistanceToLocalPlayer => Select(0x129A0, 0x15BB0, 0x16120);
        public static int SetColor => Select(0x129D0, 0x15BE0, 0x16150);
        public static int GetColorAsRgba => Select(0x129F0, 0x15C00, 0x16170);
        public static int GetColorAsArgb => Select(0x12A00, 0x15C10, 0x16180);
        public static int GetStatus => Select(0x12BA0, 0x15DB0, 0x16330);
        public static int DoesExist => Select(0x1080, 0x1080, 0x1080);
        public static int Spawn => Select(0x13890, 0x16A90, 0x17130);
        public static int Remove => Select(0x13C60, 0x16E70, 0x17530);
    }

    public static class CVehicle
    {
        public static int HasDriver => Select(0xB0B90, 0xB6850, 0xB70E0);
        public static int IsOccupied => Select(0xB0BE0, 0xB68A0, 0xB7130);
        public static int SetInvulnerable => Select(0xB0C40, 0xB6900, 0xB7190);
        public static int SetLocked => Select(0xB0CE0, 0xB69A0, 0xB7230);
        public static int GetHealth => Select(0xB0D50, 0xB6A10, 0xB72A0);
        public static int SetHealth => Select(0xB0D70, 0xB6A30, 0xB72C0);
        public static int SetColor => Select(0xB0D90, 0xB6A50, 0xB72E0);
        public static int UpdateColor => Select(0xB0DE0, 0xB6AA0, 0xB7330);
        public static int EnableSiren => Select(0xB0FF0, 0xB6CB0, 0xB7540);
        public static int SirenEnabled => Select(0xB1010, 0xB6CD0, 0xB7560);
        public static int GetTrailer => Select(0xB1740, 0xB7400, 0xB7C90);
        public static int DoesExist => Select(0xB1BE0, 0xB78A0, 0xB8140);
        public static int SetLicensePlateText => Select(0xB1BF0, 0xB78B0, 0xB8150);
        public static int SetRotation => Select(0xB1C10, 0xB78D0, 0xB8170);
        public static int EnableEngine => Select(0xB2510, 0xB81D0, 0xB8A70);
    }

    public static class CPlayerPool
    {
        public const int MaxPlayers = 1004;
        public static int RemotePlayerInfoArray => Select(0x2E, 0x4, 0x1F8A);
        public static int ConnectionStateArray => Select(0xFDE, 0xFB4, 0x2A);
        public const int LocalPlayerPing = 0x2F14;
        public const int LocalPlayerScore = 0x2F18;
        public const int LocalPlayerId = 0x2F1C;
        public const int LocalPlayerName = 0x2F22;
        public const int LocalPlayerPointer = 0x2F3A;
        public const int PlayerInfoPing = 0x04;
        public const int PlayerInfoName = 0x0C;
        public const int PlayerInfoScore = 0x24;
        public const int PlayerInfoNpcFlag = 0x28;
        public static int IsConnected => Select(0x10B0, 0x10B0, 0x10B0);
        public static int GetPlayer => Select(0x10F0, 0x10F0, 0x10F0);
        public static int GetLocalPlayer => Select(0x1A30, 0x1A30, 0x1A40);
        public static int GetCount => Select(0x10520, 0x13670, 0x139F0);
        public static int Find => Select(0x10420, 0x13570, 0x138C0);
        public static int Create => Select(0x10D50, 0x13E80, 0x14250);
        public static int Delete => Select(0x10B90, 0x13CB0, 0x14090);
        public static int GetName => Select(0x13CE0, 0x16F00, 0x175C0);
        public static int GetScore => Select(0x6A190, 0x6E0E0, 0x6E850);
        public static int GetPing => Select(0x6A1C0, 0x6E110, 0x6E880);
        public static int GetLocalPlayerPing => Select(0x6A200, 0x6E150, 0x6E8C0);
    }

    public static class CScoreboard
    {
        public static int Instance => Select(0x21A0B4, 0x26E894, 0x26EB4C);
        public const int Enabled = 0x00;
        public static int UpdateScoresPingsIps => Select(0xD490, 0x10090, 0x103E0);
    }

    // SAMP-API commit 0d43a3603239f2f4bc65b8305ffc72177386cc29, R3-1.
    public static class CGame
    {
        public static int Instance => Select(0x21A10C, 0x26E8F4, 0x26EBAC);
        // CGame::m_nCursorMode in the pinned packed x86 layout.
        public static int CursorMode => Select(0x55, 0x61, 0x61);
        public static int SetCursorMode => Select(0x9BD30, 0x9FFE0, 0xA06F0);
        public static int GetWindowHandle => Select(0x2D00, 0x2CF0, 0x2D10);
    }

    public static class CChat
    {
        public static int Instance => Select(0x21A0E4, 0x26E8C8, 0x26EB80);
        public static int AddEntry => Select(0x64010, 0x67460, 0x67BE0);
        public static int AddChatMessage => Select(0x64450, 0x678A0, 0x68020);
        public static int AddMessage => Select(0x645A0, 0x679F0, 0x68170);

        public const int Entries = 0x0132;
        public const int Redraw = 0x63DA;
        public const int Size = 25578;

        public const int EntryCount = 100;
        public const int EntrySize = 252;

        public const int Entry_Timestamp = 0x00;
        public const int Entry_Prefix = 0x04;
        public const int Entry_PrefixSize = 28;
        public const int Entry_Text = 0x20;
        public const int Entry_TextSize = 144;
        public const int Entry_Type = 0xF0;
        public const int Entry_TextColor = 0xF4;
        public const int Entry_PrefixColor = 0xF8;
    }

    public static class RakClientVTable
    {
        public const int EntryCount = 55;
        public const int Send_BitStream = 6;
        public const int Send_Data = 7;
        public const int Receive = 8;
    }

    public static class RakPeer
    {
        // Packet* __cdecl AllocPacket(unsigned int dataSize)
        public static int AllocPacket => Select(0x347E0, 0x37B90, 0x382D0);

        // Offset within RakPeer struct to the packet SingleProducerConsumer queue.
        public const int OffsetPackets = 0xDB6;

        // Packet** __thiscall WriteLock(void* queue) — acquires write slot.
        public static int WriteLock => Select(0x35B10, 0x38EC0, 0x39600);

        // void __thiscall WriteUnlock(void* queue) — commits the write.
        public static int WriteUnlock => Select(0x35B50, 0x38F00, 0x39640);
    }

    public static class RakClientRuntime
    {
        /// <summary>Native RakClient entry, before any third-party proxy processing.</summary>
        public static int SendBitStream => Select(r1: 0x307F0, r3: 0x33BA0, r5: 0x342E0);
        public static int SendData => Select(r1: 0x307A0, r3: 0x33B50, r5: 0x34290);
        public static int Receive => Select(r1: 0x31710, r3: 0x34AC0, r5: 0x35200);
    }

    public static class RpcRuntime
    {
        public static int HandleRpcPacket => Select(0x372F0, 0x3A6A0, 0x3ADE0);
        public static int SendRpcBitStream => Select(0x30B30, 0x33EE0, 0x34620);
    }

    public static class SampStringCompressor
    {
        public static int Instance => Select(0x50140, 0x534F0, 0x53C30);
        public static int DecodeString => Select(0x507E0, 0x53B90, 0x542D0);
    }

    public static class RakNetBitStream
    {
        public const int NumberOfBitsUsed = 0;
        public const int Data = 12;
    }

    public static class RakNetPacket
    {
        public const int Length = 8;
        public const int BitSize = 12;
        public const int Data = 16;
    }

    public static class RpcRoutes
    {
        public static class Chat
        {
            public static int AddChatMessage => CChat.AddChatMessage;
            public static int AddMessage => CChat.AddMessage;
        }

        public static class Dialog
        {
            public static int Show => CDialog.Show;
            public static int Close => CDialog.Close;
        }

        public static class Game
        {
            public static int DisplayGameText => Select(0x9C2C0, 0xA05D0, 0xA0CE0);
            public static int DeleteRacingCheckpoint => Select(0x9C310, 0xA0630, 0xA0D60);
            public static int SetCheckpoint => Select(0x9D340, 0xA16B0, 0xA1DE0);
            public static int SetRacingCheckpoint => Select(0x9D660, 0xA19D0, 0xA2100);
        }
    }
}
