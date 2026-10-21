using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using unsafe CreateDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, byte, byte*, float, float, byte, float, float, SFSharp.Runtime.Interop.Classes.Pools.CMenuInteraction*, nint>;
using unsafe DeleteDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, byte, int>;
using unsafe GetTextPointerDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, byte*, byte*>;
using unsafe HideDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, byte, void>;
using unsafe ProcessDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, void>;
using unsafe ShowDelegate = delegate* unmanaged[Thiscall]<SFSharp.Runtime.Interop.Classes.Pools.CMenuPool*, byte, void>;

namespace SFSharp.Runtime.Interop.Classes.Pools;

[StructLayout(LayoutKind.Explicit, Size = 1026, Pack = 1)]
public unsafe ref struct CMenuPool
{
    private static CMenuPool* CurrentInstance => CNetGame.Instance.GetMenuPool();
    public static ref readonly CMenuPool Instance => ref *RequireInstance();

    [FieldOffset(SampOffsets.CMenuPool.CurrentMenu)]
    private readonly byte _currentMenu;

    [FieldOffset(SampOffsets.CMenuPool.Cancelled)]
    private readonly byte _cancelled;

    public ushort CurrentMenu => RequireInstance()->_currentMenu;
    public bool Cancelled => RequireInstance()->_cancelled != 0;

    private static readonly CreateDelegate _create = (CreateDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.Create);
    private static readonly DeleteDelegate _delete = (DeleteDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.Delete);
    private static readonly ShowDelegate _show = (ShowDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.Show);
    private static readonly HideDelegate _hide = (HideDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.Hide);
    private static readonly GetTextPointerDelegate _getTextPointer = (GetTextPointerDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.GetTextPointer);
    private static readonly ProcessDelegate _process = (ProcessDelegate)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenuPool.Process);

    public bool IsValidId(byte menuId) => menuId < SampOffsets.CMenuPool.MaxMenus;

    public CMenu* Get(byte menuId)
    {
        CMenuPool* instance = RequireInstance();
        if (!IsValidId(menuId))
        {
            return null;
        }

        return GetMenuArray(instance)[menuId];
    }

    public bool DoesExist(byte menuId)
    {
        CMenuPool* instance = RequireInstance();
        return IsValidId(menuId) && GetOccupancyArray(instance)[menuId] != 0;
    }

    public CMenu* Create(byte menuId, string title, float x, float y, byte columns, float firstColumnWidth, float secondColumnWidth, in CMenuInteraction interaction)
    {
        CMenuInteraction copy = interaction;
        using AnsiString titleAnsi = AnsiString.Encode(title);
        if (SampEnvironment.IsR5)
        {
            var create = (delegate* unmanaged[Thiscall]<CMenuPool*, byte, float, float, byte, float, float, CMenuInteraction*, CMenu*>)_create;
            CMenu* menu = create(RequireInstance(), menuId, x, y, columns, firstColumnWidth, secondColumnWidth, &copy);
            if (menu != null)
            {
                var setTitle = (delegate* unmanaged[Thiscall]<CMenu*, byte*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.SetTitle);
                setTitle(menu, titleAnsi);
            }
            return menu;
        }
        return (CMenu*)_create(RequireInstance(), menuId, titleAnsi, x, y, columns, firstColumnWidth, secondColumnWidth, &copy);
    }

    public bool Delete(byte menuId)
    {
        return _delete(RequireInstance(), menuId) != 0;
    }

    public void Show(byte menuId)
    {
        _show(RequireInstance(), menuId);
    }

    public void Hide(byte menuId)
    {
        _hide(RequireInstance(), menuId);
    }

    public string? GetTextPointer(string name)
    {
        using AnsiString nameAnsi = AnsiString.Encode(name);
        return AnsiString.Decode(_getTextPointer(RequireInstance(), nameAnsi));
    }

    public unsafe byte[] GetExistingIds()
    {
        CMenuPool* instance = RequireInstance();
        int* occupancy = GetOccupancyArray(instance);
        List<byte> ids = [];
        for (byte menuId = 0; menuId < SampOffsets.CMenuPool.MaxMenus; menuId++)
        {
            if (occupancy[menuId] != 0)
            {
                ids.Add(menuId);
            }
        }

        return [.. ids];
    }

    public void Process()
    {
        _process(RequireInstance());
    }

    private static CMenu** GetMenuArray(CMenuPool* instance)
    {
        return (CMenu**)((byte*)instance + SampOffsets.CMenuPool.MenuArray);
    }

    private static int* GetOccupancyArray(CMenuPool* instance)
    {
        return (int*)((byte*)instance + SampOffsets.CMenuPool.OccupancyArray);
    }

    private static CMenuPool* RequireInstance()
    {
        CMenuPool* instance = CurrentInstance;
        if (instance == null)
        {
            throw new InvalidOperationException("CMenuPool instance is not available.");
        }

        return instance;
    }
}

[StructLayout(LayoutKind.Explicit, Size = 952, Pack = 1)]
public unsafe struct CMenu
{
    [FieldOffset(0)]
    private readonly MenuR5 _r5;
    public const int MaxMenuItems = 12;
    public const int MaxColumns = 2;
    public const int MaxMenuLine = 32;

    [FieldOffset(0x00)]
    public byte Id;

    [FieldOffset(0x01)]
    public fixed byte Title[MaxMenuLine];

    [FieldOffset(0x21)]
    public fixed byte Items[MaxMenuItems * MaxColumns * MaxMenuLine];

    [FieldOffset(0x321)]
    public fixed byte Header[MaxColumns * MaxMenuLine];

    [FieldOffset(0x361)]
    private readonly float _x;
    public float X => SampEnvironment.IsR5 ? _r5.X : _x;

    [FieldOffset(0x365)]
    private readonly float _y;
    public float Y => SampEnvironment.IsR5 ? _r5.Y : _y;

    [FieldOffset(0x369)]
    private readonly float _firstColumnWidth;
    public float FirstColumnWidth => SampEnvironment.IsR5 ? _r5.FirstColumnWidth : _firstColumnWidth;

    [FieldOffset(0x36D)]
    private readonly float _secondColumnWidth;
    public float SecondColumnWidth => SampEnvironment.IsR5 ? _r5.SecondColumnWidth : _secondColumnWidth;

    [FieldOffset(0x371)]
    private readonly byte _columns;
    public byte Columns => SampEnvironment.IsR5 ? _r5.Columns : _columns;

    [FieldOffset(0x372)]
    private readonly CMenuInteraction _interaction;
    public CMenuInteraction Interaction => SampEnvironment.IsR5 ? _r5.Interaction : _interaction;

    [FieldOffset(0x3B2)]
    public fixed byte ColumnCount[MaxColumns];

    [FieldOffset(0x3B4)]
    private readonly int _panel;
    public int Panel => SampEnvironment.IsR5 ? _r5.Panel : _panel;

    public string? GetHeader(byte column)
    {
        if (column >= MaxColumns) return null;
        int line = SampEnvironment.IsR5 ? 33 : MaxMenuLine;
        byte* header = (byte*)Unsafe.AsPointer(ref this) + 1 + (1 + MaxMenuItems * MaxColumns + column) * line;
        return NativeString.Decode(header, line);
    }

    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*, void> _addItem = (delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.AddItem);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte, byte*, void> _setColumnTitle = (delegate* unmanaged[Thiscall]<CMenu*, byte, byte*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.SetColumnTitle);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, void> _hide = (delegate* unmanaged[Thiscall]<CMenu*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.Hide);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*> _getItem = (delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.GetItem);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte*> _getTitle = (delegate* unmanaged[Thiscall]<CMenu*, byte*>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.GetTitle);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*> _getString = (delegate* unmanaged[Thiscall]<CMenu*, byte, byte, byte*>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.GetString);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, byte> _getActiveRow = (delegate* unmanaged[Thiscall]<CMenu*, byte>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.GetActiveRow);
    private static readonly delegate* unmanaged[Thiscall]<CMenu*, void> _show = (delegate* unmanaged[Thiscall]<CMenu*, void>)ModuleResolver.GetProcAddress("samp.dll", SampOffsets.CMenu.Show);

    public void AddItem(byte column, byte row, string text)
    {
        using AnsiString textAnsi = AnsiString.Encode(text);
        _addItem((CMenu*)Unsafe.AsPointer(ref this), column, row, textAnsi);
    }

    public void SetColumnTitle(byte column, string text)
    {
        using AnsiString textAnsi = AnsiString.Encode(text);
        _setColumnTitle((CMenu*)Unsafe.AsPointer(ref this), column, textAnsi);
    }

    public void Hide()
    {
        _hide((CMenu*)Unsafe.AsPointer(ref this));
    }

    public string? GetItem(byte column, byte row)
    {
        return AnsiString.Decode(_getItem((CMenu*)Unsafe.AsPointer(ref this), column, row));
    }

    public string? GetTitle()
    {
        return AnsiString.Decode(_getTitle((CMenu*)Unsafe.AsPointer(ref this)));
    }

    public string? GetString(byte column, byte row)
    {
        return AnsiString.Decode(_getString((CMenu*)Unsafe.AsPointer(ref this), column, row));
    }

    public byte GetActiveRow()
    {
        return _getActiveRow((CMenu*)Unsafe.AsPointer(ref this));
    }

    public void Show()
    {
        _show((CMenu*)Unsafe.AsPointer(ref this));
    }
}

[StructLayout(LayoutKind.Sequential, Size = 64, Pack = 1)]
public unsafe struct CMenuInteraction
{
    public int Menu;
    public fixed byte RowMask[60];
}
