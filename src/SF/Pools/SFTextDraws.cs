namespace SFSharp.Runtime.Game;

public sealed unsafe class SFTextDraws : ISFTextDraws
{
    public bool Exists(ushort textDrawId) => CTextDrawPool.Instance.DoesExist(textDrawId);

    public SFTextDraw Get(ushort textDrawId)
    {
        return new(textDrawId, CTextDrawPool.Instance.Get(textDrawId));
    }

    public bool TryGet(ushort textDrawId, out SFTextDraw textDraw)
    {
        if (!CTextDrawPool.Instance.TryGet(textDrawId, out CTextDraw* nativeTextDraw))
        {
            textDraw = null!;
            return false;
        }

        textDraw = new SFTextDraw(textDrawId, nativeTextDraw);
        return true;
    }

    public bool TryGetSnapshot(ushort textDrawId, out SFTextDrawSnapshot snapshot)
    {
        snapshot = default;
        return TryGet(textDrawId, out SFTextDraw textDraw) && textDraw.TryGetSnapshot(out snapshot);
    }

    public IEnumerable<ushort> EnumerateIds()
    {
        foreach (ushort textDrawId in CTextDrawPool.Instance.GetExistingIds())
        {
            yield return textDrawId;
        }
    }

    public IEnumerable<SFTextDraw> Enumerate()
    {
        foreach (ushort textDrawId in EnumerateIds())
        {
            if (TryGet(textDrawId, out SFTextDraw textDraw))
            {
                yield return textDraw;
            }
        }
    }

    public void Delete(ushort textDrawId)
    {
        CTextDrawPool.Instance.Delete(textDrawId);
    }

    public void Draw()
    {
        CTextDrawPool.Instance.Draw();
    }
}
