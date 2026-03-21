using SFSharp;
using System;
using System.Numerics;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<CLabelPool*, ushort, byte*, uint, System.Numerics.Vector3, float, int, ushort, ushort, void>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<CLabelPool*, ushort, int>;

[StructLayout(LayoutKind.Explicit, Size = 67584, Pack = 1)]
public unsafe ref struct CLabelPool
{
    private static CLabelPool* CurrentInstance => CNetGame.Instance.GetLabelPool();
    public static ref readonly CLabelPool Instance => ref *RequireInstance();

    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLabelPool.Create);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLabelPool.Delete);

    public bool IsValidId(ushort labelId) => labelId < SampOffsets.CLabelPool.MaxLabels;

    public ref readonly TextLabel Get(ushort labelId)
    {
        CLabelPool* instance = RequireInstance();
        if (!IsValidId(labelId))
        {
            throw new ArgumentOutOfRangeException(nameof(labelId));
        }

        return ref GetObjectArray(instance)[labelId];
    }

    public bool DoesExist(ushort labelId)
    {
        CLabelPool* instance = RequireInstance();
        return IsValidId(labelId) && GetOccupancyArray(instance)[labelId] != 0;
    }

    public void Create(ushort labelId, string text, uint color, Vector3 position, float drawDistance, bool behindWalls, ushort attachedToPlayer = ushort.MaxValue, ushort attachedToVehicle = ushort.MaxValue)
    {
        using AnsiString textAnsi = AnsiString.Encode(text);
        _create(RequireInstance(), labelId, textAnsi, color, position, drawDistance, behindWalls ? 1 : 0, attachedToPlayer, attachedToVehicle);
    }

    public bool Delete(ushort labelId)
    {
        return _delete(RequireInstance(), labelId) != 0;
    }

    public unsafe ushort[] GetExistingIds()
    {
        CLabelPool* instance = RequireInstance();
        int* occupancy = GetOccupancyArray(instance);
        List<ushort> ids = [];
        for (ushort labelId = 0; labelId < SampOffsets.CLabelPool.MaxLabels; labelId++)
        {
            if (occupancy[labelId] != 0)
            {
                ids.Add(labelId);
            }
        }

        return [.. ids];
    }

    private static TextLabel* GetObjectArray(CLabelPool* instance)
    {
        return (TextLabel*)((byte*)instance + SampOffsets.CLabelPool.ObjectArray);
    }

    private static int* GetOccupancyArray(CLabelPool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CLabelPool.OccupancyArray);
    }

    private static CLabelPool* RequireInstance()
    {
        CLabelPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CLabelPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Sequential, Size = 29, Pack = 1)]
public unsafe struct TextLabel
{
    public byte* Text;
    public uint Color;
    public Vector3 Position;
    public float DrawDistance;
    public byte BehindWalls;
    public ushort AttachedToPlayer;
    public ushort AttachedToVehicle;
}

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
public unsafe struct CLabel;
