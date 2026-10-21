namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFTextDraws(ISFMainThread mainThread) : ISFTextDraws
{
    private ref readonly CTextDrawPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP text draws require the game thread.");
            return ref CTextDrawPool.Instance;
        }
    }

    public bool Exists(ushort textDrawId) => Pool.DoesExist(textDrawId);

    public SFTextDraw Get(ushort textDrawId)
    {
        return new(textDrawId, Pool.Get(textDrawId));
    }

    public bool TryGet(ushort textDrawId, out SFTextDraw textDraw)
    {
        if (!Pool.TryGet(textDrawId, out CTextDraw* nativeTextDraw))
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
        return Pool.GetExistingIds().ToArray();
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
        Pool.Delete(textDrawId);
    }

    public void Draw()
    {
        Pool.Draw();
    }
}
