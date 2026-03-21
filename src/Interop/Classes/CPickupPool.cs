using SFSharp;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, Pickup*, ushort, void>;
using unsafe CreateWeaponDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, int, System.Numerics.Vector3, int, ushort, void>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, int, void>;
using unsafe DeleteWeaponDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, ushort, void>;
using unsafe GetIndexDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, int, int>;
using unsafe ProcessDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, void>;
using unsafe SendNotificationDelegate = delegate* unmanaged[Thiscall]<CPickupPool*, int, void>;

[StructLayout(LayoutKind.Explicit, Size = 143364, Pack = 1)]
public unsafe ref struct CPickupPool
{
    private static CPickupPool* CurrentInstance => CNetGame.Instance.GetPickupPool();
    public static ref readonly CPickupPool Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CPickupPool.Count)]
    private readonly int _count;

    public int Count => RequireInstance()->_count;

    private static readonly CreateWeaponDelegate _createWeapon = (CreateWeaponDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.CreateWeapon);
    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.Create);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.Delete);
    private static readonly DeleteWeaponDelegate _deleteWeapon = (DeleteWeaponDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.DeleteWeapon);
    private static readonly GetIndexDelegate _getIndex = (GetIndexDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.GetIndex);
    private static readonly SendNotificationDelegate _sendNotification = (SendNotificationDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.SendNotification);
    private static readonly ProcessDelegate _process = (ProcessDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CPickupPool.Process);

    public bool IsValidIndex(int pickupIndex) => pickupIndex >= 0 && pickupIndex < SampOffsets.CPickupPool.MaxPickups;

    public ref readonly Pickup GetPickup(int pickupIndex)
    {
        CPickupPool* instance = RequireInstance();
        if (!IsValidIndex(pickupIndex))
        {
            throw new ArgumentOutOfRangeException(nameof(pickupIndex));
        }

        return ref GetPickupArray(instance)[pickupIndex];
    }

    public int GetServerId(int pickupIndex)
    {
        CPickupPool* instance = RequireInstance();
        if (!IsValidIndex(pickupIndex))
        {
            throw new ArgumentOutOfRangeException(nameof(pickupIndex));
        }

        return GetServerIdArray(instance)[pickupIndex];
    }

    public int GetIndex(int serverPickupId)
    {
        return _getIndex(RequireInstance(), serverPickupId);
    }

    public void Create(in Pickup pickup, ushort pickupId)
    {
        Pickup copy = pickup;
        _create(RequireInstance(), &copy, pickupId);
    }

    public void CreateWeapon(int modelId, Vector3 position, int ammo, ushort excludedOwnerId)
    {
        _createWeapon(RequireInstance(), modelId, position, ammo, excludedOwnerId);
    }

    public void Delete(int pickupIndex)
    {
        _delete(RequireInstance(), pickupIndex);
    }

    public void DeleteWeapon(ushort excludedOwnerId)
    {
        _deleteWeapon(RequireInstance(), excludedOwnerId);
    }

    public void SendNotification(int pickupIndex)
    {
        _sendNotification(RequireInstance(), pickupIndex);
    }

    public void Process()
    {
        _process(RequireInstance());
    }

    private static Pickup* GetPickupArray(CPickupPool* instance)
    {
        return (Pickup*)((byte*)instance + SampOffsets.CPickupPool.Pickups);
    }

    private static int* GetServerIdArray(CPickupPool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CPickupPool.ServerIds);
    }

    private static CPickupPool* RequireInstance()
    {
        CPickupPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CPickupPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Pickup
{
    public int Model;
    public int Type;
    public Vector3 Position;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct WeaponPickup
{
    public byte Exists;
    public ushort ExcludedOwnerId;
}
