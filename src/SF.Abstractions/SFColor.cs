using System.Globalization;

namespace SFSharp.Abstractions;

public readonly record struct SFColor(uint Argb)
{
    public byte A => (byte)(Argb >> 24);
    public byte R => (byte)(Argb >> 16);
    public byte G => (byte)(Argb >> 8);
    public byte B => (byte)Argb;

    public uint Rgba => ((uint)R << 24) | ((uint)G << 16) | ((uint)B << 8) | A;
    public string SampTag => $"{{{R:X2}{G:X2}{B:X2}}}";

    public static SFColor FromArgb(byte a, byte r, byte g, byte b)
    {
        return new(((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | b);
    }

    public static SFColor FromRgb(byte r, byte g, byte b)
    {
        return FromArgb(0xFF, r, g, b);
    }

    public static SFColor FromHex(string hex)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hex);

        string normalized = hex.Trim().TrimStart('#').Trim('{', '}');
        return normalized.Length switch
        {
            6 => FromRgb(
                ParseByte(normalized, 0),
                ParseByte(normalized, 2),
                ParseByte(normalized, 4)),
            8 => FromArgb(
                ParseByte(normalized, 0),
                ParseByte(normalized, 2),
                ParseByte(normalized, 4),
                ParseByte(normalized, 6)),
            _ => throw new FormatException($"Unsupported color format '{hex}'. Use RRGGBB or AARRGGBB.")
        };
    }

    public SFColor WithAlpha(byte alpha)
    {
        return FromArgb(alpha, R, G, B);
    }

    public SFColor Blend(SFColor other, float weight = 0.5f)
    {
        float clamped = Math.Clamp(weight, 0f, 1f);
        return FromArgb(
            Lerp(A, other.A, clamped),
            Lerp(R, other.R, clamped),
            Lerp(G, other.G, clamped),
            Lerp(B, other.B, clamped));
    }

    public string Apply(string text)
    {
        return $"{SampTag}{text}";
    }

    public override string ToString()
    {
        return $"#{Argb:X8}";
    }

    public static SFColor operator |(SFColor left, SFColor right)
    {
        return left.Blend(right, 0.5f);
    }

    public static implicit operator uint(SFColor color)
    {
        return color.Argb;
    }

    private static byte ParseByte(string value, int startIndex)
    {
        return byte.Parse(value.AsSpan(startIndex, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    private static byte Lerp(byte from, byte to, float amount)
    {
        return (byte)Math.Clamp((int)MathF.Round(from + ((to - from) * amount)), 0, 255);
    }
}

public static class SFColors
{
    public static SFColor White => SFColor.FromRgb(0xFF, 0xFF, 0xFF);
    public static SFColor Black => SFColor.FromRgb(0x00, 0x00, 0x00);
    public static SFColor Gray => SFColor.FromRgb(0xAA, 0xAA, 0xAA);
    public static SFColor LightGray => SFColor.FromRgb(0xE0, 0xE0, 0xE0);
    public static SFColor DarkGray => SFColor.FromRgb(0x66, 0x66, 0x66);

    public static SFColor Red => SFColor.FromRgb(0xFF, 0x52, 0x52);
    public static SFColor Green => SFColor.FromRgb(0x00, 0xC8, 0x53);
    public static SFColor Blue => SFColor.FromRgb(0x29, 0xB6, 0xF6);
    public static SFColor Yellow => SFColor.FromRgb(0xF4, 0xD3, 0x5E);
    public static SFColor Orange => SFColor.FromRgb(0xFF, 0xB3, 0x00);
    public static SFColor Cyan => SFColor.FromRgb(0x8E, 0xCA, 0xE6);
    public static SFColor Purple => SFColor.FromRgb(0x7E, 0x57, 0xC2);

    public static SFColor Mint => SFColor.FromRgb(0x26, 0xA6, 0x9A);
    public static SFColor Ice => SFColor.FromRgb(0xE0, 0xFB, 0xFC);
    public static SFColor Rose => SFColor.FromRgb(0xFF, 0x6B, 0x6B);
    public static SFColor Sand => SFColor.FromRgb(0xE9, 0xC4, 0x6A);
    public static SFColor Slate => SFColor.FromRgb(0xB0, 0xBE, 0xC5);
}
