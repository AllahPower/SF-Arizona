namespace SFSharp.Runtime.Interop.Engine;

/// <summary>
/// Access to the GTA ped, vehicle and object pools through <c>CPools</c>. Pointers are resolved by the
/// game's own getters so slot sizes never have to be hard-coded. Main thread only.
/// </summary>
public static unsafe class GtaPools
{
    private static readonly delegate* unmanaged[Cdecl]<int, GtaPed*> _getPed = (delegate* unmanaged[Cdecl]<int, GtaPed*>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetPed);
    private static readonly delegate* unmanaged[Cdecl]<GtaPed*, int> _getPedRef = (delegate* unmanaged[Cdecl]<GtaPed*, int>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetPedRef);
    private static readonly delegate* unmanaged[Cdecl]<int, GtaVehicle*> _getVehicle = (delegate* unmanaged[Cdecl]<int, GtaVehicle*>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetVehicle);
    private static readonly delegate* unmanaged[Cdecl]<GtaVehicle*, int> _getVehicleRef = (delegate* unmanaged[Cdecl]<GtaVehicle*, int>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetVehicleRef);
    private static readonly delegate* unmanaged[Cdecl]<int, GtaObject*> _getObject = (delegate* unmanaged[Cdecl]<int, GtaObject*>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetObject);
    private static readonly delegate* unmanaged[Cdecl]<GtaObject*, int> _getObjectRef = (delegate* unmanaged[Cdecl]<GtaObject*, int>)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.GetObjectRef);

    public static GtaPool* PedPool => *(GtaPool**)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.PedPool);
    public static GtaPool* VehiclePool => *(GtaPool**)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.VehiclePool);
    public static GtaPool* ObjectPool => *(GtaPool**)ModuleResolver.GetGameAddress(GtaOffsets.CPoolsRva.ObjectPool);

    public static GtaPed* GetPed(int handle) => IsValidHandle(PedPool, handle) ? _getPed(handle) : null;
    public static GtaVehicle* GetVehicle(int handle) => IsValidHandle(VehiclePool, handle) ? _getVehicle(handle) : null;
    public static GtaObject* GetObject(int handle) => IsValidHandle(ObjectPool, handle) ? _getObject(handle) : null;

    public static int GetPedHandle(GtaPed* ped) => ped is null ? -1 : _getPedRef(ped);
    public static int GetVehicleHandle(GtaVehicle* vehicle) => vehicle is null ? -1 : _getVehicleRef(vehicle);
    public static int GetObjectHandle(GtaObject* obj) => obj is null ? -1 : _getObjectRef(obj);

    /// <summary>Handles of every occupied slot, in slot order.</summary>
    public static List<int> GetOccupiedHandles(GtaPool* pool)
    {
        List<int> handles = [];
        if (pool is null || pool->ByteMap is null)
        {
            return handles;
        }

        for (int slot = 0; slot < pool->Capacity; slot++)
        {
            byte entry = pool->ByteMap[slot];
            if (!GtaPoolHandle.IsFree(entry))
            {
                handles.Add(GtaPoolHandle.Compose(slot, entry));
            }
        }

        return handles;
    }

    private static bool IsValidHandle(GtaPool* pool, int handle)
    {
        return pool is not null && GtaPoolHandle.IsInRange(handle, pool->Capacity);
    }
}
