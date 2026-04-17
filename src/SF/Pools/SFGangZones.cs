namespace SFSharp.Runtime.Game;

public sealed class SFGangZones : ISFGangZones
{
    public bool Exists(ushort zoneId) => CGangZonePool.Instance.DoesExist(zoneId);

    public SFGangZone Get(ushort zoneId)
    {
        return new(zoneId);
    }

    public bool TryGet(ushort zoneId, out SFGangZone zone)
    {
        if (!Exists(zoneId))
        {
            zone = default;
            return false;
        }

        zone = new SFGangZone(zoneId);
        return true;
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort zoneId in CGangZonePool.Instance.GetExistingIds())
        {
            yield return zoneId;
        }
    }

    public IEnumerable<SFGangZone> Enumerate()
    {
        foreach (ushort zoneId in EnumerateIds())
        {
            yield return new SFGangZone(zoneId);
        }
    }

    public void Create(ushort zoneId, float left, float top, float right, float bottom, uint color)
    {
        CGangZonePool.Instance.Create(zoneId, left, top, right, bottom, color);
    }

    public void StartFlashing(ushort zoneId, uint color)
    {
        CGangZonePool.Instance.StartFlashing(zoneId, color);
    }

    public void StopFlashing(ushort zoneId)
    {
        CGangZonePool.Instance.StopFlashing(zoneId);
    }

    public void Delete(ushort zoneId)
    {
        CGangZonePool.Instance.Delete(zoneId);
    }

    public void Draw()
    {
        CGangZonePool.Instance.Draw();
    }
}
