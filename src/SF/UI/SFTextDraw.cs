namespace SFSharp;

public sealed unsafe class SFTextDraw
{
    private readonly CTextDraw* _native;

    internal SFTextDraw(ushort id, CTextDraw* native)
    {
        Id = id;
        _native = native;
    }

    public ushort Id { get; }
    public nint NativePointer => (nint)_native;
    public bool Exists => _native != null && CTextDrawPool.Instance.DoesExist(Id);
    public ref readonly CTextDrawData Data => ref _native->Data;
    public float X => _native == null ? 0f : _native->Data.X;
    public float Y => _native == null ? 0f : _native->Data.Y;
    public int Style => _native == null ? 0 : _native->Data.Style;
    public ushort Model => _native == null ? (ushort)0 : _native->Data.Model;
    public float Zoom => _native == null ? 0f : _native->Data.Zoom;
    public bool Selectable => _native != null && _native->Data.Selectable != 0;

    public string? Text
    {
        get
        {
            if (_native == null)
            {
                return null;
            }

            return _native->GetText();
        }
    }

    public string? String
    {
        get
        {
            if (_native == null)
            {
                return null;
            }

            return _native->GetString();
        }
    }

    public void SetText(string text)
    {
        if (_native != null)
        {
            _native->SetText(text);
        }
    }

    public void Draw()
    {
        if (_native != null)
        {
            _native->Draw();
        }
    }
}
