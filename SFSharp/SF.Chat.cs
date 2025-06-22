using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SFSharp;

public static partial class SF
{
    public static void SendChatMessage(string message) => SFCore.SendToChat(message);
    public static void SendChatMessage(SFChatInterpolatedStringHandler handler) => SFCore.SendToChat(handler.GetFormattedString());
    public static void AddChatMessage(string message) => SFCore.LogToChat(message);
    public static void AddChatMessage(SFChatInterpolatedStringHandler handler) => SFCore.LogToChat(handler.GetFormattedString());
}

[InterpolatedStringHandler]
public struct SFChatInterpolatedStringHandler
{
    StringBuilder _builder;

    public SFChatInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _builder = new(literalLength + formattedCount * 8);
    }

    public void AppendLiteral(string s)
    {
        _builder.Append(s);
    }

    public void AppendFormatted<T>(T value)
    {
        if (value is Color colorValue)
        {
            var rgb = (colorValue.ToArgb() & 0xFFFFFF).ToString("X6");

            _builder.Append('{');
            _builder.Append(rgb);
            _builder.Append('}');
            return;
        }
        else
        {
            _builder.Append(value);
        }
    }

    public string GetFormattedString() => _builder.ToString();
}