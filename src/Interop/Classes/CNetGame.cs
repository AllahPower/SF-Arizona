using SFSharp;
using System;
using System.Runtime.InteropServices;

using unsafe GetActorPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CActorPool*>;
using unsafe GetMenuPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CMenuPool*>;
using unsafe GetObjectPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CObjectPool*>;
using unsafe GetPickupPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CPickupPool*>;
using unsafe GetPlayerPoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CPlayerPool*>;
using unsafe GetStateDelegate = delegate* unmanaged[Thiscall]<CNetGame*, int>;
using unsafe GetVehiclePoolDelegate = delegate* unmanaged[Thiscall]<CNetGame*, CVehiclePool*>;
using unsafe SetStateDelegate = delegate* unmanaged[Thiscall]<CNetGame*, int, void>;
using unsafe UpdatePlayersDelegate = delegate* unmanaged[Thiscall]<CNetGame*, void>;

[StructLayout(LayoutKind.Explicit, Size = 994, Pack = 1)]
public unsafe ref struct CNetGame
{
    private static readonly nuint _instanceAddress = (nuint)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.Instance);
    private static CNetGame* CurrentInstance => *(CNetGame**)_instanceAddress;
    public static ref readonly CNetGame Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CNetGame.State)]
    private readonly int _state;

    [FieldOffset(SampOffsets.CNetGame.Pools)]
    private readonly CNetGamePools* _pools;

    [FieldOffset(SampOffsets.CNetGame.RakClient)]
    private readonly nint _rakClient;

    public int State => RequireInstance()->_state;
    public CNetGamePools* Pools => RequireInstance()->_pools;

    private static readonly GetPlayerPoolDelegate _getPlayerPool = (GetPlayerPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetPlayerPool);
    private static readonly GetVehiclePoolDelegate _getVehiclePool = (GetVehiclePoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetVehiclePool);
    private static readonly GetObjectPoolDelegate _getObjectPool = (GetObjectPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetObjectPool);
    private static readonly GetActorPoolDelegate _getActorPool = (GetActorPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetActorPool);
    private static readonly GetPickupPoolDelegate _getPickupPool = (GetPickupPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetPickupPool);
    private static readonly GetMenuPoolDelegate _getMenuPool = (GetMenuPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetMenuPool);
    private static readonly GetStateDelegate _getState = (GetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetState);
    private static readonly SetStateDelegate _setState = (SetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.SetState);
    private static readonly UpdatePlayersDelegate _updatePlayers = (UpdatePlayersDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.UpdatePlayers);

    public static nint GetRakClientPtr()
    {
        return GetRakClientPtr((nint)RequireInstance());
    }

    public static nint GetRakClientPtr(nint cnetGamePtr)
    {
        if (cnetGamePtr == 0)
        {
            return 0;
        }

        return *(nint*)(cnetGamePtr + SampOffsets.CNetGame.RakClient);
    }

    public CPlayerPool* GetPlayerPool()
    {
        return _getPlayerPool(RequireInstance());
    }

    public CVehiclePool* GetVehiclePool()
    {
        return _getVehiclePool(RequireInstance());
    }

    public CObjectPool* GetObjectPool()
    {
        return _getObjectPool(RequireInstance());
    }

    public CActorPool* GetActorPool()
    {
        return _getActorPool(RequireInstance());
    }

    public CPickupPool* GetPickupPool()
    {
        return _getPickupPool(RequireInstance());
    }

    public CMenuPool* GetMenuPool()
    {
        return _getMenuPool(RequireInstance());
    }

    public int GetState()
    {
        return _getState(RequireInstance());
    }

    public void SetState(int state)
    {
        _setState(RequireInstance(), state);
    }

    public void UpdatePlayers()
    {
        _updatePlayers(RequireInstance());
    }

    private static CNetGame* RequireInstance()
    {
        CNetGame* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CNetGame instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 36, Pack = 1)]
public unsafe struct CNetGamePools
{
    [FieldOffset(0x00)]
    public CMenuPool* Menu;

    [FieldOffset(0x04)]
    public CActorPool* Actor;

    [FieldOffset(0x08)]
    public CPlayerPool* Player;

    [FieldOffset(0x0C)]
    public CVehiclePool* Vehicle;

    [FieldOffset(0x10)]
    public CPickupPool* Pickup;

    [FieldOffset(0x14)]
    public CObjectPool* Object;

    [FieldOffset(0x18)]
    public CTextDrawPool* TextDraw;

    [FieldOffset(0x1C)]
    public CGangZonePool* GangZone;

    [FieldOffset(0x20)]
    public CLabelPool* Label;
}

public unsafe struct CActorPool;
public unsafe struct CMenuPool;
public unsafe struct CObjectPool;
public unsafe struct CPickupPool;
public unsafe struct CVehiclePool;
public unsafe struct CTextDrawPool;
public unsafe struct CGangZonePool;
public unsafe struct CLabelPool;
