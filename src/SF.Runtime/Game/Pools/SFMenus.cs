namespace SFSharp.Runtime.Game.Pools;

public sealed unsafe class SFMenus(ISFMainThread mainThread) : ISFMenus
{
    private ref readonly CMenuPool Pool
    {
        get
        {
            if (!mainThread.IsMainThread) throw new InvalidOperationException("SA-MP menus require the game thread.");
            return ref CMenuPool.Instance;
        }
    }

    public ushort CurrentMenu => Pool.CurrentMenu;
    public bool Cancelled => Pool.Cancelled;

    public bool Exists(byte menuId) => Pool.DoesExist(menuId);

    public SFMenu Get(byte menuId)
    {
        return new(menuId, Pool.Get(menuId));
    }

    public bool TryGet(byte menuId, out SFMenu menu)
    {
        CMenu* nativeMenu = Pool.Get(menuId);
        if (nativeMenu == null || !Pool.DoesExist(menuId))
        {
            menu = null!;
            return false;
        }

        menu = new SFMenu(menuId, nativeMenu);
        return true;
    }

    public IEnumerable<byte> EnumerateIds()
    {
        return Pool.GetExistingIds().ToArray();
    }

    public IEnumerable<SFMenu> Enumerate()
    {
        foreach (byte menuId in EnumerateIds())
        {
            if (TryGet(menuId, out SFMenu menu))
            {
                yield return menu;
            }
        }
    }

    public SFMenu? GetCurrent()
    {
        return TryGet((byte)CurrentMenu, out SFMenu menu) ? menu : null;
    }

    public string? GetTextPointer(string name)
    {
        return Pool.GetTextPointer(name);
    }

    public void Show(byte menuId)
    {
        Pool.Show(menuId);
    }

    public void Hide(byte menuId)
    {
        Pool.Hide(menuId);
    }

    public bool Delete(byte menuId)
    {
        return Pool.Delete(menuId);
    }

    public void Process()
    {
        Pool.Process();
    }
}
