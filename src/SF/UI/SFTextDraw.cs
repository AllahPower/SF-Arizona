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

    public bool TryGetSnapshot(out SFTextDrawSnapshot snapshot)
    {
        if (!CTextDrawPool.Instance.TryGet(Id, out CTextDraw* textDraw))
        {
            snapshot = default;
            return false;
        }

        snapshot = new SFTextDrawSnapshot(
            textDraw->GetText(),
            textDraw->GetString(),
            new SFTextDrawDataSnapshot(
                textDraw->Data.LetterWidth,
                textDraw->Data.LetterHeight,
                textDraw->Data.LetterColor,
                textDraw->Data.Center != 0,
                textDraw->Data.Box != 0,
                textDraw->Data.BoxSizeX,
                textDraw->Data.BoxSizeY,
                textDraw->Data.BoxColor,
                textDraw->Data.Proportional != 0,
                textDraw->Data.BackgroundColor,
                textDraw->Data.Shadow,
                textDraw->Data.Outline,
                textDraw->Data.Left != 0,
                textDraw->Data.Right != 0,
                textDraw->Data.Style,
                textDraw->Data.X,
                textDraw->Data.Y,
                textDraw->Data.Field99B,
                textDraw->Data.Field99F,
                textDraw->Data.Index,
                textDraw->Data.Selectable != 0,
                textDraw->Data.Model,
                textDraw->Data.Rotation,
                textDraw->Data.Zoom,
                textDraw->Data.Color0,
                textDraw->Data.Color1,
                textDraw->Data.TextContainsKeys != 0,
                textDraw->Data.DrawnThisFrame != 0,
                textDraw->Data.IsSelected != 0,
                textDraw->Data.ComputedLeft,
                textDraw->Data.ComputedRight,
                textDraw->Data.ComputedTop,
                textDraw->Data.ComputedBottom,
                textDraw->Data.ColorIfSelected));
        return true;
    }
}
