namespace SFSharp;

public sealed unsafe class SFMenu
{
    private readonly CMenu* _native;

    internal SFMenu(byte id, CMenu* native)
    {
        Id = id;
        _native = native;
    }

    public byte Id { get; }
    public nint NativePointer => (nint)_native;
    public bool Exists => _native != null && CMenuPool.Instance.DoesExist(Id);
    public float X => _native == null ? 0f : _native->X;
    public float Y => _native == null ? 0f : _native->Y;
    public float FirstColumnWidth => _native == null ? 0f : _native->FirstColumnWidth;
    public float SecondColumnWidth => _native == null ? 0f : _native->SecondColumnWidth;
    public byte Columns => _native == null ? (byte)0 : _native->Columns;
    public int Panel => _native == null ? 0 : _native->Panel;
    public byte ActiveRow => _native == null ? byte.MaxValue : _native->GetActiveRow();

    public string? Title
    {
        get
        {
            if (_native == null)
            {
                return null;
            }

            return NativeString.Decode(_native->Title, CMenu.MaxMenuLine);
        }
    }

    public string? GetHeader(byte column)
    {
        if (_native == null || column >= CMenu.MaxColumns)
        {
            return null;
        }

        return NativeString.Decode(_native->Header + (column * CMenu.MaxMenuLine), CMenu.MaxMenuLine);
    }

    public string? GetItem(byte column, byte row)
    {
        return _native == null ? null : _native->GetItem(column, row);
    }

    public void AddItem(byte column, byte row, string text)
    {
        if (_native != null)
        {
            _native->AddItem(column, row, text);
        }
    }

    public void SetColumnTitle(byte column, string text)
    {
        if (_native != null)
        {
            _native->SetColumnTitle(column, text);
        }
    }

    public void Show()
    {
        if (_native != null)
        {
            _native->Show();
        }
    }

    public void Hide()
    {
        if (_native != null)
        {
            _native->Hide();
        }
    }
}
