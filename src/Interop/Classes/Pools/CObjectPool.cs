using System;
using System.Numerics;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, ushort, int, System.Numerics.Vector3, System.Numerics.Vector3, float, int>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, ushort, int>;
using unsafe DrawDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, void>;
using unsafe GetCountDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, int>;
using unsafe GetDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, ushort, nint>;
using unsafe GetIdDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, nint, int>;
using unsafe ProcessDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CObjectPool*, void>;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 8004, Pack = 1)]
public unsafe ref struct CObjectPool
{
    private static CObjectPool* CurrentInstance => CNetGame.Instance.GetObjectPool();
    public static ref readonly CObjectPool Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CObjectPool.LargestId)]
    private readonly int _largestId;

    public int LargestId => RequireInstance()->_largestId;

    private static readonly GetDelegate _get = (GetDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Get);
    private static readonly GetCountDelegate _getCount = (GetCountDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.GetCount);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Delete);
    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Create);
    private static readonly GetIdDelegate _getId = (GetIdDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.GetId);
    private static readonly ProcessDelegate _process = (ProcessDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Process);
    private static readonly DrawDelegate _constructMaterials = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.ConstructMaterials);
    private static readonly DrawDelegate _shutdownMaterials = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.ShutdownMaterials);
    private static readonly DrawDelegate _draw = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.Draw);
    private static readonly DrawDelegate _drawLast = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CObjectPool.DrawLast);

    public bool IsValidId(ushort objectId) => objectId < SampOffsets.CObjectPool.MaxObjects;

    public CObject* Get(ushort objectId)
    {
        return (CObject*)_get(RequireInstance(), objectId);
    }

    public bool IsAllocated(ushort objectId)
    {
        CObjectPool* instance = RequireInstance();
        if (!IsValidId(objectId))
        {
            return false;
        }

        return GetOccupancyPtr(instance)[objectId] != 0;
    }

    public bool TryGet(ushort objectId, out CObject* obj)
    {
        obj = Get(objectId);
        return obj != null;
    }

    public int GetCount()
    {
        return _getCount(RequireInstance());
    }

    public bool Delete(ushort objectId)
    {
        return _delete(RequireInstance(), objectId) != 0;
    }

    public bool Create(ushort objectId, int modelId, Vector3 position, Vector3 rotation, float drawDistance)
    {
        return _create(RequireInstance(), objectId, modelId, position, rotation, drawDistance) != 0;
    }

    public int GetId(CObject* obj)
    {
        return _getId(RequireInstance(), (nint)obj);
    }

    public unsafe ushort[] GetAllocatedIds()
    {
        CObjectPool* instance = RequireInstance();
        int* occupancy = GetOccupancyPtr(instance);
        List<ushort> ids = [];
        for (ushort objectId = 0; objectId < SampOffsets.CObjectPool.MaxObjects; objectId++)
        {
            if (occupancy[objectId] != 0)
            {
                ids.Add(objectId);
            }
        }

        return [.. ids];
    }

    public void Process()
    {
        _process(RequireInstance());
    }

    public void ConstructMaterials()
    {
        _constructMaterials(RequireInstance());
    }

    public void ShutdownMaterials()
    {
        _shutdownMaterials(RequireInstance());
    }

    public void Draw()
    {
        _draw(RequireInstance());
    }

    public void DrawLast()
    {
        _drawLast(RequireInstance());
    }

    private static int* GetOccupancyPtr(CObjectPool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CObjectPool.OccupancyArray);
    }

    private static CObjectPool* RequireInstance()
    {
        CObjectPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CObjectPool instance is not available.");
        }

        return instance;
    }
}
