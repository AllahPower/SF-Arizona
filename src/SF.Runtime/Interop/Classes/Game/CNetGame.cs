using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace SFSharp.Runtime.Interop.Classes.Game;

using unsafe GetActorPoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CActorPool*>;
using unsafe GetCounterDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, long>;
using unsafe GetMenuPoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*>;
using unsafe GetObjectPoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CObjectPool*>;
using unsafe GetPickupPoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CPickupPool*>;
using unsafe GetPlayerPoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CPlayerPool*>;
using unsafe GetStateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, int>;
using unsafe GetVehiclePoolDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*>;
using unsafe InitializeDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, void>;
using unsafe LanModeDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, int>;
using unsafe SetStateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, int, void>;
using unsafe UpdatePlayersDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Game.CNetGame*, void>;


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
    private readonly CRakClientInterface* _rakClient;

    [FieldOffset(0)]
    private readonly CRakClientInterface* _rakClientR5;

    private CRakClientInterface* Client => SampEnvironment.IsR5 ? _rakClientR5 : _rakClient;

    public int State => RequireInstance()->_state;
    public CNetGamePools* Pools => RequireInstance()->_pools;
    public bool HasPools => RequireInstance()->_pools != null;
    public CRakClientInterface* RakClient => RequireInstance()->Client;

    private static readonly GetPlayerPoolDelegate _getPlayerPool = (GetPlayerPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetPlayerPool);
    private static readonly GetVehiclePoolDelegate _getVehiclePool = (GetVehiclePoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetVehiclePool);
    private static readonly GetObjectPoolDelegate _getObjectPool = (GetObjectPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetObjectPool);
    private static readonly GetActorPoolDelegate _getActorPool = (GetActorPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetActorPool);
    private static readonly GetPickupPoolDelegate _getPickupPool = (GetPickupPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetPickupPool);
    private static readonly GetMenuPoolDelegate _getMenuPool = (GetMenuPoolDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetMenuPool);
    private static readonly GetStateDelegate _getState = (GetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetState);
    private static readonly LanModeDelegate _lanMode = (LanModeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.LanMode);
    private static readonly delegate* unmanaged[Thiscall]<CNetGame*, CRakClientInterface*> _getRakClient =
        (delegate* unmanaged[Thiscall]<CNetGame*, CRakClientInterface*>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetRakClient);
    private static readonly GetCounterDelegate _getCounter = (GetCounterDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.GetCounter);
    private static readonly SetStateDelegate _setState = (SetStateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.SetState);
    private static readonly InitializeDelegate _initializePools = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.InitializePools);
    private static readonly InitializeDelegate _initialNotification = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.InitialNotification);
    private static readonly InitializeDelegate _initializeGameLogic = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.InitializeGameLogic);
    private static readonly InitializeDelegate _connect = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.Connect);
    private static readonly InitializeDelegate _spawnScreen = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.SpawnScreen);
    private static readonly UpdatePlayersDelegate _updatePlayers = (UpdatePlayersDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.UpdatePlayers);
    private static readonly InitializeDelegate _resetPlayerPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetPlayerPool);
    private static readonly InitializeDelegate _resetVehiclePool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetVehiclePool);
    private static readonly InitializeDelegate _resetTextDrawPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetTextDrawPool);
    private static readonly InitializeDelegate _resetObjectPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetObjectPool);
    private static readonly InitializeDelegate _resetGangZonePool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetGangZonePool);
    private static readonly InitializeDelegate _resetPickupPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetPickupPool);
    private static readonly InitializeDelegate _resetMenuPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetMenuPool);
    private static readonly InitializeDelegate _resetLabelPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetLabelPool);
    private static readonly InitializeDelegate _resetActorPool = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetActorPool);
    private static readonly InitializeDelegate _resetMarkers = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetMarkers);
    private static readonly InitializeDelegate _resetPools = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ResetPools);
    private static readonly InitializeDelegate _shutdownForRestart = (InitializeDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CNetGame.ShutdownForRestart);

    public static bool TryGetInstance(out CNetGame* instance)
    {
        instance = CurrentInstance;
        return instance != null;
    }

    public static bool TryGetPools(out CNetGamePools* pools)
    {
        if (!TryGetInstance(out CNetGame* instance))
        {
            pools = null;
            return false;
        }

        pools = instance->_pools;
        return pools != null;
    }

    public static nint GetRakClientPtr()
    {
        return (nint)GetRakClient();
    }

    public static nint GetRakClientPtr(nint cnetGamePtr)
    {
        if (cnetGamePtr == 0)
        {
            return 0;
        }

        return *(nint*)(cnetGamePtr + SampOffsets.Select(SampOffsets.CNetGame.RakClient, 0));
    }

    public static CRakClientInterface* GetRakClient()
    {
        return RequireInstance()->Client;
    }

    public static bool TryGetRakClient(out CRakClientInterface* rakClient)
    {
        if (!TryGetInstance(out CNetGame* instance))
        {
            rakClient = null;
            return false;
        }

        rakClient = instance->Client;
        return rakClient != null;
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

    public CGangZonePool* GetGangZonePool()
    {
        CNetGamePools* pools = Pools;
        return pools is null ? null : pools->GangZone;
    }

    public CLabelPool* GetLabelPool()
    {
        CNetGamePools* pools = Pools;
        return pools is null ? null : pools->Label;
    }

    public CMenuPool* GetMenuPool()
    {
        return _getMenuPool(RequireInstance());
    }

    public CTextDrawPool* GetTextDrawPool()
    {
        CNetGamePools* pools = Pools;
        return pools is null ? null : pools->TextDraw;
    }

    public bool IsLanMode()
    {
        return _lanMode(RequireInstance()) != 0;
    }

    public CRakClientInterface* GetRakClientInstance()
    {
        return _getRakClient(RequireInstance());
    }

    public long GetCounter()
    {
        return _getCounter(RequireInstance());
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

    public void InitializePools()
    {
        _initializePools(RequireInstance());
    }

    public void InitialNotification()
    {
        _initialNotification(RequireInstance());
    }

    public void InitializeGameLogic()
    {
        _initializeGameLogic(RequireInstance());
    }

    public void Connect()
    {
        _connect(RequireInstance());
    }

    public void SpawnScreen()
    {
        _spawnScreen(RequireInstance());
    }

    public void ResetPlayerPool()
    {
        _resetPlayerPool(RequireInstance());
    }

    public void ResetVehiclePool()
    {
        _resetVehiclePool(RequireInstance());
    }

    public void ResetTextDrawPool()
    {
        _resetTextDrawPool(RequireInstance());
    }

    public void ResetObjectPool()
    {
        _resetObjectPool(RequireInstance());
    }

    public void ResetGangZonePool()
    {
        _resetGangZonePool(RequireInstance());
    }

    public void ResetPickupPool()
    {
        _resetPickupPool(RequireInstance());
    }

    public void ResetMenuPool()
    {
        _resetMenuPool(RequireInstance());
    }

    public void ResetLabelPool()
    {
        _resetLabelPool(RequireInstance());
    }

    public void ResetActorPool()
    {
        _resetActorPool(RequireInstance());
    }

    public void ResetMarkers()
    {
        _resetMarkers(RequireInstance());
    }

    public void ResetPools()
    {
        _resetPools(RequireInstance());
    }

    public void ShutdownForRestart()
    {
        _shutdownForRestart(RequireInstance());
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
    private CMenuPool* _menu;

    public CMenuPool* Menu => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Menu : _menu;

    [FieldOffset(0x04)]
    private CActorPool* _actor;

    public CActorPool* Actor => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Actor : _actor;

    [FieldOffset(0x08)]
    private CPlayerPool* _player;

    public CPlayerPool* Player => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Player : _player;

    [FieldOffset(0x0C)]
    private CVehiclePool* _vehicle;

    public CVehiclePool* Vehicle => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Vehicle : _vehicle;

    [FieldOffset(0x10)]
    private CPickupPool* _pickup;

    public CPickupPool* Pickup => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Pickup : _pickup;

    [FieldOffset(0x14)]
    private CObjectPool* _object;

    public CObjectPool* Object => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Object : _object;

    [FieldOffset(0x18)]
    private CGangZonePool* _gangZone;

    public CGangZonePool* GangZone => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->GangZone : _gangZone;

    [FieldOffset(0x1C)]
    private CLabelPool* _label;

    public CLabelPool* Label => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->Label : _label;

    [FieldOffset(0x20)]
    private CTextDrawPool* _textDraw;

    public CTextDrawPool* TextDraw => SampEnvironment.IsR5
        ? ((NetGamePoolsR5*)Unsafe.AsPointer(ref this))->TextDraw : _textDraw;

    public bool IsAvailable =>
        Menu != null ||
        Actor != null ||
        Player != null ||
        Vehicle != null ||
        Pickup != null ||
        Object != null ||
        GangZone != null ||
        Label != null ||
        TextDraw != null;

    public int GetInitializedCount()
    {
        int count = 0;
        if (Menu != null) count++;
        if (Actor != null) count++;
        if (Player != null) count++;
        if (Vehicle != null) count++;
        if (Pickup != null) count++;
        if (Object != null) count++;
        if (GangZone != null) count++;
        if (Label != null) count++;
        if (TextDraw != null) count++;
        return count;
    }
}
