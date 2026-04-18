namespace SFSharp.Runtime.Game.World;

public readonly unsafe struct SFGangZone(ushort id)
{
    public ushort Id => id;
    public bool Exists => CGangZonePool.Instance.DoesExist(id);
    public GangZoneRect Rect => CGangZonePool.Instance.Get(id)->Rect;
    public uint Color => CGangZonePool.Instance.Get(id)->Color;
    public uint AltColor => CGangZonePool.Instance.Get(id)->AltColor;
    public bool IsFlashing => CGangZonePool.Instance.Get(id)->IsFlashing;
}
