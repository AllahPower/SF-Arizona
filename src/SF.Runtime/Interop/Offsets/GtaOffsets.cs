namespace SFSharp.Runtime.Interop.Offsets;

public static class GtaOffsets
{
    // GTA SA 1.0 US / current Arizona client. RVAs are relative to the EXE base,
    // unlike the legacy absolute camera addresses below. Device/HWND verified by
    // read-only process inspection on 2026-09-29; function identities checked in IDA.
    public static class RenderWareRva
    {
        public const int D3D9Device = 0x897C28;
        public const int WindowHandle = 0x897C1C;
        public const int RasterShowRaster = 0x3F99B0;
        public const int CameraBeginUpdate = 0x3F8F20;
        public const int CameraEndUpdate = 0x3F98D0;
    }

    public static class InputRva
    {
        public const int MainWndProc = 0x347EB0;
        public const int UpdateMouse = 0x13F3C0;
        public const int GetMouseState = 0x346ED0;
    }

    public static class CCamera
    {
        public const nint TheCamera = 0xB6F028;
        public const int Size = 0xD78;

        public const int WideScreenOn = 0x70;
        public const int ShakeForce = 0x74;
        public const int FadeAlpha = 0x7C;
        public const int FadeState = 0x7E;

        public const int ActiveCam = 0x174;
    }

    public static class CCam
    {
        public const int Size = 0x238;
        public const int Mode = 0x00C;
        public const int Fov = 0x040;
        public const int Source = 0x0F0;
        public const int Front = 0x138;
        public const int Up = 0x168;
    }

    // Entity model below: GTA SA 1.0 US gta_sa.exe MD5 84a781f30fac7eba030af2497da86930,
    // layouts and functions verified in IDA on 2026-10-02. Function and global values are RVAs.
    public static class CPoolsRva
    {
        public const int PedPool = 0x774490;
        public const int VehiclePool = 0x774494;
        public const int ObjectPool = 0x77449C;

        public const int GetPed = 0x14FF90;
        public const int GetPedRef = 0x14FF60;
        public const int GetVehicle = 0x14FFF0;
        public const int GetVehicleRef = 0x14FFC0;
        public const int GetObject = 0x150050;
        public const int GetObjectRef = 0x150020;
    }

    public static class CPool
    {
        public const int Size = 0x14;
        public const int Objects = 0x00;
        public const int ByteMap = 0x04;
        public const int Capacity = 0x08;
        public const byte FreeSlotFlag = 0x80;
    }

    public static class CPlaceable
    {
        public const int Size = 0x18;
        public const int VTable = 0x00;
        public const int SimplePosition = 0x04;
        public const int SimpleAngle = 0x10;
        public const int Matrix = 0x14;

        public const int SetHeadingRva = 0x03E0C0;
    }

    public static class CMatrix
    {
        public const int Size = 0x48;
        public const int Right = 0x00;
        public const int Up = 0x10;
        public const int At = 0x20;
        public const int Position = 0x30;
    }

    public static class CEntity
    {
        public const int Size = 0x38;
        public const int RwObject = 0x18;
        public const int Flags = 0x1C;
        public const int ModelIndex = 0x22;
        public const int Interior = 0x2F;
        public const int TypeStatus = 0x36;

        // CEntity::Teleport(CVector, bool), overridden by CPed (0x5E4110) and CAutomobile (0x6A9CA0).
        public const int TeleportVTableIndex = 14;
    }

    public static class CPhysical
    {
        public const int Size = 0x138;
        public const int MoveSpeed = 0x44;
        public const int TurnSpeed = 0x50;
        public const int Mass = 0x8C;
        public const int TurnMass = 0x90;
        public const int AttachedTo = 0xFC;
    }

    public static class CPed
    {
        public const int Size = 0x79C;
        public const int Flags = 0x46C;
        public const uint InVehicleFlag = 0x100;
        public const int PedState = 0x530;
        public const int Health = 0x540;
        public const int MaxHealth = 0x544;
        public const int Armour = 0x548;
        public const int CurrentRotation = 0x558;
        public const int AimingRotation = 0x55C;
        public const int Vehicle = 0x58C;
        public const int ActiveWeaponSlot = 0x718;
    }

    public static class CVehicle
    {
        public const int Size = 0x5A0;
        public const int PrimaryColor = 0x434;
        public const int SecondaryColor = 0x435;
        public const int Driver = 0x460;
        public const int Passengers = 0x464;
        public const int PassengerSlots = 8;
        public const int MaxPassengers = 0x488;
        public const int Health = 0x4C0;
        public const int DoorLock = 0x4F8;
    }

    public static class CObject
    {
        public const int Size = 0x17C;
        public const int ObjectType = 0x13C;
        public const int Health = 0x154;
    }

    public static class DamageRva
    {
        // CPedDamageResponseCalculator::ComputeDamageResponse(CPed*, CPedDamageResponse&, bool), thiscall.
        public const int ComputePedDamageResponse = 0x0B5AC0;
        // CVehicle::InflictDamage(CEntity*, eWeaponType, float, CVector), thiscall.
        public const int InflictVehicleDamage = 0x2D7C90;
        // CObject::ObjectDamage(float, CVector*, CVector*, CEntity*, eWeaponType), thiscall.
        public const int ObjectDamage = 0x1A0D90;
    }

    public static class CPedDamageResponseCalculator
    {
        public const int Damager = 0x00;
        public const int DamageFactor = 0x04;
        public const int BodyPart = 0x08;
        public const int WeaponType = 0x0C;
    }
}
