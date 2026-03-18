using System.Numerics;

namespace SFSharp;

public readonly record struct ClientMessageRpc(uint Color, string Text);
public readonly record struct ChatMessageRpc(string Prefix, uint PrefixColor, string Text);
public readonly record struct SetCheckpointRpc(Vector3 Position, Vector3 Size);
public readonly record struct SetRaceCheckpointRpc(byte Type, Vector3 CurrentPosition, Vector3 NextPosition, float Size);
public readonly record struct DeathMessageRpc(ushort KillerPlayerId, ushort KilledPlayerId, byte WeaponId);
public readonly record struct ShowDialogRpcHeader(ushort DialogId, DialogStyle Style, string Title, string LeftButton, string RightButton, byte[] EncodedTextPayload);

public static class SampRpc
{
    public static int HandleRpcPacketOffset => SampOffsets.RpcRuntime.HandleRpcPacket;

    public static ClientMessageRpc ParseClientMessage(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        uint color = reader.ReadUInt32();
        string text = reader.ReadStringUInt32Length();
        return new ClientMessageRpc(color, text);
    }

    public static ChatMessageRpc ParseChatMessage(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        string prefix = reader.ReadStringUInt8Length();
        uint prefixColor = reader.ReadUInt32();
        string text = reader.ReadStringUInt32Length();
        return new ChatMessageRpc(prefix, prefixColor, text);
    }

    public static SetCheckpointRpc ParseSetCheckpoint(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        Vector3 position = ReadVector3(ref reader);
        Vector3 size = ReadVector3(ref reader);
        return new SetCheckpointRpc(position, size);
    }

    public static SetRaceCheckpointRpc ParseSetRaceCheckpoint(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        byte type = reader.ReadUInt8();
        Vector3 currentPosition = ReadVector3(ref reader);
        Vector3 nextPosition = ReadVector3(ref reader);
        float size = reader.ReadFloat();
        return new SetRaceCheckpointRpc(type, currentPosition, nextPosition, size);
    }

    public static DeathMessageRpc ParseDeathMessage(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        ushort killerPlayerId = reader.ReadUInt16();
        ushort killedPlayerId = reader.ReadUInt16();
        byte weaponId = reader.ReadUInt8();
        return new DeathMessageRpc(killerPlayerId, killedPlayerId, weaponId);
    }

    public static ShowDialogRpcHeader ParseShowDialogHeader(IncomingRpcArgs args)
    {
        BitStreamReader reader = args.CreateReader();
        ushort dialogId = reader.ReadUInt16();
        DialogStyle style = (DialogStyle)reader.ReadUInt8();
        string title = reader.ReadStringUInt8Length();
        string leftButton = reader.ReadStringUInt8Length();
        string rightButton = reader.ReadStringUInt8Length();
        byte[] encodedTextPayload = reader.ReadRemainingBytes();
        return new ShowDialogRpcHeader(dialogId, style, title, leftButton, rightButton, encodedTextPayload);
    }

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
        {
            return int.MinValue;
        }

        int score = 0;
        foreach (char c in text)
        {
            if (char.IsLetterOrDigit(c))
            {
                score += 4;
                continue;
            }

            if (c is ' ' or '\t' or '\r' or '\n')
            {
                score += 1;
                continue;
            }

            if (c is '{' or '}' or '[' or ']' or '(' or ')' or ':' or ';' or '.' or ',' or '!' or '?' or '-' or '+' or '/' or '\\' or '@' or '#' or '_' or '"' or '\'')
            {
                score += 2;
                continue;
            }

            if (char.IsControl(c))
            {
                score -= 40;
                continue;
            }

            score -= 6;
        }

        return score;
    }

    private static Vector3 ReadVector3(ref BitStreamReader reader)
    {
        return new Vector3(reader.ReadFloat(), reader.ReadFloat(), reader.ReadFloat());
    }
}
