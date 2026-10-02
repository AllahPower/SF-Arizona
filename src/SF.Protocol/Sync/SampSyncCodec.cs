using System.Numerics;

namespace SFSharp.Protocol.Sync;

// SA-MP 0.3.7 R3-1 sync packet structures.
// Reference: samp.dll packet readers and SAMP.Lua synchronization.lua.
// All sync packets are prefixed with a RakNetPacketId byte which is NOT included in these structs.

internal static class SampSyncCodec
{
    private const float QuaternionScale = 1.0f / 65535.0f;
    private const float VectorComponentScale = 2.0f / 65535.0f;

    public static ushort ReadOptionalUInt16(ref SampBitStreamReader r)
    {
        return r.ReadBitBool() ? r.ReadUInt16() : (ushort)0;
    }

    public static uint ReadOptionalUInt32(ref SampBitStreamReader r)
    {
        return r.ReadBitBool() ? r.ReadUInt32() : 0u;
    }

    public static void ReadPackedHealthArmor(byte raw, out byte health, out byte armor)
    {
        armor = ExpandNibble((byte)(raw & 0x0F));
        health = ExpandNibble((byte)(raw >> 4));
    }

    public static byte ExpandNibble(byte nibble)
    {
        if (nibble == 0x0F)
        {
            return 100;
        }

        if (nibble == 0)
        {
            return 0;
        }

        return (byte)(nibble * 7);
    }

    public static (float W, float X, float Y, float Z) ReadCompressedQuaternion(ref SampBitStreamReader r)
    {
        bool wPositive = r.ReadBitBool();
        bool xPositive = r.ReadBitBool();
        bool yPositive = r.ReadBitBool();
        bool zPositive = r.ReadBitBool();

        ushort xRaw = r.ReadUInt16();
        ushort yRaw = r.ReadUInt16();
        ushort zRaw = r.ReadUInt16();

        float x = xRaw * QuaternionScale;
        float y = yRaw * QuaternionScale;
        float z = zRaw * QuaternionScale;

        if (!xPositive)
        {
            x = -x;
        }

        if (!yPositive)
        {
            y = -y;
        }

        if (!zPositive)
        {
            z = -z;
        }

        float wSquared = 1.0f - (x * x) - (y * y) - (z * z);
        if (wSquared < 0.0f)
        {
            wSquared = 0.0f;
        }

        float w = MathF.Sqrt(wSquared);
        if (!wPositive)
        {
            w = -w;
        }

        return (w, x, y, z);
    }

    public static Vector3 ReadCompressedVector(ref SampBitStreamReader r)
    {
        float magnitude = r.ReadFloat();
        if (magnitude <= 0.00001f)
        {
            return Vector3.Zero;
        }

        float x = (r.ReadUInt16() * VectorComponentScale) - 1.0f;
        float y = (r.ReadUInt16() * VectorComponentScale) - 1.0f;
        float z = (r.ReadUInt16() * VectorComponentScale) - 1.0f;

        return new Vector3(x * magnitude, y * magnitude, z * magnitude);
    }
}
