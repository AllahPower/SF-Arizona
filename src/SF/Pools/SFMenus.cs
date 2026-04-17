namespace SFSharp.Runtime.Game;

public sealed unsafe class SFMenus : ISFMenus
{
    public ushort CurrentMenu => CMenuPool.Instance.CurrentMenu;
    public bool Cancelled => CMenuPool.Instance.Cancelled;

    public bool Exists(byte menuId) => CMenuPool.Instance.DoesExist(menuId);

    public SFMenu Get(byte menuId)
    {
        return new(menuId, CMenuPool.Instance.Get(menuId));
    }

    public bool TryGet(byte menuId, out SFMenu menu)
    {
        CMenu* nativeMenu = CMenuPool.Instance.Get(menuId);
        if (nativeMenu == null || !CMenuPool.Instance.DoesExist(menuId))
        {
            menu = null!;
            return false;
        }

        menu = new SFMenu(menuId, nativeMenu);
        return true;
    }

    public IEnumerable<byte> EnumerateIds()
    {
        foreach (byte menuId in CMenuPool.Instance.GetExistingIds())
        {
            yield return menuId;
        }
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
        return CMenuPool.Instance.GetTextPointer(name);
    }

    public void Show(byte menuId)
    {
        CMenuPool.Instance.Show(menuId);
    }

    public void Hide(byte menuId)
    {
        CMenuPool.Instance.Hide(menuId);
    }

    public bool Delete(byte menuId)
    {
        return CMenuPool.Instance.Delete(menuId);
    }

    public void Process()
    {
        CMenuPool.Instance.Process();
    }
}
