using System.Numerics;

namespace SFSharp.Protocol.Rpc;

public static partial class SampRpc
{
    public static int ScoreClientMessagePayload(IncomingRpcArgs args)
    {
        ClientMessageRpc payload = ParseClientMessage(args);
        return ScoreText(payload.Text);
    }

    public static int ScoreChatMessagePayload(IncomingRpcArgs args)
    {
        ChatMessageRpc payload = ParseChatMessage(args);
        return ScoreText(payload.Prefix) + ScoreText(payload.Text);
    }

    private static int ScoreText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return int.MinValue;

        int score = 0;
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c)) { score += 4; continue; }
            if (c is ' ' or '\t' or '\r' or '\n') { score += 1; continue; }
            if (c is '{' or '}' or '[' or ']' or '(' or ')' or ':' or ';' or '.' or ',' or '!' or '?' or '-' or '+' or '/' or '\\' or '@' or '#' or '_' or '"' or '\'') { score += 2; continue; }
            if (char.IsControl(c)) { score -= 40; continue; }
            score -= 6;
        }
        return score;
    }

    private static MenuColumnRpc ReadMenuColumn(ref SampBitStreamReader reader, float width)
    {
        string title = reader.ReadFixedString(32);
        int rowCount = reader.ReadUInt8();
        List<string> text = new(rowCount);
        for (int i = 0; i < rowCount; i++)
        {
            text.Add(reader.ReadFixedString(32));
        }

        return new MenuColumnRpc
        {
            Title = title,
            Width = width,
            Text = text
        };
    }

    private static ObjectMaterialTextureRpc ReadObjectMaterialTexture(ref SampBitStreamReader reader)
    {
        return new ObjectMaterialTextureRpc
        {
            Type = ObjectMaterialType.Texture,
            MaterialId = reader.ReadUInt8(),
            ModelId = reader.ReadUInt16(),
            LibraryName = reader.ReadStringUInt8Length(),
            TextureName = reader.ReadStringUInt8Length(),
            Color = reader.ReadInt32()
        };
    }

    private static ObjectMaterialTextRpc ReadObjectMaterialText(ref SampBitStreamReader reader)
    {
        return new ObjectMaterialTextRpc
        {
            Type = ObjectMaterialType.Text,
            MaterialId = reader.ReadUInt8(),
            MaterialSize = reader.ReadUInt8(),
            FontName = reader.ReadStringUInt8Length(),
            FontSize = reader.ReadUInt8(),
            Bold = reader.ReadUInt8(),
            FontColor = reader.ReadInt32(),
            BackgroundColor = reader.ReadInt32(),
            Align = reader.ReadUInt8(),
            Text = reader.ReadEncodedString(2048)
        };
    }

    private static byte[] ReadUInt8Array(ref SampBitStreamReader reader, int count)
    {
        byte[] values = new byte[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = reader.ReadUInt8();
        }

        return values;
    }

    private static Vector2 ReadVector2(ref SampBitStreamReader reader)
    {
        return new Vector2(reader.ReadFloat(), reader.ReadFloat());
    }

    private static Vector3 ReadVector3(ref SampBitStreamReader reader)
    {
        return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
    }

}
