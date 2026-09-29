using System.Numerics;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CLabelPool*, ushort, byte*, uint, System.Numerics.Vector3, float, int, ushort, ushort, void>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CLabelPool*, ushort, int>;
using unsafe DrawDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CLabelPool*, void>;

namespace SFSharp.Runtime.Interop.Classes.Pools;

[StructLayout(LayoutKind.Explicit, Size = 67584, Pack = 1)]
public unsafe ref struct CLabelPool
{
    private static CLabelPool* CurrentInstance => CNetGame.Instance.GetLabelPool();
    public static ref readonly CLabelPool Instance => ref *RequireInstance();

    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLabelPool.Create);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLabelPool.Delete);
    private static readonly DrawDelegate _draw = (DrawDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CLabelPool.Draw);

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

    public bool TryGet(ushort labelId, out TextLabel label)
    {
        CLabelPool* instance = RequireInstance();
        if (!IsValidId(labelId) || GetOccupancyArray(instance)[labelId] == 0)
        {
            label = default;
            return false;
        }

        label = GetObjectArray(instance)[labelId];
        return true;
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

    public string? GetText(ushort labelId)
    {
        return Get(labelId).GetText();
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

    public void Draw()
    {
        _draw(RequireInstance());
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

    public string? GetText()
    {
        return NativeString.Decode(Text, 400);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
public unsafe struct CLabel;
