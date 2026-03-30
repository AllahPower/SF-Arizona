using System.Numerics;

namespace SFSharp;

public readonly unsafe struct SFLabel(ushort id)
{
    public ushort Id => id;
    public bool Exists => CLabelPool.Instance.DoesExist(id);
    public uint Color => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.Color : 0u;
    public Vector3 Position => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.Position : Vector3.Zero;
    public float DrawDistance => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.DrawDistance : 0f;
    public bool BehindWalls => TryGetSnapshot(out SFLabelSnapshot snapshot) && snapshot.BehindWalls;
    public ushort AttachedToPlayer => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.AttachedToPlayer : ushort.MaxValue;
    public ushort AttachedToVehicle => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.AttachedToVehicle : ushort.MaxValue;
    public string? Text => TryGetSnapshot(out SFLabelSnapshot snapshot) ? snapshot.Text : null;

    public bool TryGetSnapshot(out SFLabelSnapshot snapshot)
    {
        if (!CLabelPool.Instance.TryGet(id, out TextLabel label))
        {
            snapshot = default;
            return false;
        }

        snapshot = new SFLabelSnapshot(
            label.GetText(),
            label.Color,
            label.Position,
            label.DrawDistance,
            label.BehindWalls != 0,
            label.AttachedToPlayer,
            label.AttachedToVehicle);
        return true;
    }
}

public readonly record struct SFLabelSnapshot(
    string? Text,
    uint Color,
    Vector3 Position,
    float DrawDistance,
    bool BehindWalls,
    ushort AttachedToPlayer,
    ushort AttachedToVehicle);
