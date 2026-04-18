using System;
using System.Runtime.InteropServices;

using unsafe ChangeInteriorDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort, int, void>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort, int>;
using unsafe DoesExistDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort, int>;
using unsafe GetDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort, SFSharp.Runtime.Interop.Classes.Entities.CVehicle*>;
using unsafe GetNearestDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort>;
using unsafe ProcessDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, void>;
using unsafe SetParamsDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, ushort, int, int, void>;
using unsafe UpdateCountDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CVehiclePool*, void>;

namespace SFSharp.Runtime.Interop.Classes.Pools;

[StructLayout(LayoutKind.Explicit, Size = 96408, Pack = 1)]
public unsafe ref struct CVehiclePool
{
    private static CVehiclePool* CurrentInstance => CNetGame.Instance.GetVehiclePool();
    public static ref readonly CVehiclePool Instance => ref *RequireInstance();

    [FieldOffset(0x00)]
    private readonly int _count;

    public int Count => RequireInstance()->_count;

    private static readonly GetDelegate _get = (GetDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.Get);
    private static readonly DoesExistDelegate _doesExist = (DoesExistDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.DoesExist);
    private static readonly UpdateCountDelegate _updateCount = (UpdateCountDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.UpdateCount);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.Delete);
    private static readonly ChangeInteriorDelegate _changeInterior = (ChangeInteriorDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.ChangeInterior);
    private static readonly SetParamsDelegate _setParams = (SetParamsDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.SetParams);
    private static readonly GetNearestDelegate _getNearest = (GetNearestDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.GetNearest);
    private static readonly ProcessDelegate _process = (ProcessDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CVehiclePool.Process);

    public bool IsValidId(ushort vehicleId) => vehicleId < SampOffsets.CVehiclePool.MaxVehicles;

    public CVehicle* Get(ushort vehicleId)
    {
        return _get(RequireInstance(), vehicleId);
    }

    public bool DoesExist(ushort vehicleId)
    {
        return IsValidId(vehicleId) && _doesExist(RequireInstance(), vehicleId) != 0;
    }

    public bool TryGet(ushort vehicleId, out CVehicle* vehicle)
    {
        vehicle = Get(vehicleId);
        return vehicle != null;
    }

    public void UpdateCount()
    {
        _updateCount(RequireInstance());
    }

    public bool Delete(ushort vehicleId)
    {
        return _delete(RequireInstance(), vehicleId) != 0;
    }

    public void ChangeInterior(ushort vehicleId, int interiorId)
    {
        _changeInterior(RequireInstance(), vehicleId, interiorId);
    }

    public void SetParams(ushort vehicleId, bool isObjective, bool isLocked)
    {
        _setParams(RequireInstance(), vehicleId, isObjective ? 1 : 0, isLocked ? 1 : 0);
    }

    public ushort GetNearest()
    {
        return _getNearest(RequireInstance());
    }

    public ushort[] GetExistingIds()
    {
        List<ushort> ids = [];
        for (ushort vehicleId = 0; vehicleId < SampOffsets.CVehiclePool.MaxVehicles; vehicleId++)
        {
            if (DoesExist(vehicleId))
            {
                ids.Add(vehicleId);
            }
        }

        return [.. ids];
    }

    public void Process()
    {
        _process(RequireInstance());
    }

    private static CVehiclePool* RequireInstance()
    {
        CVehiclePool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CVehiclePool instance is not available.");
        }

        return instance;
    }
}
