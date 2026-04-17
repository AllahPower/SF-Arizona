using System;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CGangZonePool*, ushort, float, float, float, float, uint, void>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CGangZonePool*, ushort, void>;
using unsafe DrawDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CGangZonePool*, void>;
using unsafe StartFlashingDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CGangZonePool*, ushort, uint, void>;
using unsafe StopFlashingDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.CGangZonePool*, ushort, void>;

namespace SFSharp.Runtime.Interop;

[StructLayout(LayoutKind.Explicit, Size = 8192, Pack = 1)]
public unsafe ref struct CGangZonePool
{
    private static CGangZonePool* CurrentInstance => CNetGame.Instance.GetGangZonePool();
    public static ref readonly CGangZonePool Instance => ref *RequireInstance();

    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CGangZonePool.Create);
    private static readonly StartFlashingDelegate _startFlashing = (StartFlashingDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CGangZonePool.StartFlashing);
    private static readonly StopFlashingDelegate _stopFlashing = (StopFlashingDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CGangZonePool.StopFlashing);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CGangZonePool.Delete);
    private static readonly DrawDelegate _draw = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CGangZonePool.Draw);

    public bool IsValidId(ushort zoneId) => zoneId < SampOffsets.CGangZonePool.MaxGangZones;

    public GangZone* Get(ushort zoneId)
    {
        CGangZonePool* instance = RequireInstance();
        if (!IsValidId(zoneId))
        {
            return null;
        }

        return GetObjectArray(instance)[zoneId];
    }

    public bool DoesExist(ushort zoneId)
    {
        CGangZonePool* instance = RequireInstance();
        return IsValidId(zoneId) && GetOccupancyArray(instance)[zoneId] != 0;
    }

    public void Create(ushort zoneId, float left, float top, float right, float bottom, uint color)
    {
        _create(RequireInstance(), zoneId, left, top, right, bottom, color);
    }

    public void StartFlashing(ushort zoneId, uint color)
    {
        _startFlashing(RequireInstance(), zoneId, color);
    }

    public void StopFlashing(ushort zoneId)
    {
        _stopFlashing(RequireInstance(), zoneId);
    }

    public void Delete(ushort zoneId)
    {
        _delete(RequireInstance(), zoneId);
    }

    public unsafe ushort[] GetExistingIds()
    {
        CGangZonePool* instance = RequireInstance();
        int* occupancy = GetOccupancyArray(instance);
        List<ushort> ids = [];
        for (ushort zoneId = 0; zoneId < SampOffsets.CGangZonePool.MaxGangZones; zoneId++)
        {
            if (occupancy[zoneId] != 0)
            {
                ids.Add(zoneId);
            }
        }

        return [.. ids];
    }

    public void Draw()
    {
        _draw(RequireInstance());
    }

    private static GangZone** GetObjectArray(CGangZonePool* instance)
    {
        return (GangZone**)((byte*)instance + SampOffsets.CGangZonePool.ObjectArray);
    }

    private static int* GetOccupancyArray(CGangZonePool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CGangZonePool.OccupancyArray);
    }

    private static CGangZonePool* RequireInstance()
    {
        CGangZonePool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CGangZonePool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Sequential, Size = 24, Pack = 1)]
public struct GangZone
{
    public GangZoneRect Rect;
    public uint Color;
    public uint AltColor;

    public bool IsFlashing => AltColor != 0 && AltColor != Color;
}

[StructLayout(LayoutKind.Sequential, Size = 16, Pack = 1)]
public struct GangZoneRect
{
    public float Left;
    public float Bottom;
    public float Right;
    public float Top;
}
