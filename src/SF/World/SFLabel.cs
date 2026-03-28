using System.Numerics;

namespace SFSharp;

public readonly unsafe struct SFLabel(ushort id)
{
    public ushort Id => id;
    public bool Exists => CLabelPool.Instance.DoesExist(id);
    public uint Color => CLabelPool.Instance.Get(id).Color;
    public Vector3 Position => CLabelPool.Instance.Get(id).Position;
    public float DrawDistance => CLabelPool.Instance.Get(id).DrawDistance;
    public bool BehindWalls => CLabelPool.Instance.Get(id).BehindWalls != 0;
    public ushort AttachedToPlayer => CLabelPool.Instance.Get(id).AttachedToPlayer;
    public ushort AttachedToVehicle => CLabelPool.Instance.Get(id).AttachedToVehicle;
    public string? Text => CLabelPool.Instance.Get(id).GetText();
}
