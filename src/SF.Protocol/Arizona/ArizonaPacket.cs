using System.Numerics;
using System.Text;

namespace SFSharp.Protocol.Arizona;

public static partial class ArizonaPacket
{
    // ---- helpers ----

    private static Vector3 ReadVec3(ref SampBitStreamReader r)
    {
        return new Vector3(r.ReadFloat(), r.ReadFloat(), r.ReadFloat());
    }

    // read remaining bytes as string (cp1251)
    private static string ReadStringRemaining(ref SampBitStreamReader r)
    {
        int bytes = (r.RemainingBits + 7) / 8;
        return bytes > 0 ? r.ReadFixedString(bytes) : string.Empty;
    }

    // Arizona maybeEncoded format seen on Packet 220 CEF packets:
    // u16 length, i8 encoded_flag, then either plain bytes or encoded/compressed bytes.
    private static string ReadMaybeEncodedString(ref SampBitStreamReader r)
    {
        ushort decodedLength = r.ReadUInt16();
        byte encodedFlag = r.ReadUInt8();

        if (encodedFlag == 0)
        {
            return decodedLength > 0 ? r.ReadFixedString(decodedLength) : string.Empty;
        }

        int maxCharsToWrite = decodedLength + encodedFlag;
        return r.ReadEncodedString(maxCharsToWrite);
    }
}
